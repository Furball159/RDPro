using System.Collections.ObjectModel;
using Avalonia.Media;
using RDPro.Config;

namespace RDPro.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private string _selectedTheme = "Default";
    private bool _preferUserAccentColor = true;
    private Color _customAccentColor = Colors.DodgerBlue;

    public ObservableCollection<string> Themes { get; } =
        new ObservableCollection<string> { "Default", "Light", "Dark" };

    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (_selectedTheme != value)
            {
                _selectedTheme = value;
                Save();
                OnPropertyChanged();
            }
        }
    }

    public bool PreferUserAccentColor
    {
        get => _preferUserAccentColor;
        set
        {
            if (_preferUserAccentColor != value)
            {
                _preferUserAccentColor = value;
                Save();
                OnPropertyChanged();
            }
        }
    }

    public Color CustomAccentColor
    {
        get => _customAccentColor;
        set
        {
            if (_customAccentColor != value)
            {
                _customAccentColor = value;
                Save();
                OnPropertyChanged();
            }
        }
    }

    public SettingsViewModel()
    {
        var cfg = SettingsConfig.Load();

        SelectedTheme = string.IsNullOrWhiteSpace(cfg.Theme) ? "Default" : cfg.Theme;
        PreferUserAccentColor = cfg.PreferUserAccentColor;

        if (Color.TryParse(cfg.CustomAccentHex, out var c))
            CustomAccentColor = c;
        else
            CustomAccentColor = Colors.DodgerBlue;
    }

    private void Save()
    {
        var cfg = new SettingsConfig
        {
            Theme = SelectedTheme,
            PreferUserAccentColor = PreferUserAccentColor,
            CustomAccentHex = CustomAccentColor.ToString() // store as hex
        };

        cfg.Save();

        if (RDPro.App.Current is App app)
        {
            app.ApplyUserSettingsFromViewModel(this);
        }
    }
}
