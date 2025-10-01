using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Metadata;
using System.Windows.Input;
using ReactiveUI;
using RDPro.Config;
using RDPro.Services;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using Avalonia.Platform.Storage;


namespace RDPro.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private string _selectedTheme = "Default";
    private bool _preferUserAccentColor = true;
    private Color _customAccentColor = Colors.DodgerBlue; // Default to Windows blue

    public ICommand DestroyUserDataCommand { get; }

    public ICommand OpenGitHubCommand { get; } = ReactiveCommand.Create(() =>
    {
        // Open GitHub repository in default browser
        string url = "https://github.com/Furball159/RDPro/tree/main";
        //The Below works on only on Windows. Cross-platform versions not required as this is a Windows-only app.
        Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
    });

    public ICommand OpenAboutCommand { get; } = ReactiveCommand.Create(async () =>
    {
        // Open About dialog or page
        string title = "About Remote Desktop Connection Pro";
        string message = "Remote Desktop Connection Pro\nVersion 0.6.8b\n\nDeveloped by Furball159 (Ethan C)\n\nThis application is open-source and available on GitHub.\n\n© 2025 Furball159. All rights reserved.\n\nThis program was built for you! A power user with a need for easy and powerful Remote Desktop Connection Management, with a focus on privacy and security. Enjoy!\n";
        await UiService.ShowInfoAsync(title, message);
    });

    public ICommand OpenPrivacyPolicyCommand { get; } = ReactiveCommand.Create(() =>
    {
        // Open Privacy Policy in default browser
        string url = "https://github.com/Furball159/RDPro/blob/main/PrivacyPolicy.md";
        //The Below works on only on Windows. Cross-platform versions not required as this is a Windows-only app.
        Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
    });
    

    public ICommand ExportSettingsCommand { get; } = ReactiveCommand.CreateFromTask(async () =>
    {
        // Export user data logic here.
        // This is a placeholder for the actual implementation. - Ethan 30-09-2025 @ 11:42:53 PM Canberra/Melbourne/Sydney time
        // To implement: Export all user data (connections, gateways, settings) to a user-specified location.
        var title = "Export Settings (Beta)";
        var message = "Warning! This feature is in beta and may not work as expected. Please ensure you have manual backups of your data before proceeding.\n\nThe export will include all connections, gateways, and application settings. You will be prompted to choose a location to save the exported data.\n\nDo you wish to proceed anyway?\n";
        bool confirmed = await UiService.ShowConfirmAsync(title, message);
        if (confirmed)
        {
            Console.WriteLine("User acknowledged export warning.");

            // Correctly get special folder paths
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Use Path.Combine for safety and define a .zip file for the destination
            string destinationArchiveFileName = Path.Combine(userProfile, "Desktop", "RDPro-ExportedData.zip");
            string sourceDirectory = Path.Combine(localAppData, "RD Pro");
            if (File.Exists(destinationArchiveFileName))
            {
                // If the file already exists, delete it to avoid exceptions
                try
                {
                    File.Delete(destinationArchiveFileName);
                    Console.WriteLine("Existing export file deleted successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not delete existing export file: {ex.Message}");
                    await UiService.ShowInfoAsync("Export Failed", "Could not delete existing export file. Please close any applications using the file and try again.");
                    return;
                }
            }
            ZipFile.CreateFromDirectory(sourceDirectory, destinationArchiveFileName, CompressionLevel.Optimal, true);
            // The Result Dialog is shown after the operation completes.
            title = "Export Settings (Beta)";
            message = $"Export completed successfully!\n\nYour data has been exported to:\n{destinationArchiveFileName}\n\nPlease check your desktop for the RDPro-ExportedData.zip file.\n";
            await UiService.ShowInfoAsync(title, message);
            Console.WriteLine($"User data exported successfully to: {destinationArchiveFileName}");
            //Optionally, ask if they want to open the folder.
            bool openFolder = await UiService.ShowConfirmAsync("Open Folder?", "Do you want to open the folder containing the exported data?");
            if (openFolder)
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{destinationArchiveFileName}\"") { CreateNoWindow = true });
            }
            else
            {
                Console.WriteLine("User opted not to open the folder.");
            }
            //Ask User if they want to wipe local data after export.
            bool wipeData = await UiService.ShowConfirmAsync("Wipe Local Data?", "Do you want to wipe local user data now? This will delete all connections, gateways, and settings from this device. Ensure your exported data is safely backed up before proceeding.\n\nWARNING: This action is irreversible and will close the application after completion.\n\n The App Will Close after wiping local data if you continue\n Press no to retain local data.\n");
            if (wipeData)
            {
                Console.WriteLine("User opted to wipe local data after export.");
                // Call the same logic as DestroyUserDataCommand
                try
                {
                    // 1. Get the actual path for the %localappdata% folder.
                    string rdProFolder = Path.Combine(localAppData, "RD Pro");

                    // 2. Build the full paths to your files and directories.
                    string connectionsPath = Path.Combine(rdProFolder, "connections");
                    string configPath = Path.Combine(rdProFolder, "settings.json");
                    string gatewaysPath = Path.Combine(rdProFolder, "gateways");

                    Console.WriteLine($"Attempting to delete config at: {configPath}");
                    Console.WriteLine($"Attempting to delete connections at: {connectionsPath}");
                    Console.WriteLine($"Attempting to delete gateways at: {gatewaysPath}");

                    // 3. (Best Practice) Check if they exist before trying to delete them.
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
                }
            }
            else
                Console.WriteLine("User opted not to wipe local data after export.");
        }
        else
        {
            Console.WriteLine("User export cancelled by user.");
        }
    });

    public ICommand ImportSettingsCommand { get; } = ReactiveCommand.CreateFromTask(async () =>
    {
        // Import user data logic here.
        // This is a placeholder for the actual implementation. - Ethan 30-09-2025 @ 11:43:13 PM Canberra/Melbourne/Sydney time
        // To implement: Import all user data (connections, gateways, settings) from a user-specified location.
        string title = "Import Settings (Beta)"; // Declare title here
        string message = "Warning! This feature is in beta and may not work as expected. \n\nFor Importing only 1 .rdpx/.rdp file please use the import connection button, not the import settings/user data. \n\nPlease ensure you have manual backups of your data before proceeding.\n\nThe import will overwrite all existing connections, gateways, and application settings. You will be prompted to choose a location to import the data from.\n\nDo you wish to proceed anyway?\n"; // Declare message here
        bool confirmed = await UiService.ShowConfirmAsync(title, message);
        if (confirmed)
        {
            Console.WriteLine("User acknowledged import warning.");

            var topLevel = App.GetTopLevel();
            if (topLevel is null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select RDPro Exported Data (.zip)",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Zip Files") { Patterns = new[] { "*.zip" } }
                }
            });

            if (files.Count > 0)
            {
                // Use TryGetPath() for safety on desktop platforms.
                string? selectedFile = files[0].Path.LocalPath;
                if (selectedFile is null) return;

                Console.WriteLine($"User selected file for import: {selectedFile ?? "N/A"}");

                // Correctly get special folder paths
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                string destinationDirectory = Path.Combine(localAppData);

                try
                {
                    // Ensure the destination directory exists
                    if (!Directory.Exists(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                    // Extract the zip file to the destination directory
                    ZipFile.ExtractToDirectory(selectedFile, destinationDirectory, true); // true to overwrite existing files

                    title = "Import Settings (Beta)"; // Re-assign, but it's already declared
                    message = "Import completed successfully! The application will now close to apply the changes. Please restart it."; // Re-assign, but it's already declared
                    await UiService.ShowInfoAsync(title, message);
                    Environment.Exit(0); // Exit gracefully to apply changes
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during import: {ex.Message}");
                    await UiService.ShowInfoAsync("Import Failed", $"An error occurred during the import process: {ex.Message}");
                }
            }
        }
        else
        {
            Console.WriteLine("User cancelled import.");
        }
    });
    
    public ICommand ClearHistoryCommand { get; } = ReactiveCommand.Create(async () =>
    {
        // Clear history logic here.
        // This is a placeholder for the actual implementation. - Ethan 30-09-2025 @ 11:08:31 PM Canberra/Melbourne/Sydney time
        // To implement: Clear the history of connections from wherever it is stored.
        // Note: Implementation of ClearHistoryCommand is pending un-commenting above lines and adding the command to the ViewModels.SettingsViewModel. is required for this to function.
        // leave the above command in comments until implemented to avoid runtime errors. - Ethan 30-09-2025 @ 10:53:41 PM Canberra/Melbourne/Sydney time
        var title = "Not Implemented";
        var message = "The Clear History feature is not implemented yet. Please check back in a future update.";
        await UiService.ShowInfoAsync(title, message);
    });

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
            string groupsPath = Path.Combine(rdProFolder, "groups.json");

            Console.WriteLine($"Attempting to delete config at: {configPath}");
            Console.WriteLine($"Attempting to delete connections at: {connectionsPath}");
            Console.WriteLine($"Attempting to delete gateways at: {gatewaysPath}");
            Console.WriteLine($"Attempting to delete groups at: {groupsPath}");

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

            if (File.Exists(groupsPath))
            {
                File.Delete(groupsPath);
                Console.WriteLine("groups.json deleted successfully.");
            }
            else
            {
                Console.WriteLine("groups.json not found, skipping.");
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
