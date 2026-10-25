namespace Tawqi3i.QrSanad.WebApi.Controllers.V1;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Branches;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using Tawqi3i.QrSanad.Domain.Services.V1.Branches;
using Tawqi3i.QrSanad.Identity.Authorization;
using Tawqi3i.QrSanad.WebApi.Common;

/// <summary>
/// Branches are managed by the SuperAdmin only. Each branch's admin is created through
/// POST /api/v1/users with type "admin" and the branch id.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[ProducesResponseType(typeof(Response), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(Response), StatusCodes.Status403Forbidden)]
public class BranchesController : ControllerBase
{
    private readonly IBranchesService branchesService;
    private readonly IStringLocalizer<SharedResource> localizer;

    public BranchesController(IBranchesService branchesService, IStringLocalizer<SharedResource> localizer)
    {
        this.branchesService = branchesService;
        this.localizer = localizer;
    }

    [HttpGet, SuperAdminOnly]
    [ProducesResponseType(typeof(PagedResponse<BranchViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] BranchesQuery query, CancellationToken cancellationToken)
    {
        var result = await this.branchesService.GetAll(query, cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// The caller's own branch. Available to every signed-in admin and user.
    /// </summary>
    [HttpGet("current"), Authorize]
    [ProducesResponseType(typeof(Response<BranchViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var result = await this.branchesService.GetCurrent(this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    [HttpGet("{id:guid}"), SuperAdminOnly]
    [ProducesResponseType(typeof(Response<BranchViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await this.branchesService.Get(id, cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    [HttpPost, SuperAdminOnly]
    [ProducesResponseType(typeof(Response<BranchViewModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] BranchRequest request, CancellationToken cancellationToken)
    {
        var result = await this.branchesService.Create(request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    [HttpPut("{id:guid}"), SuperAdminOnly]
    [ProducesResponseType(typeof(Response<BranchViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] BranchRequest request, CancellationToken cancellationToken)
    {
        var result = await this.branchesService.Update(id, request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Disabling a branch locks out its admin and users immediately; activating lets them back in.
    /// </summary>
    [HttpPut("{id:guid}/status"), SuperAdminOnly]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(Guid id, [FromBody] BranchStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await this.branchesService.SetStatus(id, request, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }

    /// <summary>
    /// Soft-deletes the branch. Its admin and users can no longer sign in.
    /// </summary>
    [HttpDelete("{id:guid}"), SuperAdminOnly]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await this.branchesService.Delete(id, this.HttpContext.ExtractUser(), cancellationToken);

        return this.HttpContext.ToObjectResult(result, this.localizer);
    }
}
