namespace Tawqi3i.QrSanad.Domain.Services.V1.QRSanad;

using Microsoft.EntityFrameworkCore;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.QRSanad;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.Enums;
using Tawqi3i.QrSanad.Common.Helpers;
using Tawqi3i.QrSanad.Common.Identity;
using Tawqi3i.QrSanad.Common.Licensing;
using Tawqi3i.QrSanad.Common.Persistence;
using Tawqi3i.QrSanad.Common.Sanad;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

internal sealed class QRSanadService : IQRSanadService
{
    private const int MaxLoggedQrCodeLength = 1024;

    private readonly IQRSanadClient qrSanadClient;
    private readonly IUsersRepository usersRepository;
    private readonly ISecurityAuditService auditService;
    private readonly IUnitOfWork unitOfWork;
    private readonly AccessGuard accessGuard;
    private readonly ILicenseService licenseService;

    public QRSanadService(
        IQRSanadClient qrSanadClient,
        IUsersRepository usersRepository,
        ISecurityAuditService auditService,
        IUnitOfWork unitOfWork,
        AccessGuard accessGuard,
        ILicenseService licenseService)
    {
        this.qrSanadClient = qrSanadClient;
        this.usersRepository = usersRepository;
        this.auditService = auditService;
        this.unitOfWork = unitOfWork;
        this.accessGuard = accessGuard;
        this.licenseService = licenseService;
    }

    public async Task<Response<SealedDocViewModel>> GetSealedDocument(SealedDocRequest request, RequestUser actor, CancellationToken cancellationToken = default)
    {
        // Refused before anything reaches MoDEE, so it is not a scan and is not audited.
        if (await this.licenseService.EnsureActive(cancellationToken) is { } licenseDenied)
        {
            return Response<SealedDocViewModel>.From(licenseDenied);
        }

        var actorAccess = await this.accessGuard.GetActorAccess(actor, cancellationToken);
        var userName = await this.usersRepository.GetQueryable()
            .Where(a => a.Id == actor.Id)
            .Select(a => a.UserName)
            .FirstOrDefaultAsync(cancellationToken);

        var result = await this.qrSanadClient.GetSealedDocument(request.QrCode, cancellationToken);

        var qrCode = Normalizer.Truncate(request.QrCode, MaxLoggedQrCodeLength);
        var details = result.Success
            ? $"QR: {qrCode}"
            : $"{result.StatusCode} {result.Message} | QR: {qrCode}";

        this.auditService.Record(
            result.Success ? SecurityEventType.QrScanSucceeded : SecurityEventType.QrScanFailed,
            actor,
            actor.Id,
            userName,
            branchId: actorAccess.BranchId,
            details: details);

        // MoDEE has already been called, so the scan is recorded even if the caller disconnects now.
        await this.unitOfWork.SaveChanges(CancellationToken.None);

        return result;
    }
}
