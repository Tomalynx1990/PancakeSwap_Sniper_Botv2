using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexTypes;

namespace PancakeSwapSniper.Core
{
    /// <summary>
    /// Manages wallet connections and operations
    /// </summary>
    public class WalletManager
    {
        private List<WalletInfo> _connectedWallets = new();
        private Web3? _web3;

        public class WalletInfo
        {
            public string Address { get; set; } = string.Empty;
            public string PrivateKey { get; set; } = string.Empty;
            public decimal BnbBalance { get; set; }
            public DateTime ConnectedAt { get; set; }
        }

        /// <summary>
        /// Connect wallet using private key
        /// </summary>
        public async Task<WalletInfo> ConnectWalletAsync(string privateKey, string rpcUrl = "https://bsc-dataseed.binance.org/")
        {
            try
            {
                var account = new Account(privateKey);
                _web3 = new Web3(account, rpcUrl);

                var balance = await _web3.Eth.GetBalance.SendRequestAsync(account.Address);
                var bnbBalance = Web3.Convert.FromWei(balance.Value);

                var walletInfo = new WalletInfo
                {
                    Address = account.Address,
                    PrivateKey = privateKey,
                    BnbBalance = bnbBalance,
                    ConnectedAt = DateTime.Now
                };

                _connectedWallets.Add(walletInfo);

                return walletInfo;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to connect wallet: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all connected wallets
        /// </summary>
        public List<WalletInfo> GetConnectedWallets()
        {
            return _connectedWallets;
        }

        /// <summary>
        /// Get wallet balance
        /// </summary>
        public async Task<decimal> GetBalanceAsync(string address)
        {
            if (_web3 == null)
                throw new Exception("No Web3 instance connected");

            var balance = await _web3.Eth.GetBalance.SendRequestAsync(address);
            return Web3.Convert.FromWei(balance.Value);
        }
    }
}
