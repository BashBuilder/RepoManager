using System;

namespace api.Models.ViewModels.Auth;

public record class TokenResponseVM(
  string AccessToken,
  string RefreshToken,
  DateTime ExpirationDate
);

