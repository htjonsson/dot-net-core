public class JwtSettings
{
    public String Secret { get; set; } = string.Empty;
    public int ExpiryInMinutes { get; set; } = 15;

    public JwtSettings(IConfiguration config)
    {
        this.Secret = Convert.ToString(config["Jwt:Secret"] ?? string.Empty);
        this.ExpiryInMinutes = Convert.ToInt32(config["Jwt:ExpiryInMinutes"]);
    }

    public bool IsValid()
    {
        if (String.IsNullOrEmpty(this.Secret) && this.ExpiryInMinutes <= 0)
            return false;

        return true;
    }
}