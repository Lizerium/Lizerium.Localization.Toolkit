/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 06 мая 2026 10:30:02
 * Version: 1.0.12
 */

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
    }
}
