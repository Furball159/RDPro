using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Media;

namespace RDPro.Services;

public static class UiService
{
    // Simple modal message box built with Avalonia controls so we don't add a dependency.
    public static async Task ShowMessageAsync(string title, string message)
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var dialog = new Window
                {
                    Title = title,
                    Width = 480,
                    Height = 160,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    CanResize = false,
                };

                var panel = new StackPanel { Margin = new Thickness(12) };
                var txt = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap };
                var ok = new Button { Content = "OK", HorizontalAlignment = HorizontalAlignment.Right, Width = 80 };
                ok.Click += (_, _) => dialog.Close();

                panel.Children.Add(txt);
                panel.Children.Add(ok);

                dialog.Content = panel;

                if (desktop.MainWindow != null)
                {
                    await dialog.ShowDialog(desktop.MainWindow);
                }
                else
                {
                    // Fallback to non-modal show if there's no main window
                    dialog.Show();
                }
            }
        }
        catch
        {
            // If UI show fails, fallback to console.
            System.Console.WriteLine($"{title}: {message}");
        }
    }

    // Modal confirmation dialog. Returns true if user clicked Yes/Confirm.
    public static async Task<bool> ShowConfirmAsync(string title, string message, string confirmText = "Yes", string cancelText = "No")
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var dialog = new Window
                {
                    Title = title,
                    Width = 480,
                    Height = 160,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    CanResize = false,
                };

                var panel = new StackPanel { Margin = new Thickness(12) };
                var txt = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap };
                var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right };

                bool result = false;

                var ok = new Button { Content = confirmText, Width = 80, Margin = new Thickness(6,0) };
                var cancel = new Button { Content = cancelText, Width = 80, Margin = new Thickness(6,0) };

                ok.Click += (_, _) => { result = true; dialog.Close(); };
                cancel.Click += (_, _) => { result = false; dialog.Close(); };

                buttons.Children.Add(ok);
                buttons.Children.Add(cancel);

                panel.Children.Add(txt);
                panel.Children.Add(buttons);

                dialog.Content = panel;

                if (desktop.MainWindow != null)
                {
                    await dialog.ShowDialog(desktop.MainWindow);
                }
                else
                {
                    dialog.Show();
                }

                return result;
            }
        }
        catch
        {
            // Fallback: write to console and return false to be safe
            System.Console.WriteLine($"CONFIRM: {title}: {message}");
        }

        return false;
    }
}
