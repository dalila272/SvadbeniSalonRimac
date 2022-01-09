using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

namespace SvadbeniSalon.WinUI.Service
{
    public class GenericService
    {
        private string _resource;
        private string endpoint = string.Empty;

        public GenericService(string resource)
        {
            _resource = resource;
            endpoint = ConfigurationManager.AppSettings["endpoint"];
        }


        public async Task<string> PostAsync(string action, object request)
        {
            // (AuthorizedMethodAttribute) Attribute.GetCustomAttribute(action.GetType(), typeof(AuthorizedMethodAttribute));
            var url = $"{endpoint}/{_resource}{(string.IsNullOrEmpty(action) ? string.Empty : $"/{action}")}";
            return await url.PostJsonAsync(request).ReceiveString();
        }

        public async Task<T> GetAsync<T>(string action)
        {
            var url = $"{endpoint}/{_resource}/{action}";

            return await url.GetJsonAsync<T>();
        }
    }
}
