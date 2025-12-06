namespace sample.gateway.Tokens;

using Azure.Identity;
using System.Security.Cryptography.X509Certificates;

public class MicrosoftAuthenticationConfig
{
    private Uri _AuthorityHost = GetGlobalAuthorityHost();

    public bool UseManagedIdentity { get; set; }

    public string ClientId { get; set; }

    public string TenantId { get; set; }

    public string ClientCertificateCommonName { get; set; }

    public string RegionName { get; set; }

    public StoreName CertificateStoreName { get; set; } = StoreName.My;

    public StoreLocation CertificateStoreLocation { get; set; } = StoreLocation.LocalMachine;

    public X509Certificate2 Certificate { get; set; }

    public bool? AttemptAutoRegionDiscovery { get; set; }

    public bool? UseMultiTenantCredential { get; set; }

    public bool EnableMsalInternalLogging { get; set; }

    public Uri AuthorityHost
    {
        get
        {
            return _AuthorityHost;
        }
        set
        {
            if (value != null && value.IsAbsoluteUri && string.Equals(value.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                _AuthorityHost = value;
                return;
            }

            throw new InvalidOperationException($"Cannot assign value '{value}' to parameter {"AuthorityHost"} as it is not an absolute uri or non-HTTPS.");
        }
    }

    public override string ToString()
    {
        string value = (UseMultiTenantCredential.GetValueOrDefault() ? "common" : TenantId);
        return $"[authorityHost] '{AuthorityHost}';[authoritySuffix] '{value}';[authConfig] ClientId:'{ClientId}',ClientCertCommonName:'{ClientCertificateCommonName}',ClientCertStore:'{CertificateStoreLocation}', UseManagedIdentity: {UseManagedIdentity}";
    }

    //
    // Summary:
    //     Retrieves a default authority host from machine configuration.
    //
    // Returns:
    //     The authority host.
    private static Uri GetGlobalAuthorityHost()
    {
        return AzureAuthorityHosts.AzurePublicCloud;
    }
}