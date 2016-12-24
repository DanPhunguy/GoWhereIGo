using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Sabio.Web.Domain;
using Sabio.Web.Enums;
using Sabio.Data;
using Sabio.Data.Extensions;
using Sabio.Web.Models.Requests.ExternalUserServices;
using System.Data.SqlClient;
using Sabio.Web.Models.Requests.Login;
using System.Web.Security;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNet.Identity.EntityFramework;
using Sabio.Web.Models.Requests;
using Sabio.Web.Models.Requests.MyMedia;
using Sabio.Web.Services.MyMedia_Services;
using Sabio.Web.Models;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Sabio.Web.Services.Interface;
using Sabio.Web.Models.Requests.Followers;

namespace Sabio.Web.Services
{
    public class _externalUserService : BaseService, IExternalUserService
    {
        [Dependency]
        public IMediaService _mediaService { get; set; }

        [Dependency]
        public IUserProfileService _userProfileService { get; set; }

        [Dependency]
        public IFollowersService _followersService { get; set; }

        public int AddExternalUser(string userId, CreateExternalUserRequest request)
        {
            int id = 0;

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.ExternalUserServices_Insert", param =>
            {
                param.AddWithValue("@UserId", userId);
                param.AddWithValue("@ServiceType", request.ServiceType);
                param.AddWithValue("@ExternalUserId", request.ExternalUserId);
                param.AddWithValue("@ExternalUsername", request.ExternalUsername);
                param.AddWithValue("@ExternalUserToken", request.ExternalUserToken);
                
                param.AddOutput("@Id", SqlDbType.Int);
            }, ret =>
            {
                id = ret.GetOutput<int>("@Id");
            });

            return id;
        }

        public ExternalUser GetExternalUserByUserIdService(string userId, ExternalUserServiceType type)
        {
            ExternalUser user = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.ExternalUserServices_SelectByUserIdAndService", param =>
            {
                param.AddWithValue("@UserId", userId);
                param.AddWithValue("@ServiceType", (int)type);
            }, (map, set) =>
            {
                user = new ExternalUser
                {
                    UserId = map.GetSafeString(0),
                    ServiceType = (ExternalUserServiceType)map.GetSafeInt32(1),
                    ExternalUserId = map.GetSafeString(2),
                    ExternalUsername = map.GetSafeString(3),
                    ExternalUserToken = map.GetSafeString(4)
                };
            });

