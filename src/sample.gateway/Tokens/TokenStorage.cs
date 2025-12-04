namespace sample.gateway.Tokens
{
    using System.IO;
    using System.Runtime.Versioning;
    using System.Security.Cryptography;
    using System.Text.Json;

    /// <summary>
    /// - Use `DataProtectionScope.CurrentUser` so only the local user can decrypt.
    /// - The directory should be access-protected (AppData is usually sufficient).
    /// </summary>
    public static class TokenStorage
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "_SampleGateway");

        [SupportedOSPlatform("windows")]
        public static void SaveToken(TokenInfo token, string resource)
        {
            var tokenResourceFile = Path.Combine(FilePath, resource);
            Directory.CreateDirectory(Path.GetDirectoryName(tokenResourceFile));
            string json = JsonSerializer.Serialize(token);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
            byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(tokenResourceFile, encrypted);
        }

        [SupportedOSPlatform("windows")]
        public static TokenInfo LoadToken(string resource)
        {
            var tokenResourceFile = Path.Combine(FilePath, resource);
            if (!File.Exists(tokenResourceFile))
            {
                return null;
            }

            byte[] encrypted = File.ReadAllBytes(tokenResourceFile);
            byte[] data = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            string json = System.Text.Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<TokenInfo>(json);
        }

        public static void DeleteToken(string resource)
        {
            var tokenResourceFile = Path.Combine(FilePath, resource);
            if (File.Exists(tokenResourceFile))
            {
                File.Delete(tokenResourceFile);
            }
        }
    }
}