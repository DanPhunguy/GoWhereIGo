using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Sabio.Web.Domain;
using Sabio.Web.Enums;
using Sabio.Web.Models.Requests.Login;
using System;
using Sabio.Web.Models.Responses;
using Sabio.Web.Services;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Sabio.Web.Services.Interface;

namespace Sabio.Web.Controllers
{
    [RoutePrefix("Login")]
    public class LoginController : BaseController
    {
        [Dependency]
        public IExternalUserService _externalUserService { get; set; }

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Facebook()
        {
            // Facebook requires an access token 
            // Calls on facebook to send an accesstoken .
            string redirectUrl = Url.Action("FacebookCallback", "Login", new { }, Request.Url.Scheme);
            string facebookUrl = $"https://www.facebook.com/v2.8/dialog/oauth?client_id={ConfigService.facebookAppId}&redirect_uri={redirectUrl}&scope=email,public_profile,user_friends,user_education_history,user_photos";

            return Redirect(facebookUrl);
        }

        public async Task<ActionResult> FacebookCallback(string code)
        {
            //extract the accesstoken
            string redirectUrl = Url.Action("FacebookCallback", "Login", new { }, Request.Url.Scheme);
            string url = $"https://graph.facebook.com/v2.8/oauth/access_token?client_id={ConfigService.facebookAppId}&redirect_uri={redirectUrl}&client_secret={ConfigService.facebookAppSecret}&code={code}";

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);

            string json = await response.Content.ReadAsStringAsync();

            FacebookResponse facebookResp = JsonConvert.DeserializeObject<FacebookResponse>(json);

            string accessToken = facebookResp.access_token;

            //Gets the user data from facebook
            FacebookMeModel me = FacebookService.GetMeObject(accessToken);

            // Use the ExternalUserService to see if the user has already sign up to GWIG
            me.ExternalUserServiceType = ExternalUserServiceType.Facebook;
            ExternalUser user = _externalUserService.GetExternalUserServiceByExtUserId(me.id, me.ExternalUserServiceType);

            
             await _externalUserService.FacebookSignin(user, me);
          
                return RedirectToAction("","dashboard");
           

        }


    }
}