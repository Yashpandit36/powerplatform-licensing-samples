namespace sample.gateway.Tokens
{
    /// <summary>
    /// Interface for implementing MSAL credential factory.
    /// </summary>
    public interface IMsalCredentialFactory
    {
        /// <summary>
        /// Builds the default credential provider capable to acquiring OAuth tokens.
        /// <param name="config">The configuration to use for acquiring access token.</param>
        /// </summary>
        /// <returns>Instance of <see cref="TokenCredential"/> capable of acquiring access tokens with the configured behavior.</returns>
        TokenCredential CreateCredential(MicrosoftAuthenticationConfig config);
    }
}