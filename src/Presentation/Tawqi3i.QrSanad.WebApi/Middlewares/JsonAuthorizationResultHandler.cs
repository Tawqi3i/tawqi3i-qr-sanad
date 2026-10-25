namespace Tawqi3i.QrSanad.WebApi.Middlewares;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Localization;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.WebApi.Common;

/// <summary>
/// 401 and 403 with a JSON body in the standard response shape, instead of an empty response.
/// A 403 carries the reason a handler gave, e.g. "you must change your password".
/// </summary>
internal sealed class JsonAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged)
        {
            // Lets JwtBearer add its WWW-Authenticate header before the body is written.
            if (policy.AuthenticationSchemes.Count > 0)
            {
                foreach (var scheme in policy.AuthenticationSchemes)
                {
                    await context.ChallengeAsync(scheme);
                }
            }
            else
            {
                await context.ChallengeAsync();
            }

            await Write(context, Response.Unauthorized(), StatusCodes.Status401Unauthorized);
            return;
        }

        if (authorizeResult.Forbidden)
        {
            var reason = authorizeResult.AuthorizationFailure?.FailureReasons.FirstOrDefault()?.Message;
            await Write(context, Response.Forbidden(reason), StatusCodes.Status403Forbidden);
            return;
        }

        await this.defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }

    private static async Task Write(HttpContext context, Response response, int statusCode)
    {
        var localizer = context.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(response.Localise(localizer), context.RequestAborted);
    }
}
