using System;
using System.Reactive;
using ReactiveUI;
using RDPro.Models;

namespace RDPro.ViewModels
{
    public class EditGatewayViewModel : ReactiveObject
    {
        private string _hostname = string.Empty;
        public string Hostname
        {
            get => _hostname;
            set => this.RaiseAndSetIfChanged(ref _hostname, value);
        }

        private string _friendlyName = string.Empty;
        public string FriendlyName
        {
            get => _friendlyName;
            set => this.RaiseAndSetIfChanged(ref _friendlyName, value);
        }

        private bool _usePcCredentials = true;
        public bool UsePcCredentials
        {
            get => _usePcCredentials;
            set => this.RaiseAndSetIfChanged(ref _usePcCredentials, value);
        }

        private string _gatewayUsername = string.Empty;
        public string GatewayUsername
        {
            get => _gatewayUsername;
            set => this.RaiseAndSetIfChanged(ref _gatewayUsername, value);
        }

        private string _gatewayPassword = string.Empty;
        public string GatewayPassword
        {
            get => _gatewayPassword;
            set => this.RaiseAndSetIfChanged(ref _gatewayPassword, value);
        }

        public ReactiveCommand<Unit, Gateway?> SaveCommand { get; }
        public ReactiveCommand<Unit, Gateway?> CancelCommand { get; }

        public EditGatewayViewModel()
        {
            var canSave = this.WhenAnyValue(x => x.Hostname, h => !string.IsNullOrWhiteSpace(h));
            SaveCommand = ReactiveCommand.Create(() =>
            {
                var g = new Gateway
                {
                    Hostname = this.Hostname?.Trim() ?? string.Empty,
                    FriendlyName = this.FriendlyName?.Trim() ?? string.Empty,
                    UsePcCredentials = this.UsePcCredentials,
                    GatewayUsername = this.GatewayUsername ?? string.Empty,
                    GatewayPassword = this.GatewayPassword ?? string.Empty,
                };
                return (Gateway?)g;
            }, canSave);

            CancelCommand = ReactiveCommand.Create(() => (Gateway?)null);
        }
    }
}
