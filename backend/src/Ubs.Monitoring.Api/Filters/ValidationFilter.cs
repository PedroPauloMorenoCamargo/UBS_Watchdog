using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ubs.Monitoring.Api.Filters;

/// <summary>
/// Global action filter that automatically validates request DTOs using FluentValidation.
/// Returns RFC7807 Problem Details on validation failure for consistent error responses.
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ValidationFilter> _logger;

    public ValidationFilter(IServiceProvider serviceProvider, ILogger<ValidationFilter> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var parameter in context.ActionDescriptor.Parameters)
        {
            if (!context.ActionArguments.TryGetValue(parameter.Name, out var argument) || argument == null)
                continue;

            var argumentType = argument.GetType();

            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            var validator = _serviceProvider.GetService(validatorType) as IValidator;

            if (validator == null)
            {
                _logger.LogDebug(
                    "No FluentValidation validator found for parameter '{ParameterName}' of type '{ParameterType}'. " +
                    "This is expected for endpoints without validation. If validation was expected, ensure the validator is registered in DI.",
                    parameter.Name,
                    argumentType.Name);
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var validationResult = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            if (!validationResult.IsValid)
            {
                var problemDetails = new ValidationProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7807#section-3.1",
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = context.HttpContext.Request.Path
                };

                foreach (var error in validationResult.Errors)
                {
                    if (!problemDetails.Errors.ContainsKey(error.PropertyName))
                        problemDetails.Errors[error.PropertyName] = new[] { error.ErrorMessage };
                    else
                    {
                        var existingErrors = problemDetails.Errors[error.PropertyName].ToList();
                        existingErrors.Add(error.ErrorMessage);
                        problemDetails.Errors[error.PropertyName] = existingErrors.ToArray();
                    }
                }

                context.Result = new BadRequestObjectResult(problemDetails);
                return;
            }
        }

        await next();
    }
}
