using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using RDPro.Models;
using RDPro.Services;
using RDPro.Views;
using System.Diagnostics;

namespace RDPro.ViewModels
{
    public class AllConnectionsViewModel : ReactiveObject
    {
        public ObservableCollection<ConnectionDetails> Connections { get; } = new();
        public ICommand OpenNewConnectionDialogCommand { get; }
        public ICommand EditConnectionCommand { get; }
        public ICommand ShowRdpFileCommand { get; }
        public ICommand ShowRdpXFileCommand { get; }
    public ICommand ConnectCommand { get; }
    public ICommand ConnectWithoutGatewayCommand { get; }
    public ICommand DeleteConnectionCommand { get; }

        private bool _hasConnections;
        public bool HasConnections
        {
            get => _hasConnections;
            private set
            {
                if (this.RaiseAndSetIfChanged(ref _hasConnections, value))
                {
                    this.RaisePropertyChanged(nameof(NoConnections));
                }
            }
        }

        public bool NoConnections => !HasConnections;

        public AllConnectionsViewModel()
        {
            OpenNewConnectionDialogCommand = ReactiveCommand.CreateFromTask(ShowNewConnectionDialog);
            EditConnectionCommand = ReactiveCommand.CreateFromTask<ConnectionDetails>(EditConnectionAsync);
            ShowRdpFileCommand = ReactiveCommand.CreateFromTask<ConnectionDetails>(ShowRdpFileAsync);
            ShowRdpXFileCommand = ReactiveCommand.CreateFromTask<ConnectionDetails>(ShowRdpXFileAsync);
            ConnectCommand = ReactiveCommand.CreateFromTask<ConnectionDetails>(ShowRdpFileAsync);
            ConnectWithoutGatewayCommand = ReactiveCommand.CreateFromTask<ConnectionDetails>(ShowRdpFileWithoutGatewayAsync);
            DeleteConnectionCommand = ReactiveCommand.Create<ConnectionDetails>(DeleteConnection);

            LoadExistingConnections();

            HasConnections = Connections.Count > 0;

            Connections.CollectionChanged += (sender, e) =>
            {
                HasConnections = Connections.Count > 0;
            };
        }

        private async Task ShowNewConnectionDialog()
        {
            var dialog = new NewConnectionWindow
            {
                DataContext = new NewConnectionViewModel()
            };

            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                var result = await dialog.ShowDialog<ConnectionDetails?>(desktop.MainWindow);

                if (result != null)
                {
                    ConnectionFileService.SaveConnection(result);
                    AddNewConnection(result);
                }
            }
        }

        private async Task EditConnectionAsync(ConnectionDetails existing)
        {
            if (existing == null) return;

            var vm = new NewConnectionViewModel
            {
                ConnectionName = existing.ConnectionName,
                ServerAddress = existing.ServerAddress,
                Username = existing.Username,
                // SelectedGateway will be set after the VM populates its AvailableGateways
            };

            var dialog = new NewConnectionWindow
            {
                DataContext = vm
            };
            dialog.Title = "Edit Connection";

            // Try to set SelectedGateway to the matching GatewayOption before showing the dialog
            if (!string.IsNullOrWhiteSpace(existing.Gateway))
            {
                var match = vm.AvailableGateways.FirstOrDefault(x => string.Equals(x.Hostname, existing.Gateway, StringComparison.OrdinalIgnoreCase));
                if (match != null) vm.SelectedGateway = match;
            }

            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                var result = await dialog.ShowDialog<ConnectionDetails?>(desktop.MainWindow);

                if (result != null)
                {
                    // Keep the same ConnectionID to overwrite files
                    result.ConnectionID = existing.ConnectionID;

                    ConnectionFileService.SaveConnection(result);

                    // Update in-memory list (replace existing)
                    int idx = Connections.IndexOf(existing);
                    if (idx >= 0)
                    {
                        Connections[idx] = result;
                    }
                }
            }
        }

        private async Task ShowRdpFileAsync(ConnectionDetails details)
        {
            if (details == null) return;
            // Ensure the legacy .rdp is regenerated from the current connection (so gateway info is up-to-date)
            try
            {
                ConnectionFileService.SaveConnection(details);

                string baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RD Pro", "Connections", "Legacy");
                string rdpFile = Path.Combine(baseDir, $"{details.ConnectionID}.rdp");

                if (File.Exists(rdpFile))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = rdpFile,
                        UseShellExecute = true
                    });
                }
                else
                {
                    await UiService.ShowMessageAsync("File not found", $"RDP file not found:\n{rdpFile}");
                }
            }
            catch (Exception ex)
            {
                await UiService.ShowMessageAsync("Error launching RDP", ex.Message);
            }
        }

        private async Task ShowRdpFileWithoutGatewayAsync(ConnectionDetails details)
        {
            if (details == null) return;

            try
            {
                // Use the ConnectionFileService to create a temporary .rdp without gateway entries and launch it.
                string tempRdp = ConnectionFileService.CreateTemporaryRdpWithoutGateway(details);

                if (File.Exists(tempRdp))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tempRdp,
                        UseShellExecute = true
                    });
                }
                else
                {
                    await UiService.ShowMessageAsync("Temporary file missing", $"Temporary RDP file was not created:\n{tempRdp}");
                }
            }
            catch (Exception ex)
            {
                await UiService.ShowMessageAsync("Error launching temporary RDP", ex.Message);
            }
        }

        private async Task ShowRdpXFileAsync(ConnectionDetails details)
        {
            if (details == null) return;

            string baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RD Pro", "Connections", "Enhanced");
            string rdpxFile = Path.Combine(baseDir, $"{details.ConnectionID}.rdpx");

            try
            {
                if (File.Exists(rdpxFile))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = rdpxFile,
                        UseShellExecute = true
                    });
                }
                else
                {
                    await UiService.ShowMessageAsync("File not found", $"RDPX file not found:\n{rdpxFile}");
                }
            }
            catch (Exception ex)
            {
                await UiService.ShowMessageAsync("Error opening file", ex.Message);
            }
        }

        private void DeleteConnection(ConnectionDetails details)
        {
            if (details == null) return;

            try
            {
                string baseLegacy = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RD Pro", "Connections", "Legacy");
                string baseEnhanced = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RD Pro", "Connections", "Enhanced");

                string rdpFile = Path.Combine(baseLegacy, $"{details.ConnectionID}.rdp");
                string rdpxFile = Path.Combine(baseEnhanced, $"{details.ConnectionID}.rdpx");

                if (File.Exists(rdpFile)) File.Delete(rdpFile);
                if (File.Exists(rdpxFile)) File.Delete(rdpxFile);

                // Remove from collection
                if (Connections.Contains(details))
                {
                    Connections.Remove(details);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting connection files: {ex.Message}");
            }
        }

        private void LoadExistingConnections()
        {
            try
            {
                string connectionsPath = ConnectionFileService.GetEnhancedPath();
                
                Directory.CreateDirectory(connectionsPath);

                var files = Directory.EnumerateFiles(connectionsPath, "*.rdpx").ToList();
                
                if (files.Any())
                {
                    foreach (var filePath in files)
                    {
                        try
                        {
                            var connection = ConnectionFileService.Load(filePath); 
                            if (connection != null)
                            {
                                Connections.Add(connection);
                            }
                        }
                        catch (Exception fileEx)
                        {
                            Console.WriteLine($"Error loading file {filePath}: {fileEx.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during connection directory access: {ex.Message}");
            }
        }

        public void AddNewConnection(ConnectionDetails newConnection)
        {
            Connections.Add(newConnection);
        }
    }
}
