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

            // Skip validation for primitive types and known non-validatable types
            if (ShouldSkipValidation(argumentType))
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            var validator = _serviceProvider.GetService(validatorType) as IValidator;

            if (validator == null)
            {
                // Only log warnings for request DTOs that likely should have validators
                if (IsRequestDto(argumentType))
                {
                    _logger.LogWarning(
                        "No validator registered for '{ParameterType}'. Validation skipped. " +
                        "If validation is required, ensure a validator is registered in DI.",
                        argumentType.Name);
                }
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

    /// <summary>
    /// Determines if a type should skip validation entirely (primitive types, Guids, etc.).
    /// </summary>
    private static bool ShouldSkipValidation(Type type)
    {
        return type.IsPrimitive ||
               type == typeof(string) ||
               type == typeof(Guid) ||
               type == typeof(DateTime) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(TimeSpan) ||
               type.IsEnum;
    }

    /// <summary>
    /// Determines if a type is a request DTO that should have a validator.
    /// Checks for Application namespace and common DTO naming patterns.
    /// </summary>
    private static bool IsRequestDto(Type type)
    {
        var ns = type.Namespace ?? string.Empty;
        var name = type.Name;

        return ns.Contains("Application", StringComparison.OrdinalIgnoreCase) &&
               (name.EndsWith("Request", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("Command", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("Query", StringComparison.OrdinalIgnoreCase));
    }
}
