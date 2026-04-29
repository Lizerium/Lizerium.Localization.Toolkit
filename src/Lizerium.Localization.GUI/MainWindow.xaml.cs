/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 29 апреля 2026 06:52:46
 * Version: 1.0.2
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Lizerium.Localization.Core;
using Forms = System.Windows.Forms;

namespace Lizerium.Localization.GUI;

/// <summary>
/// Main window for browsing, editing, and synchronizing RESX localization files.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _model = new();
    private readonly ResxReader _reader = new();
    private readonly ResxWriter _writer = new();
    private string? _ruPath;
    private string? _enPath;

    /// <summary>
    /// Initializes the editor window and binds the view model.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        DataContext = _model;
    }

    private void OpenProject_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "Open project folder",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog() == Forms.DialogResult.OK)
            LoadProject(dialog.SelectedPath);
    }

    private void Reload_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_model.ProjectPath))
            LoadProject(_model.ProjectPath);
    }

    private void Sync_Click(object sender, RoutedEventArgs e)
    {
        if (_ruPath is null || _enPath is null)
            return;

        // Missing values are created as TODO to keep both language files structurally aligned.
        foreach (var entry in _model.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Ru))
                _writer.AddOrUpdate(_ruPath, entry.Key, "TODO");
            if (string.IsNullOrWhiteSpace(entry.En))
                _writer.AddOrUpdate(_enPath, entry.Key, "TODO");
        }

        LoadProject(_model.ProjectPath);
    }

    private void AddKey_Click(object sender, RoutedEventArgs e)
    {
        if (_ruPath is null || _enPath is null)
            return;

        var key = MainViewModel.BuildKey(_model.NewNamespace, _model.NewName);
        if (string.IsNullOrWhiteSpace(key))
            return;

        if (_model.Entries.Any(item => string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase)))
        {
            System.Windows.MessageBox.Show("Duplicate key.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _writer.AddOrUpdate(_ruPath, key, string.IsNullOrWhiteSpace(_model.NewRu) ? "TODO" : _model.NewRu);
        _writer.AddOrUpdate(_enPath, key, string.IsNullOrWhiteSpace(_model.NewEn) ? "TODO" : _model.NewEn);
        _model.ClearNewKeyForm();
        LoadProject(_model.ProjectPath);
    }

    private void Entries_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit || e.Row.Item is not LocalizationRow row)
            return;

        // Wait until WPF commits the edited binding value before writing it back to disk.
        Dispatcher.BeginInvoke(() =>
        {
            if (_ruPath is not null)
                _writer.AddOrUpdate(_ruPath, row.Key, row.Ru ?? string.Empty);
            if (_enPath is not null)
                _writer.AddOrUpdate(_enPath, row.Key, row.En ?? string.Empty);

            row.Recalculate();
        });
    }

    private void LoadProject(string projectPath)
    {
        _model.ProjectPath = projectPath;
        var files = Directory.GetFiles(projectPath, "*.resx", SearchOption.AllDirectories);
        _ruPath = files.FirstOrDefault(file => file.EndsWith(".ru.resx", StringComparison.OrdinalIgnoreCase));
        _enPath = files.FirstOrDefault(file => file.EndsWith(".en.resx", StringComparison.OrdinalIgnoreCase));

        if (_ruPath is null || _enPath is null)
        {
            System.Windows.MessageBox.Show("Could not find both .ru.resx and .en.resx files.", "Localization", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var ru = _reader.Load(_ruPath).ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
        var en = _reader.Load(_enPath).ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);

        // Merge both language files so missing translations are visible in the same grid.
        _model.Entries.Clear();
        foreach (var key in ru.Keys.Concat(en.Keys).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(item => item, StringComparer.OrdinalIgnoreCase))
        {
            ru.TryGetValue(key, out var ruValue);
            en.TryGetValue(key, out var enValue);
            _model.Entries.Add(new LocalizationRow(key, ruValue ?? string.Empty, enValue ?? string.Empty));
        }
    }
}

/// <summary>
/// View model for the localization editor window.
/// </summary>
public sealed class MainViewModel : NotifyObject
{
    private string _projectPath = string.Empty;
    private string _newNamespace = string.Empty;
    private string _newName = string.Empty;
    private string _newRu = string.Empty;
    private string _newEn = string.Empty;

    /// <summary>
    /// Gets the localization rows currently displayed by the editor.
    /// </summary>
    public ObservableCollection<LocalizationRow> Entries { get; } = new();

    /// <summary>
    /// Gets or sets the currently opened project path.
    /// </summary>
    public string ProjectPath
    {
        get => _projectPath;
        set => SetField(ref _projectPath, value);
    }

    /// <summary>
    /// Gets or sets the namespace-like prefix used when adding a new key.
    /// </summary>
    public string NewNamespace
    {
        get => _newNamespace;
        set
        {
            SetField(ref _newNamespace, value);
            OnPropertyChanged(nameof(AddKeyPreview));
        }
    }

