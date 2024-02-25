using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using coeffbot.ViewModels;
using coeffbot.Views;
using coeffbot.WS;

namespace coeffbot
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {

                mainVM main = new mainVM();
                WindowService.getInstance().ShowWindow(main);


                //desktop.MainWindow = new mainWnd()
                //{
                //    DataContext = new mainVM(),
                //};
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
