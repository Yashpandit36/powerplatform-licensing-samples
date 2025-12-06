namespace sample.gateway.Tokens;

public static class TokenExtensions
{
    /// <summary>
    /// Common method to ensure ordinal case comparison with otherString found in original
    /// </summary>
    /// <param name="original">Original string</param>
    /// <param name="otherString">destination or comparison string</param>
    /// <returns></returns>
    public static bool ContainsIgnoreCase(this string original, string otherString)
    {
        return original.Contains(otherString, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Adds a trailing slash and then appends the scope to the <paramref name="resource"/>
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="scope"></param>
    /// <returns></returns>
    public static string GetScopeEnsureResourceTrailingSlash(this string resource, string scope = ".default")
    {
        if (string.IsNullOrEmpty(resource))
        {
            return scope;
        }

        if (resource.ContainsIgnoreCase(scope))
        {
            return resource;
        }

        var resourceWithTrailingSlash = resource.GetResourceWithTrailingSlash();
        return $"{resourceWithTrailingSlash}{scope}";
    }

    /// <summary>
    /// Ensures a trailing slash is appended to the resource url
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    public static string GetResourceWithTrailingSlash(this Uri resource)
    {
        if (resource == null)
        {
            return string.Empty;
        }

        return resource.ToString().GetResourceWithTrailingSlash();
    }

    /// <summary>
    /// Ensures a trailing slash is appended to the resource url
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    public static string GetResourceWithTrailingSlash(this string resource)
    {
        var trailingSlash = System.IO.Path.AltDirectorySeparatorChar.ToString();
        if (!string.IsNullOrWhiteSpace(resource) && !resource.EndsWith(trailingSlash))
        {
            resource += trailingSlash;
        }
        return resource;
    }
}