    /// <summary>
    /// Gets or sets the final name segment used when adding a new key.
    /// </summary>
    public string NewName
    {
        get => _newName;
        set
        {
            SetField(ref _newName, value);
            OnPropertyChanged(nameof(AddKeyPreview));
        }
    }

    /// <summary>
    /// Gets or sets the initial Russian value for a new key.
    /// </summary>
    public string NewRu
    {
        get => _newRu;
        set => SetField(ref _newRu, value);
    }

    /// <summary>
    /// Gets or sets the initial English value for a new key.
    /// </summary>
    public string NewEn
    {
        get => _newEn;
        set => SetField(ref _newEn, value);
    }

    /// <summary>
    /// Gets a preview of the generated RESX key for the add-key form.
    /// </summary>
    public string AddKeyPreview => BuildKey(NewNamespace, NewName);

    /// <summary>
    /// Clears all fields used to create a new key.
    /// </summary>
    public void ClearNewKeyForm()
    {
        NewNamespace = string.Empty;
        NewName = string.Empty;
        NewRu = string.Empty;
        NewEn = string.Empty;
    }

    /// <summary>
    /// Builds a RESX key from namespace-like and name-like user input.
    /// </summary>
    /// <param name="ns">Namespace-like prefix.</param>
    /// <param name="name">Final key name.</param>
    /// <returns>A sanitized underscore-delimited RESX key.</returns>
    public static string BuildKey(string ns, string name)
    {
        var parts = (ns ?? string.Empty)
            .Split(new[] { '.', '_', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Concat((name ?? string.Empty).Split(new[] { '.', '_', ' ' }, StringSplitOptions.RemoveEmptyEntries));

        return string.Join("_", parts.Select(Sanitize).Where(item => item.Length > 0));
    }

    private static string Sanitize(string value)
    {
        return new string(value.Where(char.IsLetterOrDigit).ToArray());
    }
}

/// <summary>
/// Represents one editable localization row across Russian and English RESX files.
/// </summary>
public sealed class LocalizationRow : NotifyObject
{
    private string _ru;
    private string _en;

    /// <summary>
    /// Initializes a localization row.
    /// </summary>
    /// <param name="key">RESX data name.</param>
    /// <param name="ru">Russian value.</param>
    /// <param name="en">English value.</param>
    public LocalizationRow(string key, string ru, string en)
    {
        Key = key;
        _ru = ru;
        _en = en;
        Recalculate();
    }

    /// <summary>
    /// Gets the RESX data name.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets or sets the Russian value.
    /// </summary>
    public string Ru
    {
        get => _ru;
        set
        {
            SetField(ref _ru, value);
            Recalculate();
        }
    }

    /// <summary>
    /// Gets or sets the English value.
    /// </summary>
    public string En
    {
        get => _en;
        set
        {
            SetField(ref _en, value);
            Recalculate();
        }
    }

    /// <summary>
    /// Gets the maximum placeholder count detected across languages.
    /// </summary>
    public int Params { get; private set; }

    /// <summary>
    /// Gets the current row validation status.
    /// </summary>
    public string Status { get; private set; } = string.Empty;

    /// <summary>
    /// Gets whether the row has a placeholder mismatch.
    /// </summary>
    public bool HasError => Status.Contains("mismatch", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets whether the row is missing at least one translation.
    /// </summary>
    public bool HasWarning => Status.Contains("missing", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Recalculates placeholder counts and validation status.
    /// </summary>
    public void Recalculate()
    {
        var ruCount = PlaceholderAnalyzer.CountParams(Ru);
        var enCount = PlaceholderAnalyzer.CountParams(En);
        Params = Math.Max(ruCount, enCount);

        Status = (string.IsNullOrWhiteSpace(Ru), string.IsNullOrWhiteSpace(En), ruCount == enCount) switch
        {
            (true, true, _) => "missing ru/en",
            (true, false, _) => "missing ru",
            (false, true, _) => "missing en",
            (_, _, false) => $"mismatch ru={ruCount} en={enCount}",
            _ => "ok"
        };

        OnPropertyChanged(nameof(Params));
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(HasWarning));
    }
}

/// <summary>
/// Base class for view models that need property change notifications.
/// </summary>
public abstract class NotifyObject : INotifyPropertyChanged
{
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises <see cref="PropertyChanged"/> for the supplied property.
    /// </summary>
    /// <param name="propertyName">Name of the changed property.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Updates a backing field and raises change notification when the value changed.
    /// </summary>
    /// <typeparam name="T">Field value type.</typeparam>
    /// <param name="field">Backing field reference.</param>
    /// <param name="value">New value.</param>
    /// <param name="propertyName">Property name to notify.</param>
    protected void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        OnPropertyChanged(propertyName);
    }
}
