namespace ApparelPro.WebApi.APIModels.Registration
{
    public class TokenAPIModel
    {
        public required string Token { get; set; }
        public required string RefreshToken { get; set; }
    }
}
