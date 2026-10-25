namespace Tawqi3i.QrSanad.Common.Identity;

using Tawqi3i.QrSanad.Common.Dto.V1.Requests;
using Tawqi3i.QrSanad.Common.Dto.V1.Requests.Auth;
using Tawqi3i.QrSanad.Common.Dto.V1.Responses;
using Tawqi3i.QrSanad.Common.ViewModels.V1;

public interface IAuthService
{
    Task<Response<TokenResponse>> Login(LoginRequest request, ClientInfo client, CancellationToken cancellationToken = default);

    Task<Response<TokenResponse>> Refresh(RefreshTokenRequest request, ClientInfo client, CancellationToken cancellationToken = default);

    Task<Response> Logout(RefreshTokenRequest request, RequestUser user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends every session of the caller, on every device.
    /// </summary>
    Task<Response> LogoutAll(RequestUser user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends every other session and returns fresh tokens for this one.
    /// </summary>
    Task<Response<TokenResponse>> ChangePassword(ChangePasswordRequest request, RequestUser user, CancellationToken cancellationToken = default);

    Task<Response<ProfileViewModel>> GetProfile(RequestUser user, CancellationToken cancellationToken = default);
}
