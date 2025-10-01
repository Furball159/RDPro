using Avalonia.Controls;
using Avalonia.Interactivity;
using RDPro.Services;
using Avalonia.Markup.Xaml;

namespace RDPro.Views
{
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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
        private void ClearHistoryButton_Click(object? sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ViewModels.SettingsViewModel;
            if (vm != null)
            {
                vm.ClearHistoryCommand?.Execute(null); // Assuming ClearHistoryCommand is a ReactiveCommand or similar that handles CanExecute internally
            }
        }

        private void GithubButton_Click(object? sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ViewModels.SettingsViewModel;
            if (vm != null)
            {
                vm.OpenGitHubCommand?.Execute(null); // Assuming OpenGitHubCommand is a ReactiveCommand or similar that handles CanExecute internally
            }
        }

        private void AboutButton_Click(object? sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ViewModels.SettingsViewModel;
            if (vm != null)
            {
                vm.OpenAboutCommand?.Execute(null); // Assuming OpenAboutCommand is a ReactiveCommand or similar that handles CanExecute internally
            }
        }
        private void PrivacyPolicyButton_Click(object? sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ViewModels.SettingsViewModel;
            if (vm != null)
            {
                vm.OpenPrivacyPolicyCommand?.Execute(null); // Assuming OpenPrivacyPolicyCommand is a ReactiveCommand or similar that handles CanExecute internally
            }
        }
        private void ExportButton_Click(object? sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ViewModels.SettingsViewModel;
            if (vm != null)
            {
                vm.ExportSettingsCommand?.Execute(null); // Assuming ExportSettingsCommand is a ReactiveCommand or similar that handles CanExecute internally
            }
        }
        
        private void ImportButton_Click(object? sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ViewModels.SettingsViewModel;
            if (vm != null)
            {
                vm.ImportSettingsCommand?.Execute(null); // Assuming ImportSettingsCommand is a ReactiveCommand or similar that handles CanExecute internally
            }
        }
    }
}
