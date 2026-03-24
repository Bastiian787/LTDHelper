using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using Geode.Extension;
using Geode.Habbo.Packages;
using Geode.Network;

namespace LTDHelper;

/// <summary>
/// Main window for the LTDHelper application
/// Handles the Geode extension connection and auto-buying logic
/// </summary>
public partial class MainWindow : Window
{
    private GeodeExtension? _extension;
    private ConsoleBot? _consoleBot;
    private int _currentLanguageInt = 0;
    private bool _taskStarted = false;
    private bool _taskBlocked = false;
    private bool _testMode = false;
    private bool _taskCanBeStopped = true;
    private int _ltdsBought = 0;
    private const int _ltdBuyLimit = 2;
    private readonly string[] _catalogCategory = new[] { "ler", "set_mode" };

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Visibility = Visibility.Hidden;
        ShowInTaskbar = false;
        DetectLanguage();
        try
        {
            _extension = new GeodeExtension("LTDHelper", "Geode examples.", "Lilith");
            _extension.OnDataInterceptEvent += Extension_OnDataInterceptEvent;
            _extension.OnCriticalErrorEvent += Extension_OnCriticalErrorEvent;
            _extension.Start();

            _consoleBot = new ConsoleBot(_extension, "LTDHelper");
            _consoleBot.OnBotLoaded += ConsoleBot_OnBotLoaded;
            _consoleBot.OnMessageReceived += ConsoleBot_OnMessageReceived;
            _consoleBot.ShowBot();
        }
        catch (Exception ex)
        {
            ShowCriticalError($"Failed to initialize extension: {ex.Message}");
        }
    }

    private void DetectLanguage()
    {
        var cultureName = CultureInfo.CurrentCulture.Name.ToLower();
        if (cultureName.StartsWith("es"))
            _currentLanguageInt = 1;
        else if (cultureName.StartsWith("pt"))
            _currentLanguageInt = 2;
        else
            _currentLanguageInt = 0;
    }

    private void BotWelcome()
    {
        if (_consoleBot == null) return;
        _consoleBot.BotSendMessage(AppTranslator.WelcomeMessage[_currentLanguageInt]);
        _consoleBot.BotSendMessage(AppTranslator.BuyAdvice[_currentLanguageInt]);
        _consoleBot.BotSendMessage(AppTranslator.RiskAdvice[_currentLanguageInt]);
        _consoleBot.BotSendMessage(AppTranslator.FullCommandsList[_currentLanguageInt]);
    }

    private async Task TryToBuyLTD()
    {
        _taskCanBeStopped = false;
        if (!_taskStarted) { _taskCanBeStopped = true; return; }
        try
        {
            if (_extension == null || _consoleBot == null)
                throw new InvalidOperationException("Extension or ConsoleBot not initialized");

            await Task.Delay(new Random().Next(500, 1000));
            _extension.SendToServerAsync(_extension.Out.GetCatalogIndex, "NORMAL");
            var catalogIndexPacket = await _extension.WaitForPacketAsync(_extension.In.CatalogIndex, 4000);
            if (catalogIndexPacket == null)
                throw new TimeoutException("Catalog index response timeout");

            _consoleBot.BotSendMessage(AppTranslator.CatalogIndexLoaded[_currentLanguageInt]);
            var catalogNode = new HCatalogNode(catalogIndexPacket.Packet);
            var categoryNode = FindCatalogCategory(catalogNode.Children, _catalogCategory[_testMode ? 1 : 0]);
            if (categoryNode == null)
                throw new InvalidOperationException("Could not find catalog category");

            await Task.Delay(new Random().Next(500, 1000));
            _consoleBot.BotSendMessage(AppTranslator.SimulatingPageClick[_currentLanguageInt]);
            _extension.SendToServerAsync(_extension.Out.GetCatalogPage, categoryNode.PageId, -1, "NORMAL");
            await Task.Delay(new Random().Next(500, 1000));
            _consoleBot.BotSendMessage(AppTranslator.TryingToBuy[_currentLanguageInt]);
            _extension.SendToServerAsync(
                _extension.Out.PurchaseFromCatalog,
                categoryNode.PageId,
                categoryNode.OfferIds[0],
                "",
                1
            );
            var purchaseOkPacket = await _extension.WaitForPacketAsync(_extension.In.PurchaseOK, 2000);
            if (purchaseOkPacket == null)
                throw new InvalidOperationException("LTD not purchased!");
            _consoleBot.BotSendMessage(AppTranslator.PurchaseOK[_currentLanguageInt]);
            _ltdsBought++;
            _consoleBot.BotSendMessage($"[{_ltdsBought}/{_ltdBuyLimit}]");
            if (_ltdsBought >= _ltdBuyLimit)
            {
                _taskBlocked = true;
                _taskStarted = false;
                _consoleBot.BotSendMessage("Purchase limit reached (2/2)");
                _consoleBot.BotSendMessage(AppTranslator.ExitAdvice[_currentLanguageInt]);
            }
        }
        catch (Exception ex)
        {
            _consoleBot?.BotSendMessage(AppTranslator.PurchaseFailed[_currentLanguageInt]);
            _consoleBot?.BotSendMessage($"Error: {ex.Message}");
        }
        finally
        {
            _taskCanBeStopped = true;
        }
    }

    private HCatalogNode? FindCatalogCategory(HCatalogNode[] children, string categoryName)
    {
        foreach (var node in children)
        {
            if (string.Equals(node.PageName, categoryName, StringComparison.OrdinalIgnoreCase))
                return node;
            var foundNode = FindCatalogCategory(node.Children, categoryName);
            if (foundNode != null)
                return foundNode;
        }
        return null;
    }

    private void ConsoleBot_OnBotLoaded(string _) => BotWelcome();

    private void ConsoleBot_OnMessageReceived(string message)
    {
        if (_taskBlocked)
        {
            _consoleBot?.BotSendMessage(AppTranslator.ExitAdvice[_currentLanguageInt]);
            return;
        }
        var command = message.ToLower().Trim();
        switch (command)
        {
            case "/test":
            case "/probar":
            case "/testar":
                if (!_taskStarted)
                {
                    _consoleBot?.BotSendMessage(AppTranslator.StartedMessage[_currentLanguageInt]);
                    _testMode = true;
                    _taskStarted = true;
                    _ = TryToBuyLTD();
                }
                else
                {
                    _consoleBot?.BotSendMessage(AppTranslator.ReducedCommandsList[_currentLanguageInt]);
                }
                break;
            case "/force":
            case "/forzar":
            case "/forçar":
                if (!_taskBlocked && _taskCanBeStopped)
                {
                    _consoleBot?.BotSendMessage(AppTranslator.StartedMessage[_currentLanguageInt]);
                    _testMode = false;
                    _taskStarted = true;
                    _ = TryToBuyLTD();
                }
                else
                {
                    _consoleBot?.BotSendMessage(AppTranslator.ReducedCommandsList[_currentLanguageInt]);
                }
                break;
            case "/start":
            case "/iniciar":
            case "/começar":
                if (!_taskStarted)
                {
                    _consoleBot?.BotSendMessage(AppTranslator.StartedMessage[_currentLanguageInt]);
                    _testMode = false;
                    _taskStarted = true;
                }
                else
                {
                    _consoleBot?.BotSendMessage(AppTranslator.ReducedCommandsList[_currentLanguageInt]);
                }
                break;
            case "/stop":
            case "/detener":
            case "/parar":
                if (_taskStarted)
                {
                    if (_taskCanBeStopped)
                    {
                        _consoleBot?.BotSendMessage(AppTranslator.StoppedMessage[_currentLanguageInt]);
                        _taskStarted = false;
                    }
                    else
                    {
                        _consoleBot?.BotSendMessage(AppTranslator.StopFailed[_currentLanguageInt]);
                    }
                }
                else
                {
                    _consoleBot?.BotSendMessage(AppTranslator.FullCommandsList[_currentLanguageInt]);
                }
                break;
            case "/exit":
            case "/salir":
            case "/sair":
                _consoleBot?.CustomExitCommand = command;
                Environment.Exit(0);
                break;
            default:
                if (!_taskStarted)
                {
                    _consoleBot?.BotSendMessage(AppTranslator.FullCommandsList[_currentLanguageInt]);
                }
                else
                {
                    _consoleBot?.BotSendMessage(AppTranslator.ReducedCommandsList[_currentLanguageInt]);
                }
                break;
        }
    }

    private void Extension_OnDataInterceptEvent(DataInterceptedEventArgs e)
    {
        if (_extension == null || _consoleBot == null)
            return;
        if ((_extension.In.ErrorReport.Match(e) ||
             _extension.In.PurchaseError.Match(e) ||
             _extension.In.PurchaseNotAllowed.Match(e) ||
             _extension.In.NotEnoughBalance.Match(e)) && _taskStarted)
        {
            e.IsBlocked = true;
        }
        if (_extension.In.CatalogPublished.Match(e) && _taskStarted && _taskCanBeStopped && !_taskBlocked)
        {
            _consoleBot.BotSendMessage(AppTranslator.CatalogUpdateReceived[_currentLanguageInt]);
            _testMode = false;
            _ = TryToBuyLTD();
        }
    }

    private void Extension_OnCriticalErrorEvent(string error)
    {
        ShowCriticalError(error);
    }

    private void ShowCriticalError(string message)
    {
        Visibility = Visibility.Visible;
        ShowInTaskbar = true;
        Activate();
        MessageBox.Show(
            $"{message}",
            "Critical Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );
        Environment.Exit(0);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
        Environment.Exit(0);
    }
}
