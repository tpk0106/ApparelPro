namespace apparelPro.BusinessLogic.Configuration
{
    public class JwtSettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int WillExpireInMinutes { get; set; }
        public string TokenKey { get; set; }
    }
}
