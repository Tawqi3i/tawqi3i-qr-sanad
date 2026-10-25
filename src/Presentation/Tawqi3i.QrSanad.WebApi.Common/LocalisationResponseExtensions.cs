namespace Tawqi3i.QrSanad.WebApi.Common;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;

public static class LocalisationResponseExtensions
{
    /// <summary>
    /// Writes a service response with its status code, translating the message into the
    /// request culture (Accept-Language: ar or en).
    /// </summary>
    public static ObjectResult ToObjectResult(this HttpContext httpContext, ResponseBase response, IStringLocalizer localizer)
    {
        response.Localise(localizer);

        return new ObjectResult(response)
        {
            StatusCode = response.StatusCode ?? StatusCodes.Status400BadRequest
        };
    }

    public static T Localise<T>(this T response, IStringLocalizer localizer)
        where T : ResponseBase
    {
        if (!string.IsNullOrWhiteSpace(response.Message))
        {
            response.Message = localizer[response.Message];
        }

        return response;
    }

    /// <summary>
    /// Model-validation failures in the same shape as every other error. The individual
    /// messages are already localised by AddDataAnnotationsLocalization.
    /// </summary>
    public static BadRequestObjectResult ToValidationResult(this ActionContext context, IStringLocalizer localizer)
    {
        var errors = context.ModelState
            .Where(a => a.Value is { ValidationState: ModelValidationState.Invalid })
            .ToDictionary(
                a => a.Key,
                a => a.Value!.Errors.Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? localizer[ResponseMessages.ValidationFailed].Value : e.ErrorMessage).ToArray());

        var response = new ValidationResponse
        {
            Message = localizer[ResponseMessages.ValidationFailed],
            StatusCode = StatusCodes.Status400BadRequest,
            Errors = errors
        };

        return new BadRequestObjectResult(response);
    }
}
