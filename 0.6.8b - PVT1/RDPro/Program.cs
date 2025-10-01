using Avalonia;
using Avalonia.ReactiveUI; // ✅ this comes from Avalonia.ReactiveUI package
using System;


namespace RDPro
{
    internal static class Program
    {
        // The main entry point of the application
        [STAThread]
        public static void Main(string[] args) =>
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI(); // ✅ Required for ReactiveUI
    }
}
