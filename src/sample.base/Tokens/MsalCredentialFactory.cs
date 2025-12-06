namespace sample.gateway.Tokens;

using Azure.Identity;
using Microsoft.Identity.Client;
using System.Collections.Generic;

[ExcludeFromCodeCoverage]
public class MsalCredentialFactory : IMsalCredentialFactory
{
    private readonly ILogger<MsalCredentialFactory> logger;
    private readonly IMsalHttpClientFactory msalHttpClientFactory;

    public MsalCredentialFactory(ILogger<MsalCredentialFactory> logger, IMsalHttpClientFactory msalHttpClientFactory)
    {
        this.msalHttpClientFactory = msalHttpClientFactory;
        this.logger = logger;
    }

    public TokenCredential CreateCredential(MicrosoftAuthenticationConfig config)
    {
        List<TokenCredential> tokenCredentials = [this.CreateClientCertificateCredential(config)];

        return new ChainedTokenCredential([.. tokenCredentials]);
    }

    private TokenCredential CreateClientCertificateCredential(MicrosoftAuthenticationConfig config)
    {
        return new DynamicClientCertificateCredential(
            logger: this.logger,
            msalHttpClientFactory: this.msalHttpClientFactory,
            config: config);
    }
}