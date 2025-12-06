namespace sample.gateway.Tokens;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class MicrosoftAuthentication : IMicrosoftAuthentication, IDisposable
{
    private readonly ILogger<MicrosoftAuthentication> logger;

    private readonly IMsalCredentialFactory credentialFactory;

    private readonly ConcurrentDictionary<string, Lazy<TokenCredential>> tokenCredentialProviderCache;

    public MicrosoftAuthentication(ILogger<MicrosoftAuthentication> logger, IMsalCredentialFactory credentialFactory)
    {
        this.logger = logger;
        this.credentialFactory = credentialFactory;
        tokenCredentialProviderCache = new ConcurrentDictionary<string, Lazy<TokenCredential>>();
    }

    public void Dispose()
    {
        foreach (KeyValuePair<string, Lazy<TokenCredential>> item in tokenCredentialProviderCache)
        {
            if (item.Value.IsValueCreated)
            {
                (item.Value as IDisposable)?.Dispose();
            }
        }
    }

    public async Task<AccessToken> GetAccessTokenAsync(string resource, CancellationToken cancellation, MicrosoftAuthenticationConfig authConfig, string scope = null, bool? useCachedTokenCredential = null)
    {
        string[] scopes = new string[1] { resource + (scope ?? "/.default") };
        TokenCredential credential = GetAzureServiceTokenCredential(authConfig, useCachedTokenCredential.GetValueOrDefault(true));
        return await credential.GetTokenAsync(new TokenRequestContext(scopes, Guid.NewGuid().ToString(), null, authConfig.TenantId, isCaeEnabled: false), cancellation);
    }

    public TokenCredential GetAzureServiceTokenCredential(MicrosoftAuthenticationConfig authConfig)
    {
        return GetAzureServiceTokenCredential(authConfig, fromCache: true);
    }

    public TokenCredential GetAzureServiceTokenCredential(MicrosoftAuthenticationConfig authConfig, bool fromCache)
    {
        authConfig = authConfig ?? new MicrosoftAuthenticationConfig();
        return credentialFactory.CreateCredential(authConfig);
    }
}