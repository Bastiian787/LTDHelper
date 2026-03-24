using System;
using System.Threading;
using System.Windows;

namespace LTDHelper;

/// <summary>
/// Program entry point with single instance lock
/// </summary>
public class Program
{
    [STAThread]
    public static void Main()
    {
        bool createdNew;
        using (var mutex = new Mutex(initiallyOwned: true, "LTDHelper_for_Geode", out createdNew))
        {
            if (!createdNew)
            {
                MessageBox.Show(
                    "LTDHelper is already running!",
                    "Instance Check",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                return;
            }

            try
            {
                var app = new App();
                app.InitializeComponent();
                app.Run(new MainWindow());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fatal error: {ex.Message}\n\n{ex.StackTrace}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