            return user;
        }

        public ExternalUser GetExternalUserServiceByExtUserId(string extUserId, ExternalUserServiceType type)
        {
            ExternalUser user = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.ExternalUserServices_SelectByTypeAndExternalUserID", param =>
            {
                param.AddWithValue("@ExternalUserId", extUserId);
                param.AddWithValue("@ServiceType", (int)type);
            }, (map, set) =>
            {
                user = new ExternalUser
                {
                    UserId = map.GetSafeString(0),
                    ServiceType = (ExternalUserServiceType)map.GetSafeInt32(1),
                    ExternalUserId = map.GetSafeString(2),
                    ExternalUsername = map.GetSafeString(3),
                    ExternalUserToken = map.GetSafeString(4)
                };
            });

            return user;
        }

        public string ConvertName(string inputname)
        {
            string name = inputname.ToLowerInvariant();
            byte[] bytesfirstname = Encoding.GetEncoding("Cyrillic").GetBytes(name);
            name = Encoding.ASCII.GetString(bytesfirstname);
            name = Regex.Replace(name, @"\s", "-", RegexOptions.Compiled);

            return name;
        }

       public void addingExternalFriends( string userId, FacebookMeModel me)
        {
            for (int i = 0; i < me.friends.data.Count; i++)
            {
                ExternalUser ExternalUserForFriends = GetExternalUserServiceByExtUserId(me.friends.data[i].id, ExternalUserServiceType.Facebook);
                if (ExternalUserForFriends != null)
                {
                    bool isfollowing = _userProfileService.DecorateisFollowingAndNotLoggedin(ExternalUserForFriends.UserId, userId);
                    if (isfollowing != true)
                    {
                        FollowersAddRequest model = new FollowersAddRequest();
                        model.FollowingUserId = ExternalUserForFriends.UserId;
                        model.FollowerUserId = userId;                      
                        _followersService.InsertTest(model);
                    }
                }
            }
        }

        public async Task<bool> FacebookSignin(ExternalUser user, FacebookMeModel me)
        {
            //Check to see if the user is in the externaldatabase, if not the function will create one.
            if (user == null)
            {
                //check to see if the user facebook email already has an account with the same email address in GWIG database
                
                bool IsUser = UserService.IsUser(me.email);

                //If both checks out false, the database will create a new account
                if (IsUser != true)
                {
                    string randomPassword = Membership.GeneratePassword(16, 0);

                    string middleName = null;
                    string newUserName = null;

                    string firstName = ConvertName(me.first_name);
                   
                    string lastName = ConvertName(me.last_name);

                    if (me.middle_name != null)
                    {

                        middleName = ConvertName(me.middle_name);

                        //middleName = me.middle_name.ToLowerInvariant();
                        //byte[] bytesmiddlename = Encoding.GetEncoding("Cyrillic").GetBytes(middleName);
                        //middleName = Encoding.ASCII.GetString(bytesmiddlename);
                        //middleName = Regex.Replace(middleName, @"\s", "-", RegexOptions.Compiled);
                    }

                    string emailuserName = me.email.Split('@')[0];

                    if (middleName == null)
                    {
                        newUserName = firstName + "-" + lastName + "-" + emailuserName;
                    }
                    else
                    {
                        newUserName = firstName + "-" + middleName + "-" + lastName + "-" + emailuserName;
                    }

                    //ASP.NET USER
                    IdentityUser newUserRegistration = UserService.CreateUser(me.email, randomPassword, newUserName);

                    //UserProfile
                    CreateUserProfileJsonData CreateUser = new CreateUserProfileJsonData();
                    CreateUser.firstName = me.first_name;
                    CreateUser.lastName = me.last_name;
                    CreateUser.userName = newUserName;
                    _userProfileService.CreateProfile(newUserRegistration.Id, CreateUser);

                    //ExternalUser
                    CreateExternalUserRequest ExternalUser = new CreateExternalUserRequest();
                    ExternalUser.ServiceType = ExternalUserServiceType.Facebook;
                    ExternalUser.ExternalUsername = newUserName;
                    ExternalUser.ExternalUserId = me.id;
                    AddExternalUser(newUserRegistration.Id, ExternalUser);

                    //Facebook Profile Picture
                    if (me.picture.data.is_silhouette != true)
                    {
                        MediaAddRequest Media = new MediaAddRequest();
                        Media.MediaType = MediaType.Facebook;
                        Media.UserId = newUserRegistration.Id;
                        Media.Title = "Facebook Profile";
                        Media.Url = me.picture.data.url;

                        int Mediaid = UnityConfig.GetContainer().Resolve<IMediaService>().InsertTest(Media);

                        updateUserProfilePicture profileupdate = new updateUserProfilePicture();
                        profileupdate.mediaId = Mediaid;
                        profileupdate.userId = Media.UserId;
                        _userProfileService.updateProfilePicture(profileupdate);
                    }

                }
                else
                {
                    //Linking Facebook account with GWIG by emailaddress

                    ApplicationUser Adduser = UserService.GetUser(me.email);

                    CreateExternalUserRequest ExternalUser = new CreateExternalUserRequest();
                    ExternalUser.ServiceType = ExternalUserServiceType.Facebook;
                    ExternalUser.ExternalUsername = Adduser.UserName;
                    ExternalUser.ExternalUserId = me.id;
                    AddExternalUser(Adduser.Id, ExternalUser);


                    addingExternalFriends(Adduser.Id, me);

                    bool diditwork = await UserService.ForceUsernameLoginAsync(Adduser.UserName);
                    return diditwork;
                }
            }


            ApplicationUser theuser = UserService.GetUser(me.email);

            addingExternalFriends(theuser.Id, me);

            bool ihopethisworks = await UserService.ForceUsernameLoginAsync(theuser.UserName);

            return ihopethisworks;
        }

    }
}