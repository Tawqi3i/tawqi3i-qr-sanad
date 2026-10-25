namespace Tawqi3i.QrSanad.WebApi.Middlewares;

using System.Net;
using Microsoft.Extensions.Localization;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Helpers;
using Tawqi3i.QrSanad.Common.Persistence;
using Tawqi3i.QrSanad.WebApi.Common;

/// <summary>
/// Turns exceptions into the standard response shape. Nothing about the exception itself
/// reaches the client; it is logged instead.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ErrorHandlingMiddleware> logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IStringLocalizer<SharedResource> localizer)
    {
        try
        {
            await this.next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client went away; there is nobody to answer.
            this.logger.LogDebug("Request {Path} was cancelled by the client.", context.Request.Path);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var response = this.ToResponse(exception);

            context.Response.Clear();
            context.Response.StatusCode = response.StatusCode ?? StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(response.Localise(localizer), context.RequestAborted);
        }
    }

    private Response ToResponse(Exception exception)
    {
        switch (exception)
        {
            case AppException appException:
                this.logger.LogInformation("Request aborted with {StatusCode}: {Message}", appException.Response.StatusCode, appException.Message);
                return appException.Response;

            case PersistenceException { Code: ErrorCodes.DuplicateKey } persistence:
                this.logger.LogWarning(persistence, "Unique constraint violated.");
                return Response.Conflict(ResponseMessages.DuplicateRecord);

            case PersistenceException { Code: ErrorCodes.ConcurrencyConflict } persistence:
                this.logger.LogWarning(persistence, "Optimistic concurrency conflict.");
                return Response.Conflict(ResponseMessages.ConcurrencyConflict);

            case BadHttpRequestException badRequest:
                this.logger.LogInformation("Bad request: {Message}", badRequest.Message);
                return Response.Fail(ResponseMessages.ValidationFailed, (HttpStatusCode)badRequest.StatusCode);

            default:
                this.logger.LogError(exception, "Unhandled exception.");
                return Response.Fail(ResponseMessages.SomethingWentWrong, HttpStatusCode.InternalServerError);
        }
    }
}
