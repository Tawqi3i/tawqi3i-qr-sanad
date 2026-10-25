namespace Tawqi3i.QrSanad.Domain.Services.V1.QRSanad;

using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.QRSanad;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

public interface IQRSanadService
{
    /// <summary>
    /// Fetches the sealed document for a scanned QR code and writes the scan, successful or not,
    /// to the security audit log of the caller's branch.
    /// </summary>
    Task<Response<SealedDocViewModel>> GetSealedDocument(SealedDocRequest request, RequestUser actor, CancellationToken cancellationToken = default);
}
