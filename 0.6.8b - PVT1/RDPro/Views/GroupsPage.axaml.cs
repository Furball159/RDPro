using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RDPro.ViewModels;

namespace RDPro.Views
{
    public partial class GroupsPage : UserControl
    {
        public GroupsPage()
        {
            InitializeComponent();
            DataContext = new GroupsViewModel();
        }
    }
}
