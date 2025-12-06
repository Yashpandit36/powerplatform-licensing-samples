namespace sample.gateway.Models;

using System;

public struct EnvironmentId : IEquatable<EnvironmentId>
{
    private const string DefaultPrefix = "Default-";
    private const string DefaultPrefixNoDash = "Default";
    private const string LegacyPrefix = "Legacy-";
    private const string LegacyPrefixNoDash = "Legacy";
    private const string PrimaryPrefix = "Primary-";
    private const string PrimaryPrefixNoDash = "Primary";

    private readonly string id;

    private EnvironmentId(string id)
    {
        this.id = id;
    }

    /// <summary>
    /// Determines whether the environment ids are equal.
    /// </summary>
    public static bool operator ==(EnvironmentId id1, EnvironmentId id2) => id1.Equals(id2);

    /// <summary>
    /// Determines whether the environment ids are not equal.
    /// </summary>
    public static bool operator !=(EnvironmentId id1, EnvironmentId id2) => !id1.Equals(id2);

    /// <summary>
    /// Allows implicit conversion of <see cref="string"/> to a <see cref="EnvironmentId"/>.
    /// </summary>
    public static implicit operator EnvironmentId(string environmentId) => Parse(environmentId);

    /// <summary>
    /// Allows implicit conversion of <see cref="EnvironmentId"/> to a <see cref="string"/>.
    /// </summary>
    public static implicit operator string(EnvironmentId environmentId) => environmentId.ToString();

    /// <summary>
    /// Parses an environment id from a string value.
    /// </summary>
    /// <param name="input">The string value to parse.</param>
    /// <exception cref="ArgumentNullException">Thrown when the string value is null.</exception>
    /// <exception cref="FormatException">Thrown when the string value is not a valid environment id.</exception>
    public static EnvironmentId Parse(string input)
    {
        return TryParse(input, out EnvironmentId result)
            ? result
            : throw new FormatException($"Invalid environment id string: {input}");
    }

    /// <summary>
    /// Tries to parse an environment id from a string value.
    /// </summary>
    /// <param name="input">The string value to parse.</param>
    /// <param name="result">The environment id set after a successful parsing.</param>
    /// <returns>A value indicating whether the parsing was successful.</returns>
    public static bool TryParse(string input, out EnvironmentId result)
    {
        if (TryNormalize(input, out var stringResult))
        {
            result = new EnvironmentId(stringResult);
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    public override bool Equals(object obj)
    {
        return obj is EnvironmentId other
            && this.Equals(other);
    }

    /// <summary>
    /// Determines whether the specified environment id is equal to the current environment id.
    /// </summary>
    public bool Equals(EnvironmentId other) => string.Equals(this.id, other.id, StringComparison.Ordinal);

    /// <summary>
    /// Gets a hash code for the current object.
    /// </summary>
    public override int GetHashCode() => this.id?.GetHashCode() ?? 0;

    /// <summary>
    /// Returns a string representation of the current environment id.
    /// </summary>
    public override string ToString() => this.id ?? Guid.Empty.ToString();

    /// <summary>
    /// Tries to normalize an environment id string.
    /// </summary>
    /// <param name="input">The input string to normalize.</param>
    /// <param name="result">The normalized value.</param>
    /// <returns>A value indicating whether normalization was successful.</returns>
    private static bool TryNormalize(string input, out string result)
    {
        if (TryParseGuidStringAndPrefix(input, out var guidPrefix, out var guidString) &&
            guidString.TryParseGuidWithOptionalHyphens(out Guid guidResult) &&
            guidResult != Guid.Empty)
        {
            result = $"{guidPrefix}{guidResult}";
            return true;
        }
        else
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to parse the GUID and non-GUID prefix strings from an environment id input string.
    /// </summary>
    /// <param name="input">The input string to parse.</param>
    /// <param name="guidPrefix">The out parameter in which to supply the prefix of the GUID, or <see cref="string.Empty"/> if one does not exist.</param>
    /// <param name="guidString">The out parameter in which to supply the string that should be a GUID.</param>
    /// <returns>A value indicating whether parsing was successful.</returns>
    public static bool TryParseGuidStringAndPrefix(string input, out string guidPrefix, out string guidString)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            guidPrefix = null;
            guidString = null;
            return false;
        }

        if (input.TryRemovePrefix(DefaultPrefix, StringComparison.OrdinalIgnoreCase, out guidString) ||
            input.TryRemovePrefix(DefaultPrefixNoDash, StringComparison.OrdinalIgnoreCase, out guidString))
        {
            guidPrefix = DefaultPrefix;
            return true;
        }

        if (input.TryRemovePrefix(LegacyPrefix, StringComparison.OrdinalIgnoreCase, out guidString) ||
            input.TryRemovePrefix(LegacyPrefixNoDash, StringComparison.OrdinalIgnoreCase, out guidString))
        {
            guidPrefix = LegacyPrefix;
            return true;
        }

        if (input.TryRemovePrefix(PrimaryPrefix, StringComparison.OrdinalIgnoreCase, out guidString) ||
            input.TryRemovePrefix(PrimaryPrefixNoDash, StringComparison.OrdinalIgnoreCase, out guidString))
        {
            guidPrefix = PrimaryPrefix;
            return true;
        }

        guidString = input;
        guidPrefix = string.Empty;
        return true;
    }
}