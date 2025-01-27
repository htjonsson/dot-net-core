using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using JwtApp.Request;
using JwtApp.Services;
using JwtApp.Response;
using JwtApp;

namespace JwtApp.Controllers;

[Authorize]
[ApiController]
[Route("/jwt")]
public class JwtController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IUserService   _userService;

    public JwtController(IConfiguration config, IUserService userService)
    {
        _config = config;
        _userService = userService;
    }  

    [AllowAnonymous]
    [HttpPost("register")]
    public IActionResult Register(RegisterRequest registerRequest)
    {
        if (false == _userService.Exists(registerRequest.Username))    
        {
            _userService.Register(registerRequest.Username, registerRequest.Password);    
        }  
        
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login(LoginRequest loginRequest)
    {
        // Authenticate user
        var user = _userService.Authenticate(loginRequest.Username, loginRequest.Password);

        if (user == null)
            return Unauthorized();

        var settings = new JwtSettings(_config);

        if (false == settings.IsValid())
            return Unauthorized();

        #region Generate claim identity 

        string jti = Guid.NewGuid().ToString();

        var claimsIdentity = CreateClaims(user.Id.ToString(), jti);

        #endregion

        #region Generate tokens

        var accessToken = JwtHelper.GenerateAccessToken(claimsIdentity, settings.Secret, settings.ExpiryInMinutes);
        var refreshToken = JwtHelper.GenerateRefreshToken();

        #endregion

        #region Save tokens to database

        _userService.SaveRefreshToken(jti, refreshToken);
        
        #endregion

        // #region Get data out of the token

        // JwtHelper.Decode(accessToken);

        // #endregion

        var response = new JwtTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };

        return Ok(response);
    }    

    [HttpPost]
    [Route("refresh")]
    public IActionResult Refresh(JwtRefreshRequest refreshRequest)
    {
        var settings = new JwtSettings(_config);

        var userId = GetUserId();
        var jti = GetJti();

        if (_userService.IsValidRefreshToken(jti, refreshRequest.RefreshToken) == false)
            return Unauthorized();

        var claimsIdentity = CreateClaims(userId, jti);

        var reponse = new JwtRefreshResponse
        {
            AccessToken = JwtHelper.GenerateAccessToken(claimsIdentity, settings.Secret, settings.ExpiryInMinutes)
        };

        _userService.SaveRefreshToken(jti, refreshRequest.RefreshToken);

        return Ok(reponse);
    }

    [HttpDelete]
    [Route("revoke-access")]
    public IActionResult RevokeAccess()
    {
        var jti = GetJti();

        _userService.RevokeRefreshToken(jti);
        // _userService.RevokeAccessToken(jti);

        return Ok();
    }     

    [HttpDelete]
    [Route("revoke-refresh")]
    public IActionResult RevokeRefresh()
    {
        var jti = GetJti();

        var token = _userService.GetRefreshToken(jti);
        
        if (token == null)
            return NotFound();

        _userService.RevokeRefreshToken(jti);

        return Ok();
    }

    private ClaimsIdentity CreateClaims(string userId, string jti)
    {
        var claimsIdentity = new ClaimsIdentity(new[] 
        {
            new Claim("id", userId),
            new Claim(JwtRegisteredClaimNames.Jti, jti) 
        });

        return claimsIdentity;
    }

    private string GetJti()
    {
        return this.User.Claims.FirstOrDefault(claim => claim.Type == "jti")?.Value ?? string.Empty;
    }  

    private string GetUserId()
    {
        return this.User.Claims.FirstOrDefault(claim => claim.Type == "id")?.Value ?? string.Empty;
    }    
}