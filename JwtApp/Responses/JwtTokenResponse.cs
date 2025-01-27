namespace JwtApp.Response;

public class JwtTokenResponse
{
    public string AccessToken { get; set; } = String.Empty;
    public string RefreshToken { get; set; } = String.Empty;
}