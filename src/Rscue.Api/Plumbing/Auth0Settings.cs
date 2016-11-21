using System;
using System.Collections.Generic;

namespace Rscue.Api.Plumbing
{
    public class Auth0Settings
    {
        public const string Connection = "Username-Password-Authentication";

        public string ManagementUserToken { get; set; }

        public string Domain { get; set; }
    }
}