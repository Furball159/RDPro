using Avalonia.Controls;
using RDPro.ViewModels;

namespace RDPro
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Use MainViewModel so both AllConnectionsVM and FavouritesVM are available to the views
            this.DataContext = new RDPro.ViewModels.MainViewModel();
        }
    }
}