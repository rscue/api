namespace Rscue.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class UserRegistration
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool IsAdmin { get; set; }

        public string WorkerId { get; set; }

        public string ProviderId { get; set; }

        public string ClientId { get; set; }
    }
}
