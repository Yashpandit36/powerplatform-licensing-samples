namespace sample.gateway.Models
{
    using System;

    internal static class StringExtensions
    {
        public static bool TryRemovePrefix(this string input, string prefix, StringComparison comparisonType, out string result)
        {
            if (input.StartsWith(prefix, comparisonType))
            {
                result = input.Substring(prefix.Length);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public static bool TryParseGuidWithOptionalHyphens(this string input, out Guid result)
        {
            return Guid.TryParseExact(input, "D", out result)
                || Guid.TryParseExact(input, "N", out result);
        }
    }
}
