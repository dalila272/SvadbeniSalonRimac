using Flurl.Http;
using SvadbeniSalon.WinUI.Service;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SvadbeniSalon.WinUI.Models
{
    public sealed class AppGlobal
    {
        private string access_token;
        private DateTime tokenExpirationDate;
        public User CurrentUser { get; private set; }
        private static AppGlobal instance = null;
        private GenericService usersService;


        private AppGlobal()
        {
            usersService = new GenericService(Actions.Resources.User);
        }


        public async Task Init(string token, string username)
        {
            SetAccessToken(token);
            await LoadUser(username);
        }

        private async Task LoadUser(string username)
        {
            var action = $"{Actions.User.GetByUsername}/{username}";

            try
            {
                CurrentUser = await usersService.GetAsync<User>(action);
            }
            catch (FlurlHttpException ex)
            {

            }
        }

        public static AppGlobal Instance
        {
            get
            {
                if (instance == null)
                    instance = new AppGlobal();
                return instance;
            }
        }

        public void SetAccessToken(string token)
        {
            access_token = token;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(access_token);

            tokenExpirationDate = jwtToken.ValidTo;
        }

        public bool TokenExpired()
        {
            return DateTime.UtcNow > tokenExpirationDate.AddMinutes(-1);
        }
    }
}
