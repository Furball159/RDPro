using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RDPro.ViewModels;

/// <summary>
/// Base class for all view models. Implements INotifyPropertyChanged
/// so UI bindings update automatically without needing ReactiveUI.
/// </summary>
public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!Equals(field, value))
        {
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        return false;
    }
}
