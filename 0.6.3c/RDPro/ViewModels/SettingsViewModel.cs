using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Metadata;
using System.Windows.Input;
using ReactiveUI;
using RDPro.Config;
using RDPro.Services;
using System.Threading.Tasks;

namespace RDPro.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private string _selectedTheme = "Default";
    private bool _preferUserAccentColor = true;
    private Color _customAccentColor = Colors.DodgerBlue; // Default to Windows blue

    public ICommand DestroyUserDataCommand { get; }

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

        DestroyUserDataCommand = ReactiveCommand.CreateFromTask(DestroyUserDataAsync);
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
    
    private async Task DestroyUserDataAsync()
    {
        // Show confirmation dialog before proceeding.
        var title = "Confirm Data Deletion";
        var message = "Are you sure you want to permanently delete all user data?\n\nThis includes all connections, gateways, and application settings. This action cannot be undone.\n\nPlease ensure you have exported any important data before proceeding. \n\nThe Program will close after deletion.";

        bool confirmed = await UiService.ShowConfirmAsync(title, message);

        if (!confirmed)
        {
            Console.WriteLine("User data destruction cancelled by user.");
            return;
        }

        try
        {
            // 1. Get the actual path for the %localappdata% folder.
            string localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // 2. Safely combine it with your application's folder name.
            string rdProFolder = Path.Combine(localAppDataFolder, "RD Pro");

            // 3. Build the full paths to your files and directories.
            string connectionsPath = Path.Combine(rdProFolder, "connections");
            string configPath = Path.Combine(rdProFolder, "settings.json");
            string gatewaysPath = Path.Combine(rdProFolder, "gateways");

            Console.WriteLine($"Attempting to delete config at: {configPath}");
            Console.WriteLine($"Attempting to delete connections at: {connectionsPath}");
            Console.WriteLine($"Attempting to delete gateways at: {gatewaysPath}");

            // 4. (Best Practice) Check if they exist before trying to delete them.
            // This prevents exceptions if the method is run twice or if the files were never created.
            if (File.Exists(configPath))
            {
                File.Delete(configPath);
                Console.WriteLine("settings.json deleted successfully.");
            }
            else
            {
                Console.WriteLine("settings.json not found, skipping.");
            }

            if (Directory.Exists(connectionsPath))
            {
                Directory.Delete(connectionsPath, true); // true to delete recursively
                Console.WriteLine("Connections directory deleted successfully.");
            }
            else
            {
                Console.WriteLine("Connections directory not found, skipping.");
            }

            if (Directory.Exists(gatewaysPath))
            {
                Directory.Delete(gatewaysPath, true); // true to delete recursively
                Console.WriteLine("Gateways directory deleted successfully.");
            }
            else
            {
                Console.WriteLine("Gateways directory not found, skipping.");
            }

            Environment.Exit(1); // Exit the application after deletion
            Console.WriteLine("User data destruction process completed.");
        }
        catch (Exception ex)
        {
            // It's good practice to catch potential IO exceptions and log them.
            Console.WriteLine($"An error occurred during user data destruction: {ex.Message}");
            // Re-throwing is important if the calling code (like your ReactiveCommand)
            // needs to know that the operation failed.
            throw;
        }
    }
}
