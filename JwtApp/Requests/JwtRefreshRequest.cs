namespace JwtApp.Request;

public class JwtRefreshRequest
{
    public string RefreshToken { get; set; } = String.Empty;
}