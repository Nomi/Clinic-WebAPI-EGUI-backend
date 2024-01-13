using EGUI_Stage2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EGUI_Stage2.Auxiliary
{
    public static class AuthService
    {
        public static JwtSecurityToken GetJWTToken(List<Claim> authClaims)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SomeRandomStringThatIsHighlySecure123321"));

            return new JwtSecurityToken(
                expires: DateTime.Now.AddDays(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256)
                );
        }
        public static async Task EnsureRolesCreated(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            List<string> roles = new List<string> { "Admin", "Patient", "Doctor" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            var adminUser = new AppUser
            {
                UserName = "Manager",
                Name= "Manager",
                Email = "manager@managingfirm.co",
                IsVerified = true,
            };
            string adminPassword = "Manager123!";
            var _existingAdminUser = await userManager.FindByEmailAsync("manager@managingfirm.co");


            if (_existingAdminUser == null && (await userManager.CreateAsync(adminUser, adminPassword)).Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
