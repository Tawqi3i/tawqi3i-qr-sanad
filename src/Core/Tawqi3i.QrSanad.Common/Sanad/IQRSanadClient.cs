namespace Tawqi3i.QrSanad.Common.Sanad;

using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

/// <summary>
/// Client for the MoDEE (Sanad) e-seal service.
/// </summary>
public interface IQRSanadClient
{
    /// <summary>
    /// The MoDEE e-sealed document (base64 PAdES-T PDF) for a SANAD digital document QR code.
    /// </summary>
    Task<Response<SealedDocViewModel>> GetSealedDocument(string qrCode, CancellationToken cancellationToken);
}
