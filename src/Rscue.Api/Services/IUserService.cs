using Rscue.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rscue.Api.Services
{
    public interface IUserService
    {
        Task<UserRegistration> RegisterUserAsync(UserRegistration userRegistration);
    }
}
