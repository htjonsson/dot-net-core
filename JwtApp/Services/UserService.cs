using System.Text;
using JwtApp.Models;

namespace JwtApp.Services;

public interface IUserService
{
    User? Authenticate(string username, string password);
    void Register(string username, string password);  
    bool Exists(string username);
    void SaveRefreshToken(string jti, string refreshToken);
    string GetRefreshToken(string jti);
    bool IsValidRefreshToken(string jti, string refreshToken);
    void RevokeRefreshToken(string jti);
}

public class UserService : IUserService
{
    private readonly DataContext _context;

    public UserService(DataContext context)
    {
        _context = context;
    }

    public User? Authenticate(string username, string password)
    {
        return _context.Users.SingleOrDefault(x => x.Username == username && x.Password == password);
    }

    public bool Exists(string username)
    {
        var user = _context.Users.FirstOrDefault(x => x.Username == username);

        return (user != null ? true : false);
    }

    public void Register(string username, string password)
    {
        var user = new User()
        {
            Username = username,
            Password = password
        };

        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public void SaveRefreshToken(string jti, string refreshToken)
    {
        // Convert refreshToken arg, from base64 to a string of ASCII
        var bytes = Convert.FromBase64String(refreshToken);
        var token = Encoding.Default.GetString(bytes);

        var jwtToken  = new JwtToken()
        {
            // UserId = userId,
            Jti = jti,
            // Type = JwtType.RefreshToken.ToString(),
            RefreshToken = token
        };

        _context.JwtTokens.Add(jwtToken);
        _context.SaveChanges();
    }

    public string GetAccessToken(string jti)
    {
        var token = _context.JwtTokens.FirstOrDefault(x => jti == x.Jti && x.RevokedAt != DateTime.MinValue);

        if (token == null)
            return string.Empty;

        return token.Jti;
    }

    public bool IsValidRefreshToken(string jti, string refreshToken)
    {
        string savedToken = GetRefreshToken(jti);

        if (string.IsNullOrWhiteSpace(savedToken))
            return false;

        if (savedToken.Equals(refreshToken) == false)
            return false;

        return true;
    }

    public string GetRefreshToken(string jti)
    {
        var token = _context.JwtTokens.FirstOrDefault(x => jti == x.Jti);

        if (token == null)
            return string.Empty;

        return token.Jti;
    }

    public void RevokeRefreshToken(string jti)
    {
        var token = _context.JwtTokens.FirstOrDefault(x => jti == x.Jti && DateTime.MinValue.Equals(x.RevokedAt) == false);

        if (token == null)
            return;

        token.RevokedAt = DateTime.UtcNow;
        
        _context.Update(token);
        _context.SaveChanges();
    }
}