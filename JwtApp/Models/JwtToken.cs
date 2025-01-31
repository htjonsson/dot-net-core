using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JwtApp.Models;

[Table("jwt_token")]
public class JwtToken 
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("jti")]
    [MaxLength(26)]
    public string Jti { get; set; } = string.Empty;

    [Column("token_type")]
    [MaxLength(10)]
    public string Type { get; set; } = string.Empty;

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("revoked_at")]
    public DateTime RevokedAt { get; set; }

    [Column("expires_at")]
    public DateTime Expires { get; set; }

    [Column("refresh_token")]
    [MaxLength(32)]
    public string RefreshToken { get; set; } = string.Empty;
}