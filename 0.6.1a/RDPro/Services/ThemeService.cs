using Avalonia;
using Avalonia.Styling;
using Avalonia.Media;
using FluentAvalonia.Styling;

namespace RDPro.Services
{
    public enum AppTheme
    {
        Light,
        Dark,
        System
    }

    public static class ThemeService
    {
        private static FluentAvaloniaTheme? Theme =>
            Application.Current?.Styles[0] as FluentAvaloniaTheme;

        /// <summary>
        /// Apply a theme (Light, Dark, System)
        /// </summary>
        public static void SetTheme(AppTheme theme)
        {
            switch (theme)
            {
                case AppTheme.Light:
                    Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
                    break;
                case AppTheme.Dark:
                    Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case AppTheme.System:
                    Application.Current!.RequestedThemeVariant = ThemeVariant.Default;
                    break;
            }
        }

        /// <summary>
        /// Shortcut for following the system theme
        /// </summary>
        public static void FollowSystemTheme() =>
            SetTheme(AppTheme.System);

        /// <summary>
        /// Use Windows accent color or fallback
        /// </summary>
        public static void UseSystemAccent(bool useSystem)
        {
            if (Theme != null)
                Theme.PreferSystemTheme = useSystem;
        }

        /// <summary>
        /// Apply a custom accent color
        /// </summary>
        public static void SetCustomAccent(Color accent)
{
    if (Application.Current != null)
    {
        // Override the resource key used by FluentAvalonia
        Application.Current.Resources["SystemAccentColor"] = accent;
        Application.Current.Resources["SystemAccentColorLight1"] =
            Color.FromArgb(200, accent.R, accent.G, accent.B);
        Application.Current.Resources["SystemAccentColorDark1"] =
            Color.FromArgb(150, accent.R, accent.G, accent.B);
    }
}

    }
}
