using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CrowdWordle.Services;

public sealed class TokenService
{
    private readonly TokenConfiguration _config;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly TokenValidationParameters _validationParameters;
    private readonly Lock _lock = new();
    private ulong _userCounter;

    public TokenService(IOptions<TokenConfiguration> config, ulong initialUserCounter = 0)
    {
        _config = config.Value;
        _tokenHandler = new JwtSecurityTokenHandler();

        var secretKey = GenerateSecretKey();
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ClockSkew = TimeSpan.Zero
        };
        _userCounter = initialUserCounter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (string token, ulong id) GenerateToken()
    {
        lock (_lock)
        {
            var userId = Interlocked.Increment(ref _userCounter);
            var now = DateTime.UtcNow;

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

            var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                claims: claims,
                expires: now.AddYears(100),
                signingCredentials: credentials
            );

            return (_tokenHandler.WriteToken(token), userId);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ValidateToken(string token)
    {
        try
        {
            _tokenHandler.ValidateToken(token, _validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public uint ValidateAndGetUserId(string token)
    {
        try
        {
            var claimsPrincipal = _tokenHandler.ValidateToken(token, _validationParameters, out _);
            var userIdClaim = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            return uint.TryParse(userIdClaim, out var userId) ? userId : 0u;
        }
        catch
        {
            return 0u;
        }
    }

    private string GenerateSecretKey()
    {
        if (!string.IsNullOrEmpty(_config.SecretKey))
        {
            return _config.SecretKey.Replace("{datetime}", DateTime.UtcNow.ToString("yyyy-MM-dd_HH:mm:ss"));
        }

        using var rng = RandomNumberGenerator.Create();
        var keyBytes = new byte[32];
        rng.GetBytes(keyBytes);

        return $"crowd-wordle-{DateTime.UtcNow:yyyy-MM-dd}-{Convert.ToBase64String(keyBytes)}";
    }
}