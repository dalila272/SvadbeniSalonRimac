using SvadbeniSalon.WinUI.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.WinUI.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel()
        {
            OpenPageCommand = new CustomCommand(OpenSelectedPage, null);
        }

        private void OpenSelectedPage(object obj)
        {
          //  Password = (obj as PasswordBox).Password;

        }

        public CustomCommand OpenPageCommand { get; set; }

    }
}