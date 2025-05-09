namespace Application.DTO.Response.User;

public class TokenResponse
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime  { get; set; }
    
    public TokenResponse(string token, string refreshToken, DateTime refreshTokenExpiryTime)
    {
        Token = token;
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = refreshTokenExpiryTime;
    }
}