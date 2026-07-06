using System;
using api.Models.ViewModels.Auth;
using api.Models;

namespace api.Models.Mapping;

public static class Class
{
  public static AppUser ToEntity(this RegisterVM payload)
  {
    return new()
    {
      FirstName = payload.FirstName,
      LastName = payload.LastName,
      UserName = payload.UserName,
      Email = payload.Email,
    };
  }

}
