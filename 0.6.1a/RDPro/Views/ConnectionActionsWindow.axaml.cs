using Avalonia.Controls;
using Avalonia.Interactivity;
using RDPro.Models;
using RDPro.ViewModels;
using System;

namespace RDPro.Views
{
    public partial class ConnectionActionsWindow : Window
    {
        public ConnectionActionsWindow()
        {
            InitializeComponent();
        }

        public void InitializeFor(ConnectionDetails details, AllConnectionsViewModel vm)
        {
            TitleText.Text = details.ConnectionName;

            BtnConnect.Click += (_, __) => { if (vm.ConnectCommand.CanExecute(details)) vm.ConnectCommand.Execute(details); Close(); };
            BtnConnectNoGateway.Click += (_, __) => { if (vm.ConnectWithoutGatewayCommand.CanExecute(details)) vm.ConnectWithoutGatewayCommand.Execute(details); Close(); };
            BtnEdit.Click += (_, __) => { if (vm.EditConnectionCommand.CanExecute(details)) vm.EditConnectionCommand.Execute(details); Close(); };
            BtnViewRDPX.Click += (_, __) => { if (vm.ShowRdpXFileCommand.CanExecute(details)) vm.ShowRdpXFileCommand.Execute(details); Close(); };
            BtnViewRDP.Click += (_, __) => { if (vm.ShowRdpFileCommand.CanExecute(details)) vm.ShowRdpFileCommand.Execute(details); Close(); };
            BtnDelete.Click += async (_, __) =>
            {
                try
                {
                    bool ok = await RDPro.Services.UiService.ShowConfirmAsync("Delete Connection", $"Delete connection '{details.ConnectionName}'? This cannot be undone.");
                    if (ok && vm.DeleteConnectionCommand.CanExecute(details)) vm.DeleteConnectionCommand.Execute(details);
                }
                catch (Exception ex)
                {
                    try { Console.WriteLine($"ConnectionActionsWindow delete error: {ex.Message}"); } catch { }
                }

                Close();
            };
        }

        private void Close_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
