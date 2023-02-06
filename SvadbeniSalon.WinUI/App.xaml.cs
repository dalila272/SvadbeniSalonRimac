using Prism.DryIoc;
using Prism.Ioc;
using SvadbeniSalon.WinUI.ViewModels;
using SvadbeniSalon.WinUI.Views;
using System.Windows;

namespace SvadbeniSalon.WinUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainMenuPage>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<Login, LoginVM>();
            containerRegistry.RegisterForNavigation<MainMenuPage, MainPageViewModel>();
            containerRegistry.RegisterForNavigation<Zaposlenici, ZaposleniciViewModel>();
        }
        protected override void OnInitialized()
        {
            var loginDialog = Container.Resolve<Login>();

            loginDialog.ShowDialog();

            if (loginDialog.DialogResult.Value)
            {
                base.OnInitialized();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
    }
}
