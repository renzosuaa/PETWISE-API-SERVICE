namespace PetWise_Application.Common.Interfaces;

public record AuthResult(Guid UserId, string Email, string AccessToken);

public interface IAuthService
{
    Task<AuthResult?> SignUpAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<AuthResult?> SignInAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<AuthResult?> GoogleSignInAsync(string idToken, CancellationToken cancellationToken = default);
}