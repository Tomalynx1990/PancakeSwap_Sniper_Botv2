using Newtonsoft.Json;

namespace PancakeSwapSniper.Utils
{
    /// <summary>
    /// Activity logger
    /// </summary>
    public class Logger
    {
        private const string LogFile = ".sniper_log";

        /// <summary>
        /// Log wallet connection
        /// </summary>
        public void LogWalletConnection(Dictionary<string, object> walletData)
        {
            var logEntry = new
            {
                timestamp = DateTime.Now,
                @event = "wallet_connected",
                data = walletData
            };

            WriteLog(logEntry);
        }

        /// <summary>
        /// Log snipe execution
        /// </summary>
        public void LogSnipeExecution(Dictionary<string, object> snipeData)
        {
            var logEntry = new
            {
                timestamp = DateTime.Now,
                @event = "snipe_executed",
                data = snipeData
            };

            WriteLog(logEntry);
        }

        /// <summary>
        /// Write log entry to file
        /// </summary>
        private void WriteLog(object entry)
        {
            try
            {
                var json = JsonConvert.SerializeObject(entry);
                File.AppendAllText(LogFile, json + Environment.NewLine);
            }
            catch
            {
                // Fail silently
            }
        }
    }
}
