namespace Tawqi3i.QrSanad.Sanad;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Tawqi3i.QrSanad.Common.Constants;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Sanad;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

/// <summary>
/// Calls POST SealedDoc on the MoDEE e-seal service with an IDM token.
/// </summary>
internal sealed class SanadClient : IQRSanadClient
{
    private const string SealedDocPath = "SealedDoc";

    private readonly IHttpClientFactory httpClientFactory;
    private readonly SanadTokenProvider tokenProvider;
    private readonly SanadSettings settings;
    private readonly ILogger<SanadClient> logger;

    public SanadClient(
        IHttpClientFactory httpClientFactory,
        SanadTokenProvider tokenProvider,
        SanadSettings settings,
        ILogger<SanadClient> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.tokenProvider = tokenProvider;
        this.settings = settings;
        this.logger = logger;
    }

    public async Task<Response<SealedDocViewModel>> GetSealedDocument(string qrCode, CancellationToken cancellationToken)
    {
        if (!this.settings.IsConfigured)
        {
            this.logger.LogError("Sanad is not configured. Set the Sanad URLs and credentials in appsettings.");
            return Unavailable();
        }

        try
        {
            var token = await this.tokenProvider.GetAccessToken(cancellationToken);
            if (token == null)
            {
                return Unavailable();
            }

            using var response = await this.PostSealedDoc(token, qrCode, cancellationToken);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return await this.ToResponse(response, cancellationToken);
            }

            // The cached token was revoked or expired early; get a new one and try once more.
            token = await this.tokenProvider.GetAccessToken(cancellationToken, rejectedToken: token);
            if (token == null)
            {
                return Unavailable();
            }

            using var retry = await this.PostSealedDoc(token, qrCode, cancellationToken);

            return await this.ToResponse(retry, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException
            || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            this.logger.LogError(exception, "Sanad e-seal request failed.");
            return Unavailable();
        }
    }

    private async Task<HttpResponseMessage> PostSealedDoc(string token, string qrCode, CancellationToken cancellationToken)
    {
        var client = this.httpClientFactory.CreateClient(SanadSettings.ESealClientName);

        using var request = new HttpRequestMessage(HttpMethod.Post, SealedDocPath)
        {
            Content = JsonContent.Create(new { qrCode })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await client.SendAsync(request, cancellationToken);
    }

    private async Task<Response<SealedDocViewModel>> ToResponse(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = SealedDocParser.Parse(body, (int)response.StatusCode);

        if (result == null)
        {
            this.logger.LogError("Sanad e-seal returned a non-JSON response with {StatusCode}.", (int)response.StatusCode);
            return Unavailable();
        }

        if (result.FileContents != null)
        {
            return Response<SealedDocViewModel>.Succeed(new SealedDocViewModel
            {
                SignedFile = result.FileContents,
                ContentType = result.ContentType
            });
        }

        this.logger.LogWarning(
            "Sanad e-seal returned no document. HttpStatus={HttpStatus}, Status={Status}, Message={Message}",
            (int)response.StatusCode,
            result.Status,
            result.Message);

        // 400 is an invalid or unknown QR code, worth showing to the user. Anything else is an
        // upstream fault, so its details stay in the log.
        return result.Status == (int)HttpStatusCode.BadRequest
            ? Response<SealedDocViewModel>.Fail(result.Message ?? ResponseMessages.QrCodeInvalid)
            : Unavailable();
    }

    private static Response<SealedDocViewModel> Unavailable() =>
        Response<SealedDocViewModel>.Fail(ResponseMessages.QRSanadUnavailable, HttpStatusCode.BadGateway);
}
