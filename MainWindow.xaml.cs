using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using Geode.Network;

namespace LTDHelper;

/// <summary>
/// Main window for the LTDHelper application.
/// Handles the G-Earth extension connection and auto-buying logic
/// using G-Earth-Geode 1.4.1-beta.
/// </summary>
public partial class MainWindow : Window
{
    private LTDExtension? _extension;
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
        DetectLanguage();
        try
        {
            _extension = new LTDExtension();
            _extension.OnDataIntercepted += Extension_OnDataIntercepted;
            _extension.OnCriticalErrorEvent += Extension_OnCriticalErrorEvent;
            _extension.OnConnectedEvent += Extension_OnConnected;

            Log("Extension initialized. Waiting for G-Earth connection…");
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

    /// <summary>
    /// Appends a message to the on-screen log and scrolls to the bottom.
    /// Thread-safe — may be called from any thread.
    /// </summary>
    private void Log(string message)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => Log(message));
            return;
        }
        LogBox.AppendText($"{message}\n");
        LogBox.ScrollToEnd();
    }

    private async Task TryToBuyLTD()
    {
        _taskCanBeStopped = false;
        if (!_taskStarted) { _taskCanBeStopped = true; return; }
        try
        {
            if (_extension == null)
                throw new InvalidOperationException("Extension not initialized");
            if (_extension.In == null)
                throw new InvalidOperationException("Extension not connected to G-Earth");

            await Task.Delay(new Random().Next(500, 1000));
            await _extension.SendToServerAsync(_extension.Out.GetCatalogIndex, "NORMAL");
            var catalogIndexPacket = await _extension.WaitForPacketAsync(_extension.In.CatalogIndex, 4000);
            if (catalogIndexPacket == null)
                throw new TimeoutException("Catalog index response timeout");

            Log(AppTranslator.CatalogIndexLoaded[_currentLanguageInt]);
            var catalogNode = HCatalogNode.FromCatalogIndexPacket(catalogIndexPacket.Packet);
            var categoryNode = FindCatalogCategory(catalogNode.Children, _catalogCategory[_testMode ? 1 : 0]);
            if (categoryNode == null)
                throw new InvalidOperationException("Could not find catalog category");

            await Task.Delay(new Random().Next(500, 1000));
            Log(AppTranslator.SimulatingPageClick[_currentLanguageInt]);
            await _extension.SendToServerAsync(_extension.Out.GetCatalogPage, categoryNode.PageId, -1, "NORMAL");
            await Task.Delay(new Random().Next(500, 1000));
            Log(AppTranslator.TryingToBuy[_currentLanguageInt]);
            await _extension.SendToServerAsync(
                _extension.Out.PurchaseFromCatalog,
                categoryNode.PageId,
                categoryNode.OfferIds[0],
                "",
                1
            );
            var purchaseOkPacket = await _extension.WaitForPacketAsync(_extension.In.PurchaseOK, 2000);
            if (purchaseOkPacket == null)
                throw new InvalidOperationException("LTD not purchased!");
            Log(AppTranslator.PurchaseOK[_currentLanguageInt]);
            _ltdsBought++;
            Log($"[{_ltdsBought}/{_ltdBuyLimit}]");
            if (_ltdsBought >= _ltdBuyLimit)
            {
                _taskBlocked = true;
                _taskStarted = false;
                Log("Purchase limit reached (2/2)");
                Log(AppTranslator.ExitAdvice[_currentLanguageInt]);
                UpdateButtonState();
            }
        }
        catch (Exception ex)
        {
            Log(AppTranslator.PurchaseFailed[_currentLanguageInt]);
            Log($"Error: {ex.Message}");
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

    // -------------------------------------------------------------------------
    // Extension event handlers
    // -------------------------------------------------------------------------

    private void Extension_OnConnected()
    {
        Dispatcher.Invoke(() =>
        {
            StatusLabel.Content = "Connected to G-Earth";
            StartButton.IsEnabled = true;
            ForceButton.IsEnabled = true;
            TestButton.IsEnabled = true;
        });
        Log(AppTranslator.WelcomeMessage[_currentLanguageInt]);
        Log(AppTranslator.BuyAdvice[_currentLanguageInt]);
        Log(AppTranslator.RiskAdvice[_currentLanguageInt]);
        Log(AppTranslator.FullCommandsList[_currentLanguageInt]);
    }

    private void Extension_OnDataIntercepted(DataInterceptedEventArgs e)
    {
        if (_extension?.In == null)
            return;

        // Block error/purchase-failure packets when a task is in progress.
        if (_taskStarted && PacketMatchesAny(e, _extension.In.ErrorReport,
                _extension.In.PurchaseError,
                _extension.In.PurchaseNotAllowed,
                _extension.In.NotEnoughBalance))
        {
            e.IsBlocked = true;
        }

        // Trigger auto-buy on catalog update.
        if (_extension.In.CatalogPublished != null
                && e.Packet.Id == _extension.In.CatalogPublished.Id
                && _taskStarted && _taskCanBeStopped && !_taskBlocked)
        {
            Log(AppTranslator.CatalogUpdateReceived[_currentLanguageInt]);
            _testMode = false;
            _ = TryToBuyLTD();
        }
    }

    private void Extension_OnCriticalErrorEvent(string error)
    {
        ShowCriticalError(error);
    }

    // -------------------------------------------------------------------------
    // Button click handlers
    // -------------------------------------------------------------------------

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_taskBlocked)
        {
            Log(AppTranslator.ExitAdvice[_currentLanguageInt]);
            return;
        }
        if (!_taskStarted)
        {
            Log(AppTranslator.StartedMessage[_currentLanguageInt]);
            _testMode = false;
            _taskStarted = true;
            UpdateButtonState();
        }
        else
        {
            Log(AppTranslator.ReducedCommandsList[_currentLanguageInt]);
        }
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        if (_taskStarted)
        {
            if (_taskCanBeStopped)
            {
                Log(AppTranslator.StoppedMessage[_currentLanguageInt]);
                _taskStarted = false;
                UpdateButtonState();
            }
            else
            {
                Log(AppTranslator.StopFailed[_currentLanguageInt]);
            }
        }
        else
        {
            Log(AppTranslator.FullCommandsList[_currentLanguageInt]);
        }
    }

    private void ForceButton_Click(object sender, RoutedEventArgs e)
    {
        if (_taskBlocked)
        {
            Log(AppTranslator.ExitAdvice[_currentLanguageInt]);
            return;
        }
        if (_taskCanBeStopped)
        {
            Log(AppTranslator.StartedMessage[_currentLanguageInt]);
            _testMode = false;
            _taskStarted = true;
            UpdateButtonState();
            _ = TryToBuyLTD();
        }
        else
        {
            Log(AppTranslator.ReducedCommandsList[_currentLanguageInt]);
        }
    }

    private void TestButton_Click(object sender, RoutedEventArgs e)
    {
        if (_taskBlocked)
        {
            Log(AppTranslator.ExitAdvice[_currentLanguageInt]);
            return;
        }
        if (!_taskStarted)
        {
            Log(AppTranslator.StartedMessage[_currentLanguageInt]);
            _testMode = true;
            _taskStarted = true;
            UpdateButtonState();
            _ = TryToBuyLTD();
        }
        else
        {
            Log(AppTranslator.ReducedCommandsList[_currentLanguageInt]);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
        Environment.Exit(0);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void UpdateButtonState()
    {
        Dispatcher.Invoke(() =>
        {
            StartButton.IsEnabled = !_taskStarted && !_taskBlocked;
            StopButton.IsEnabled = _taskStarted;
            ForceButton.IsEnabled = !_taskBlocked && _taskCanBeStopped;
            TestButton.IsEnabled = !_taskStarted && !_taskBlocked;
        });
    }

    private static bool PacketMatchesAny(DataInterceptedEventArgs e, params Geode.Habbo.Messages.HMessage?[] messages)
    {
        foreach (var m in messages)
        {
            if (m != null && e.Packet.Id == m.Id)
                return true;
        }
        return false;
    }

    private void ShowCriticalError(string message)
    {
        Dispatcher.Invoke(() =>
        {
            Visibility = Visibility.Visible;
            ShowInTaskbar = true;
            Activate();
            MessageBox.Show(
                message,
                "Critical Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            Environment.Exit(0);
        });
    }
}
