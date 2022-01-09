using Prism.DryIoc;
using Prism.Ioc;
using Prism.Services.Dialogs;
using SvadbeniSalon.WinUI.ViewModels;
using SvadbeniSalon.WinUI.Views;
using System;
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
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

            containerRegistry.Register<Login>();
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
