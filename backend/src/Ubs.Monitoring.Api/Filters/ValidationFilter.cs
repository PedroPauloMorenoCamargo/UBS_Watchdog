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

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Iterate through all action parameters
        foreach (var parameter in context.ActionDescriptor.Parameters)
        {
            if (!context.ActionArguments.TryGetValue(parameter.Name, out var argument) || argument == null)
                continue;

            var argumentType = argument.GetType();

            // Try to get validator for this parameter type
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            var validator = _serviceProvider.GetService(validatorType) as IValidator;

            if (validator == null)
                continue;

            // Validate the argument
            var validationContext = new ValidationContext<object>(argument);
            var validationResult = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            if (!validationResult.IsValid)
            {
                // Convert validation errors to Problem Details format
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
