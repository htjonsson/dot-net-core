using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using JwtApp.Models;

namespace JwtApp;

public static class JwtHelper
{
    public static void AddAuthentication(WebApplicationBuilder builder)
    {
        AddAuthentication(builder.Services, builder.Configuration);
    }

    public static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var settings = new JwtSettings(configuration);

        AddAuthentication(services, settings.Secret);
    }

    public static void AddAuthentication(IServiceCollection services, string secret)
    {
        var key = Encoding.ASCII.GetBytes(secret);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => 
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });
    }

    public static string GenerateAccessToken(ClaimsIdentity claimsIdentity, string secret, int expiryInMinutes = 15)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes), // token expiration time
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);       
    }

    public static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }   

    public static string GenerateAccessTokenFromRefreshToken(string refreshToken, ClaimsIdentity claimsIdentity, string secret, int expiryInMinutes = 15)
    {
        // Implement logic to generate a new access token from the refresh token
        // Verify the refresh token and extract necessary information (e.g., user ID)
        // Then generate a new access token

        // For demonstration purposes, return a new token with an extended expiry
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes), // Extend expiration time
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);       
    }

    public static (string jti, DateTime validTo) Decode(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(accessToken);

        // var jti = tokenS.Claims.First(claim => claim.Type == "jti").Value; 

        var expiration = jwtSecurityToken.Claims.First(claim => claim.Type == "exp").Value; 

        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiration)).UtcDateTime;

        _ = jwtSecurityToken.ValidTo;
        _ = jwtSecurityToken.Payload.Jti;

        return (jwtSecurityToken.Payload.Jti, jwtSecurityToken.ValidTo);
    }    
}