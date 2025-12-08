namespace sample.gateway.Models;

using System;

public struct TenantId : IEquatable<TenantId>
{
    private readonly Guid id;

    private TenantId(Guid id)
    {
        this.id = id;
    }

    /// <summary>
    /// Determines whether the tenant ids are equal.
    /// </summary>
    public static bool operator ==(TenantId id1, TenantId id2) => id1.Equals(id2);

    /// <summary>
    /// Determines whether the tenant ids are not equal.
    /// </summary>
    public static bool operator !=(TenantId id1, TenantId id2) => !id1.Equals(id2);

    /// <summary>
    /// Allows implicit conversion of <see cref="string"/> to a <see cref="TenantId"/>.
    /// </summary>
    public static implicit operator TenantId(string tenantId) => Parse(tenantId);

    /// <summary>
    /// Allows implicit conversion of <see cref="TenantId"/> to a <see cref="string"/>.
    /// </summary>
    public static implicit operator string(TenantId tenantId) => tenantId.ToString();

    /// <summary>
    /// Allows implicit conversion of <see cref="Guid"/> to a <see cref="TenantId"/>.
    /// </summary>
    public static implicit operator TenantId(Guid tenantId) => tenantId != default
        ? new TenantId(tenantId)
        : throw new ArgumentException("Tenant id must be a non-empty guid", nameof(tenantId));

    /// <summary>
    /// Allows implicit conversion of <see cref="TenantId"/> to a <see cref="Guid"/>.
    /// </summary>
    public static implicit operator Guid(TenantId tenantId) => tenantId.id;

    /// <summary>
    /// Parses a tenant id from a string value.
    /// </summary>
    /// <param name="input">The string value to parse.</param>
    /// <exception cref="FormatException">Thrown when the string value is not a valid tenant id.</exception>
    public static TenantId Parse(string input)
    {
        return TryParse(input, out var result)
            ? result
            : throw new FormatException($"Invalid tenant id string: {input}");
    }

    /// <summary>
    /// Tries to parse an tenant id from a string value.
    /// </summary>
    /// <param name="input">The string value to parse.</param>
    /// <param name="result">The tenant id set after a successful parsing.</param>
    /// <returns>A value indicating whether the parsing was successful.</returns>
    public static bool TryParse(string input, out TenantId result)
    {
        if (input.TryParseGuidWithOptionalHyphens(out var guidResult) &&
            guidResult != Guid.Empty)
        {
            result = new TenantId(guidResult);
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
        return obj is TenantId other
            && this.Equals(other);
    }

    /// <summary>
    /// Determines whether the specified tenant id is equal to the current tenant id.
    /// </summary>
    public bool Equals(TenantId other) => Guid.Equals(this.id, other.id);

    /// <summary>
    /// Gets a hash code for the current object.
    /// </summary>
    public override int GetHashCode() => this.id.GetHashCode();

    /// <summary>
    /// Returns a GUID representation of the current tenant id.
    /// </summary>
    public Guid ToGuid() => this.id;

    /// <summary>
    /// Returns a string representation of the current tenant id.
    /// </summary>
    public override string ToString() => this.id.ToString();
}