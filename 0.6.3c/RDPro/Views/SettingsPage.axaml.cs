using Avalonia.Controls;
using Avalonia.Interactivity;
using RDPro.Services;

namespace RDPro.Views
{
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void OnLightThemeClick(object? sender, RoutedEventArgs e)
        {
            ThemeService.SetTheme(AppTheme.Light);
        }

        private void OnDarkThemeClick(object? sender, RoutedEventArgs e)
        {
            ThemeService.SetTheme(AppTheme.Dark);
        }

        private void OnSystemThemeClick(object? sender, RoutedEventArgs e)
        {
            ThemeService.FollowSystemTheme();
        }

        private void OnAccentToggleClick(object? sender, RoutedEventArgs e)
        {
            // Example: toggles system accent usage
            ThemeService.UseSystemAccent(true);
        }

        private void DestroyUserDataButton_Click(object? sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ViewModels.SettingsViewModel;
            if (vm != null)
            {
                vm.DestroyUserDataCommand?.Execute(null); // Assuming DestroyUserDataCommand is a ReactiveCommand or similar that handles CanExecute internally
            }
        }
    }
}
