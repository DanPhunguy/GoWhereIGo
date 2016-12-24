using Sabio.Web.Domain;
using Sabio.Web.Enums;
using Sabio.Web.Models.Requests.ExternalUserServices;
using Sabio.Web.Models.Requests.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabio.Web.Services.Interface
{
    public interface IExternalUserService
    {
        int AddExternalUser(string userId, CreateExternalUserRequest request);

        ExternalUser GetExternalUserByUserIdService(string userId, ExternalUserServiceType type);

        ExternalUser GetExternalUserServiceByExtUserId(string extUserId, ExternalUserServiceType type);

        Task<bool> FacebookSignin(ExternalUser user, FacebookMeModel me);

        string ConvertName(string inputname);
    }
}
