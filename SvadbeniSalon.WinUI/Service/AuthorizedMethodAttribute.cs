using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.WinUI.Service
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public class AuthorizedMethodAttribute : Attribute
    {
        private bool isAuthorized { get; set; }
        public AuthorizedMethodAttribute(bool isAuthorized)
        {
            this.isAuthorized = isAuthorized;
        }

        public bool IsAuthorized { get { return isAuthorized; } }
    }
}
