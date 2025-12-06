namespace sample.gateway.Tokens;

public class TokenInfo
{
    public string AccessToken { get; set; }
    public DateTimeOffset ExpirationUtc { get; set; }
}