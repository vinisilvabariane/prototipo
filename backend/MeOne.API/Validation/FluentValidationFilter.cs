using FluentValidation;
using MeOne.API.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MeOne.API.Validation;

public sealed class FluentValidationFilter(IServiceProvider services) : IAsyncActionFilter
{
    public const string Title = "Falha de validação";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new List<string>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

            if (services.GetService(validatorType) is not IValidator validator)
                continue;

            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument),
                context.HttpContext.RequestAborted);

            errors.AddRange(result.Errors.Select(error => error.ErrorMessage));
        }

        if (errors.Count > 0)
        {
            context.Result = new BadRequestObjectResult(ErrorResponse.From(Title, errors));
            return;
        }

        await next();
    }
}
