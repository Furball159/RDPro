using Avalonia.Controls;

namespace RDPro.Views
{
    public partial class GatewayAcc : UserControl
    {
        public GatewayAcc()
        {
            InitializeComponent();
            this.DataContext = new RDPro.ViewModels.GatewayManagementViewModel();
        }
    }
}
