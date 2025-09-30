using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using RDPro.Config;
using Avalonia.Media;
using RDPro.ViewModels;
using System;

namespace RDPro;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ApplyUserSettings();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    // Ensure we always have a FluentAvaloniaTheme in Styles
    private FluentAvaloniaTheme EnsureTheme()
    {
        foreach (var style in Application.Current!.Styles)
        {
            if (style is FluentAvaloniaTheme faTheme)
                return faTheme;
        }

        var newTheme = new FluentAvaloniaTheme();
        Application.Current!.Styles.Add(newTheme);
        return newTheme;
    }

    public void ApplyUserSettings()
    {
        SettingsConfig cfg;

        try
        {
            cfg = SettingsConfig.Load();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RDPro] SettingsConfig corrupted, reverting to defaults. Error: {ex.Message}");
            cfg = new SettingsConfig();
            cfg.Save();
        }

        // Theme switching
        RequestedThemeVariant = cfg.Theme switch
        {
            "Light" => ThemeVariant.Light,
            "Dark"  => ThemeVariant.Dark,
            _       => ThemeVariant.Default
        };

        // Accent color
        var theme = EnsureTheme();

        if (cfg.PreferUserAccentColor)
        {
            var accent = Application.Current?.PlatformSettings?.GetColorValues().AccentColor1;
            theme.CustomAccentColor = accent ?? Colors.DodgerBlue;
        }
        else
        {
            if (Color.TryParse(cfg.CustomAccentHex, out var custom))
                theme.CustomAccentColor = custom;
            else
            {
                Console.WriteLine($"[RDPro] Invalid custom accent '{cfg.CustomAccentHex}', using fallback blue.");
                theme.CustomAccentColor = Colors.DodgerBlue;
            }
        }
    }

    public void ApplyUserSettingsFromViewModel(SettingsViewModel vm)
    {
        RequestedThemeVariant = vm.SelectedTheme switch
        {
            "Light" => ThemeVariant.Light,
            "Dark"  => ThemeVariant.Dark,
            _       => ThemeVariant.Default
        };

        var theme = EnsureTheme();

        if (vm.PreferUserAccentColor)
        {
            var accent = Application.Current?.PlatformSettings?.GetColorValues().AccentColor1;
            theme.CustomAccentColor = accent ?? Colors.DodgerBlue;
        }
        else
        {
            theme.CustomAccentColor = vm.CustomAccentColor;
        }
    }
}
