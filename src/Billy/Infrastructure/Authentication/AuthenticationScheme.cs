using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Billy.Infrastructure.Authentication
{
    public static class AuthenticationScheme
    {
        public const string BearerScheme = JwtBearerDefaults.AuthenticationScheme;
        public const string BasicScheme = "basic";
    }
}