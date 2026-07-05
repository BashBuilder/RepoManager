using System;
using api.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Data;

public static class AppDbInitializer
{
    public static async Task SeedRolesAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync(RolesVM.Admin)) await roleManager.CreateAsync(new IdentityRole(RolesVM.Admin));
        if (!await roleManager.RoleExistsAsync(RolesVM.Player)) await roleManager.CreateAsync(new IdentityRole(RolesVM.Player));
        if (!await roleManager.RoleExistsAsync(RolesVM.User)) await roleManager.CreateAsync(new IdentityRole(RolesVM.User));
    }


    public static async Task SeedGenreAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();
    }


}
