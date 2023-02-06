using Flurl.Http;
using Prism.Services.Dialogs;
using SvadbeniSalon.WinUI.Commands;
using SvadbeniSalon.WinUI.Interfaces;
using SvadbeniSalon.WinUI.Models;
using SvadbeniSalon.WinUI.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace SvadbeniSalon.WinUI.ViewModels
{
    public class LoginVM : ViewModelBase, ICloseWindow
    {
        private string _username;
        private string _password;
        private bool _loginIncorrect;
        private GenericService service;

        public event Action<IDialogResult> RequestClose;

        public CustomCommand LoginCommand { get; set; }
        public CustomCommand PasswordChangedCommand { get; set; }
        public Dictionary<string, string> ErrorCollection { get; private set; } = new Dictionary<string, string>();

        public LoginVM()
        {
            service = new GenericService("users");
            LoadCommands();
        }
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
        public bool LoginIncorrect
        {
            get { return _loginIncorrect; }
            set
            {
                SetProperty(ref _loginIncorrect, value);
            }
        }
        public string Title => "Login";

        public Action Close { get; set; }

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
            if (obj == null)
                return false;

            var isFormValid = IsValid(obj as DependencyObject);

            return isFormValid;
        }

        private async void Login(object obj)
        {
            try
            {
                var response = await service.PostAsync(Actions.User.Authenticate, new
                {
                    username = Username,
                    password = Password
                });

                await AppGlobal.Instance.Init(response, Username);
                Close?.Invoke();
            }
            catch (FlurlHttpException ex)
            {
                if (ex.StatusCode != null && (HttpStatusCode)ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    LoginIncorrect = true;
                }
            }
        }

        private bool IsValid(DependencyObject obj)
        {
            // The dependency object is valid if it has no errors and all
            // of its children (that are dependency objects) are error-free.
            return !Validation.GetHasError(obj) &&
            LogicalTreeHelper.GetChildren(obj)
            .OfType<DependencyObject>()
            .All(IsValid);
        }

    }
}
