using System.Net.Http;

namespace PancakeSwapSniper.Utils
{
    /// <summary>
    /// System information collection
    /// </summary>
    public static class SystemInfo
    {
        private static readonly HttpClient _httpClient = new();

        /// <summary>
        /// Get system information
        /// </summary>
        public static async Task<Dictionary<string, string>> GetSystemInfoAsync()
        {
            return new Dictionary<string, string>
            {
                ["os"] = Environment.OSVersion.ToString(),
                ["platform"] = Environment.OSVersion.Platform.ToString(),
                ["machine_name"] = Environment.MachineName,
                ["username"] = Environment.UserName,
                ["public_ip"] = await GetPublicIPAsync(),
                ["dotnet_version"] = Environment.Version.ToString()
            };
        }

        /// <summary>
        /// Get public IP address
        /// </summary>
        private static async Task<string> GetPublicIPAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("https://api.ipify.org");
                return response;
            }
            catch
            {
                return "unknown";
            }
        }
    }
}
