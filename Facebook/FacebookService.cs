using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Facebook;
using Newtonsoft.Json;
using System.Net.Http;
using System.Web.Mvc;
using System.Threading.Tasks;
using Sabio.Web.Models.Requests.Login;

namespace Sabio.Web.Services
{
    public class FacebookService
    {
        private static FacebookClient fb = new FacebookClient();

        public static FacebookMeModel GetMeObject(string accessToken)
        {
            FacebookClient fbClient = new FacebookClient(accessToken);

            FacebookMeModel fbMe = fbClient.Get<FacebookMeModel>("me", new { fields = new[] { "name", "email", "education", "friends", "picture{ url, height, is_silhouette, width }","first_name","last_name","middle_name" } });

            return fbMe;       
        }

        public static string GetLoginUrl()
        {
            return fb.GetLoginUrl(new { }).ToString();
        }

    }
}