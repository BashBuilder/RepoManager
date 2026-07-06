using api.Models;
using api.Models.ViewModels.Auth;

namespace api.Services;

public interface ITokenService
{
  Task<TokenResponseVM> GenerateTokenAsync(AppUser user, string? existingRefreshToken = null);
  Task<TokenResponseVM?> VerifyAndValidateTokenAsync(RefreshTokenVM payload);
}