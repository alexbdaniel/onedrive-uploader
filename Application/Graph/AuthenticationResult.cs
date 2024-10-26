namespace Application.Graph;

public enum AuthenticationResult
{
    Success,
    RefreshTokenExpired,
    AccessTokenExpired,
    NewRefreshTokenRequired,
    NewAccessTokenRequired
}