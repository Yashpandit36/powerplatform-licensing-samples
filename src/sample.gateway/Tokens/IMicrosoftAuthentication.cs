namespace sample.gateway.Tokens
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMicrosoftAuthentication : IDisposable
    {
        Task<AccessToken> GetAccessTokenAsync(string resource, CancellationToken cancellation, MicrosoftAuthenticationConfig authConfig, string scope = null, bool? useCachedTokenCredential = null);
    }
}