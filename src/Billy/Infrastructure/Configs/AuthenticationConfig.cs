namespace Billy.Infrastructure.Configs
{
    public class AuthenticationConfig
    {
        public Jwt Jwt { get; set; }
        public Basic Basic { get; set; }

    }

    public class Basic
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class Jwt
    {
        public string Authority { get; set; }
        public bool RequireHttpsMetadata { get; set; }
        public bool SaveToken { get; set; }
        public string UserIdClaimName { get; set; }
        public TokenValidationParameters TokenValidationParameters { get; set; }
    }
    
    public class TokenValidationParameters
    {
        public bool ValidateIssuer { get; set; }
        public bool ValidateLifetime { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }
        public bool ValidateAudience { get; set; }
        
        public string ValidAudience { get; set; }
    }
}