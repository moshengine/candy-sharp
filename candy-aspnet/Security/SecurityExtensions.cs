using Microsoft.AspNetCore.Builder;

namespace Candy.AspNet
{
    public static class SecurityExtensions
    {
        public static void UseSimpleCors(this WebApplication app)
        {
            app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        }
    }
}
