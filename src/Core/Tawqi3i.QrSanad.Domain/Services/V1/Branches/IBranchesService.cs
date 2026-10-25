namespace Tawqi3i.QrSanad.Domain.Services.V1.Branches;

using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Branches;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

/// <summary>
/// Branch management is the SuperAdmin's alone; everyone else can only read their own branch.
/// </summary>
public interface IBranchesService
{
    Task<PagedResponse<BranchViewModel>> GetAll(BranchesQuery query, CancellationToken cancellationToken = default);

    Task<Response<BranchViewModel>> Get(Guid id, CancellationToken cancellationToken = default);

    Task<Response<BranchViewModel>> GetCurrent(RequestUser actor, CancellationToken cancellationToken = default);

    Task<Response<BranchViewModel>> Create(BranchRequest request, RequestUser actor, CancellationToken cancellationToken = default);

    Task<Response<BranchViewModel>> Update(Guid id, BranchRequest request, RequestUser actor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disabling locks out the branch's admin and users at once; activating lets them back in.
    /// </summary>
    Task<Response> SetStatus(Guid id, BranchStatusRequest request, RequestUser actor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft delete. The branch's admin and users can no longer sign in and their sessions end.
    /// </summary>
    Task<Response> Delete(Guid id, RequestUser actor, CancellationToken cancellationToken = default);
}
