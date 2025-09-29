using Avalonia.Controls;
using RDPro.ViewModels;

namespace RDPro
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }
    }
}