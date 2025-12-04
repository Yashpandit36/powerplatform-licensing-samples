namespace sample.gateway.Models
{
    using Newtonsoft.Json;

    public struct EnvironmentGroupId : IEquatable<EnvironmentGroupId>
    {
        private Guid _value;

        public EnvironmentGroupId(string id) : this(Parse(id))
        {
        }
        public EnvironmentGroupId(Guid id)
        {
            _value = id;
        }

        public static readonly EnvironmentGroupId Empty = (EnvironmentGroupId)Guid.Empty;

        public override bool Equals(object obj)
        {
            if (obj is EnvironmentGroupId)
            {
                return Equals((EnvironmentGroupId)obj);
            }
            if (obj is Guid)
            {
                return Equals((Guid)obj);
            }
            return false;
        }
        public bool Equals(Guid x)
        {
            return _value == x;
        }
        public bool Equals(EnvironmentGroupId x)
        {
            return _value == x._value;
        }
        public byte[] ToByteArray()
        {
            return _value.ToByteArray();
        }
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
        public override string ToString()
        {
            return _value.ToString();
        }
        public static EnvironmentGroupId NewGuid()
        {
            return (EnvironmentGroupId)Guid.NewGuid();
        }
        public static EnvironmentGroupId Parse(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return default;
            }
            return new EnvironmentGroupId
            {
                _value = Guid.Parse(input)
            };
        }
        public static bool TryParse(string input, out EnvironmentGroupId result)
        {
            if (Guid.TryParse(input, out var value))
            {
                result = new EnvironmentGroupId
                {
                    _value = value
                };
                return true;
            }
            result = default;
            return false;
        }
        public static bool operator ==(EnvironmentGroupId x, EnvironmentGroupId y)
        {
            return x._value == y._value;
        }
        public static bool operator !=(EnvironmentGroupId x, EnvironmentGroupId y)
        {
            return x._value != y._value;
        }
        public static implicit operator Guid(EnvironmentGroupId x)
        {
            return x._value;
        }
        public static explicit operator EnvironmentGroupId(Guid x)
        {
            return new EnvironmentGroupId
            {
                _value = x
            };
        }
    }
}