/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 05 мая 2026 07:01:45
 * Version: 1.0.8
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

        // These calls demonstrate CodeFix-generated keys for parameterless and parameterized values.
        var msgLog = L.MainWindow.TestMsgLog();
        var paramMsgLog = L.MainWindow.TestParam.CreateValue(AppContext.BaseDirectory, "param2", 5);
        var test = L.MainWindow.Render.Text2();
        var testParams = L.MainWindow.Render.Text1(AppContext.BaseDirectory, 5);
        var test2 = L.MainWindow.Render.Text3();
        var test3 = L.MainWindow.Render.Text4(AppContext.BaseDirectory);
        var test4 = L.MainWindow.Render.Text5();
        var test5Params = L.MainWindow.Render.Text6(AppContext.BaseDirectory);
        var test6 = L.MainWindow.Render.Text7();
        var test7Param = L.MainWindow.Render.Text8(AppContext.BaseDirectory);
        MessageText.Text = test7Param;
    }
}
