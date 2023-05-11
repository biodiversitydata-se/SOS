using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.UserService;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.Managers.Interfaces
{
    /// <summary>
    ///     User manager interface.
    /// </summary>
    public interface IUserManager
    {
        IUserService UserService { get; set; }
        Task<UserInformation> GetUserInformationAsync(string applicationIdentifier, string cultureCode);
    }
}