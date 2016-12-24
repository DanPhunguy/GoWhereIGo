using Sabio.Web.Models.Requests.Yelp;
using Sabio.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static Sabio.Web.Models.Requests.Yelp.YelpRequest;

namespace Sabio.Web.Controllers.Api
{
    [RoutePrefix("api/yelp")]
    public class YelpApiController : ApiController
    {
        [Route(), HttpPost]
        public HttpResponseMessage yelpGet(YelpSearch model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            YelpService y = new YelpService();
            var results = y.Search(model.term, model.limit, model.Latitude, model.Longitude).Result;

            return Request.CreateResponse(HttpStatusCode.OK, results);
        }
        //[Route("discover"), HttpPost]
        //public HttpResponseMessage yelpGetDiscover(YelpDiscover model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        //    }

        //    YelpService y = new YelpService();
        //    var results = y.SearchDiscover(model.location, model.limit, model.term ).Result;

        //    return Request.CreateResponse(HttpStatusCode.OK, results);
        //}



    }
}
