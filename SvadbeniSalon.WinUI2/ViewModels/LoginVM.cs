using SvadbeniSalon.WinUI.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace SvadbeniSalon.WinUI.ViewModels
{
    public class LoginVM : ViewModelBase, IDataErrorInfo
    {
        private string _username;
        private string _password;

        public CustomCommand LoginCommand { get; set; }
        public CustomCommand PasswordChangedCommand { get; set; }

        public string Username
        {
            get { return _username; }
            set
            {
                SetProperty(ref _username, value);
                LoginCommand.RaiseCanExecuteChanged();
            }
        }
        public string Password
        {
            get { return _password; }
            set
            {
                SetProperty(ref _password, value);
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                string result = null;
                if (columnName == "Username")
                {
                    if (string.IsNullOrEmpty(Username))
                        result = "Username is required";
                }

                return result;
            }
        }

        public LoginVM()
        {
            LoadCommands();
        }

        private void LoadCommands()
        {
            LoginCommand = new CustomCommand(Login, CanLogin);
            PasswordChangedCommand = new CustomCommand(PasswordChanged, null);
        }

        private void PasswordChanged(object obj)
        {
            Password = (obj as PasswordBox).Password;
        }

        private bool CanLogin(object obj)
        {
            return !(string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password));
        }

        private void Login(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
