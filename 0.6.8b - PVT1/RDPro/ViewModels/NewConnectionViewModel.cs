// RDPro/ViewModels/NewConnectionViewModel.cs (Final Fix)

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Collections.ObjectModel;
using ReactiveUI;
using RDPro.Models;

namespace RDPro.ViewModels
{
    public class NewConnectionViewModel : ReactiveObject
    {
        private string _connectionName = string.Empty;
        public string ConnectionName
        {
            get => _connectionName;
            set => this.RaiseAndSetIfChanged(ref _connectionName, value);
        }

        private string _serverAddress = string.Empty;
        public string ServerAddress
        {
            get => _serverAddress;
            set => this.RaiseAndSetIfChanged(ref _serverAddress, value);
        }

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

    public class GatewayOption
    {
        public string Hostname { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;

        public override string ToString() => Display;
    }

    public ObservableCollection<GatewayOption> AvailableGateways { get; } = new ObservableCollection<GatewayOption>();

        private GatewayOption? _selectedGateway = null;
        public GatewayOption? SelectedGateway
        {
            get => _selectedGateway;
            set => this.RaiseAndSetIfChanged(ref _selectedGateway, value);
        }
        
        public ReactiveCommand<Unit, ConnectionDetails?> SaveCommand { get; }
        public ReactiveCommand<Unit, ConnectionDetails?> CancelCommand { get; }

        public NewConnectionViewModel()
        {
            // Load available gateways from persisted store and wire up live refresh
            try
            {
                RefreshGateways();
                // Subscribe to runtime changes so open dialogs reflect updates
                RDPro.Services.GatewayService.GatewaysChanged += RefreshGateways;
            }
            catch
            {
                _selectedGateway = null;
            }

            var isSaveEnabled = this.WhenAnyValue(
                x => x.ConnectionName,
                x => x.ServerAddress,
                (name, address) => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(address));

            // Ensures connection object is reliably returned when Save is clicked.
            SaveCommand = ReactiveCommand.Create(() => 
            {
                var newConnection = new ConnectionDetails
                {
                    ConnectionID = Guid.NewGuid().ToString(),
                    ConnectionName = this.ConnectionName,
                    ServerAddress = this.ServerAddress,
                    Username = this.Username,
                    Gateway = this.SelectedGateway?.Hostname ?? string.Empty 
                };
                return (ConnectionDetails?)newConnection;
            }, isSaveEnabled);

            CancelCommand = ReactiveCommand.Create(() => 
            {
                return (ConnectionDetails?)null;
            });
        }

        private void RefreshGateways()
        {
            try
            {
                var items = RDPro.Services.GatewayService.LoadGateways();
                // Preserve selected gateway if possible (by Hostname)
                var prevHost = SelectedGateway?.Hostname;
                AvailableGateways.Clear();
                // Always have 'No Gateway' as the first choice (empty hostname)
                AvailableGateways.Add(new GatewayOption { Hostname = string.Empty, Display = "No Gateway" });
                if (items != null && items.Count > 0)
                {
                    foreach (var g in items)
                    {
                        var display = string.IsNullOrWhiteSpace(g.FriendlyName) ? g.Hostname : g.FriendlyName;
                        AvailableGateways.Add(new GatewayOption { Hostname = g.Hostname, Display = display });
                    }
                }
                // Restore selection if still present, otherwise default to 'No Gateway'
                if (!string.IsNullOrWhiteSpace(prevHost))
                {
                    var match = AvailableGateways.FirstOrDefault(x => string.Equals(x.Hostname, prevHost, StringComparison.OrdinalIgnoreCase));
                    SelectedGateway = match ?? AvailableGateways.FirstOrDefault();
                }
                else
                {
                    SelectedGateway = AvailableGateways.FirstOrDefault();
                }
            }
            catch
            {
                // ignore and leave list as-is; default to No Gateway
                if (AvailableGateways.Count == 0) AvailableGateways.Add(new GatewayOption { Hostname = string.Empty, Display = "No Gateway" });
                SelectedGateway = AvailableGateways.FirstOrDefault();
            }
        }
    }
}
