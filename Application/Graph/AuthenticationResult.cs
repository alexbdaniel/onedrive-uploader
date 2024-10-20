namespace Application.Graph;

public enum AuthenticationResult
{
    Success,
    NewRefreshTokenRequired,
    NewAccessTokenRequired
}