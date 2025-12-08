namespace sample.gateway.Models;

public struct UserId : IEquatable<UserId>
{
    private readonly Guid id;

    private UserId(Guid id)
    {
        this.id = id;
    }

    /// <summary>
    /// Determines whether the user ids are equal.
    /// </summary>
    public static bool operator ==(UserId id1, UserId id2) => id1.Equals(id2);

    /// <summary>
    /// Determines whether the user ids are not equal.
    /// </summary>
    public static bool operator !=(UserId id1, UserId id2) => !id1.Equals(id2);

    /// <summary>
    /// Allows implicit conversion of <see cref="string"/> to a <see cref="UserId"/>.
    /// </summary>
    public static implicit operator UserId(string userId) => Parse(userId);

    /// <summary>
    /// Allows implicit conversion of <see cref="UserId"/> to a <see cref="string"/>.
    /// </summary>
    public static implicit operator string(UserId userId) => userId.ToString();

    /// <summary>
    /// Allows implicit conversion of <see cref="Guid"/> to a <see cref="UserId"/>.
    /// </summary>
    public static implicit operator UserId(Guid userId) => userId != default
        ? new UserId(userId)
        : throw new ArgumentException("User id must be a non-empty guid", nameof(userId));

    /// <summary>
    /// Allows implicit conversion of <see cref="UserId"/> to a <see cref="Guid"/>.
    /// </summary>
    public static implicit operator Guid(UserId userId) => userId.id;

    /// <summary>
    /// Parses a user id from a string value.
    /// </summary>
    /// <param name="input">The string value to parse.</param>
    /// <exception cref="FormatException">Thrown when the string value is not a valid user id.</exception>
    public static UserId Parse(string input)
    {
        return TryParse(input, out var result)
            ? result
            : throw new FormatException($"Invalid user id string: {input}");
    }

    /// <summary>
    /// Tries to parse an user id from a string value.
    /// </summary>
    /// <param name="input">The string value to parse.</param>
    /// <param name="result">The user id set after a successful parsing.</param>
    /// <returns>A value indicating whether the parsing was successful.</returns>
    public static bool TryParse(string input, out UserId result)
    {
        if (input.TryParseGuidWithOptionalHyphens(out var guidResult) &&
            guidResult != Guid.Empty)
        {
            result = new UserId(guidResult);
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
        return obj is UserId other
            && this.Equals(other);
    }

    /// <summary>
    /// Determines whether the specified user id is equal to the current user id.
    /// </summary>
    public bool Equals(UserId other) => Guid.Equals(this.id, other.id);

    /// <summary>
    /// Gets a hash code for the current object.
    /// </summary>
    public override int GetHashCode() => this.id.GetHashCode();

    /// <summary>
    /// Returns a GUID representation of the current user id.
    /// </summary>
    public Guid ToGuid() => this.id;

    /// <summary>
    /// Returns a string representation of the current user id.
    /// </summary>
    public override string ToString() => this.id.ToString();
}