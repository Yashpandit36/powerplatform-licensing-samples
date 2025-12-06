namespace sample.gateway.Discovery;

using System.Collections.Generic;
using System.Collections.Immutable;

/// <summary>
/// Configuration settings for Power Platform endpoints.
/// </summary>
public class PowerPlatformEndpointsSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Endpoints";

    /// <summary>
    /// Gets or sets BAP DNS zones for different cluster categories.
    /// </summary>
    public IReadOnlyDictionary<string, string> BapDnsZones { get; set; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Gets or sets BAP audiences for different cluster categories.
    /// </summary>
    public IReadOnlyDictionary<string, string> BapDnsAudience { get; set; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Gets or sets Power Platform API endpoint suffixes for different cluster categories.
    /// </summary>
    public IReadOnlyDictionary<string, string> PowerPlatformApiEndpointSuffixes { get; set; } = ImmutableDictionary<string, string>.Empty;

}
