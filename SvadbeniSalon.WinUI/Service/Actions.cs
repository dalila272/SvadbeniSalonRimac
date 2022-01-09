using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.WinUI.Service
{
    public static class Actions
    {
        public static class Resources
        {
            public const string User = "users";
        }
        public static class User
        {
            [AuthorizedMethod(false)]
            public const string Authenticate = "authenticate";
            [AuthorizedMethod(false)]
            public const string GetByUsername = "username";
        }
    }
}
