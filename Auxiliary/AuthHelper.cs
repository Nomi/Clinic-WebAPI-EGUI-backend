using EGUI_Stage2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EGUI_Stage2.Auxiliary
{
    public static class AuthHelper
    {
        public static string JwtSecretConfigAccessor=> "JWT:Secret";
        public static string JwtValidIssuerConfigAccessor=> "JWT:ValidIssuer";
        public static string JwtValidAudienceConfigAccessor=> "JWT:ValidAudience";
        public static JwtSecurityToken GetToken(List<Claim> authClaims, IConfiguration config)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config[JwtSecretConfigAccessor]));

            var token = new JwtSecurityToken(
                issuer: config[JwtValidAudienceConfigAccessor],
                audience: config[JwtValidAudienceConfigAccessor],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
        public static async Task CreateRoles(IServiceProvider scopedServiceProvider)
        {
            //initializing custom roles 
            var roleManager = scopedServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scopedServiceProvider.GetRequiredService<UserManager<User>>();
            string[] roleNames = { UserRoles.admin, UserRoles.doctor, UserRoles.patient, UserRoles.unverifiedPatient };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    //create the roles and seed them to the database: Question 1
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            //Here you could create a super user who will maintain the web app
            var poweruser = new User
            {
                UserName = "admin",
                Email = "admin@admin.admin",
                IsVerified = true,
            };
            //Ensure you have these values in your appsettings.json file
            string userPWD = "Admin@123";
            var _user = await userManager.FindByEmailAsync("admin@admin.admin");


            if (_user == null)
            {
                var createPowerUser = await userManager.CreateAsync(poweruser, userPWD);
                if (createPowerUser.Succeeded)
                {
                    //here we tie the new user to the role
                    await userManager.AddToRoleAsync(poweruser, UserRoles.admin);

                }
            }
        }
    }
}
