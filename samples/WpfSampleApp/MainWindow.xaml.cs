using System.IO;
using System.Windows;
using Lizerium.Localization.Core;
using L = Generated.Localization.Localization;

namespace WpfSampleApp;

/// <summary>
/// Demonstrates runtime language switching and generated localization API usage.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes the sample window and configures the localization runtime.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        LocalizationService.Instance.Configure(Path.Combine(AppContext.BaseDirectory, "Resources", "Localization"));
        Render();
    }

    private void English_Click(object sender, RoutedEventArgs e)
    {
        LocalizationService.Instance.ChangeLanguage("en");
        Render();
    }

    private void Russian_Click(object sender, RoutedEventArgs e)
    {
        LocalizationService.Instance.ChangeLanguage("ru");
        Render();
    }

    private void Render()
    {
        TitleText.Text = L.MainWindow.Title();
        MessageText.Text = L.MainWindow.Log.DirectoryCorrect(AppContext.BaseDirectory);

        // These calls demonstrate CodeFix-generated keys for parameterless and parameterized values.
        var msgLog = L.MainWindow.TestMsgLog();
        var paramMsgLog = L.MainWindow.TestParam.CreateValue(AppContext.BaseDirectory, "param2", 5);
    }
}
