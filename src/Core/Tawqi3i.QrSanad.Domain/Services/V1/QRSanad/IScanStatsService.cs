namespace Tawqi3i.QrSanad.Domain.Services.V1.QRSanad;

using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.QRSanad;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

public interface IScanStatsService
{
    /// <summary>
    /// QR scan counts from the audit log, scoped like the audit log: the SuperAdmin sees every
    /// branch, everyone else their own.
    /// </summary>
    Task<Response<ScanStatsViewModel>> Get(ScanStatsQuery query, RequestUser actor, CancellationToken cancellationToken = default);
}
