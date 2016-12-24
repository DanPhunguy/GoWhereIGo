using Sabio.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sabio.Web.Controllers
{
    [RoutePrefix("places")]
    public class PlacesController : Controller
    {
        // GET: Places
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Home()
        {
            return View("HomeNg");
        }

        // UPDATE/CREATE: Places
        //[Route("manage/{id:int}")]
        //[Route("create")]
        //public ActionResult IndexTheme(int? id = null)
        //{
        //    ItemViewModel<int?> vm = new ItemViewModel<int?>();
        //    vm.Item = id;
        //    return View("IndexTheme", vm);
        //}

        [Route("manage/{id:int}")]
        [Route("create")]
        public ActionResult IndexAngular(int? id = null)
        {
            ItemViewModel<int?> vm = new ItemViewModel<int?>();
            vm.Item = id;
            return View("IndexAngular", vm);
        }

        // ROUTE: Slug URL
        [Route("singleplace/{slug}")]
        public ActionResult SinglePlace(string slug)
        {
            ItemViewModel<string> vm = new ItemViewModel<string>();
            vm.Item = slug;
            return View("SinglePlaceNg", vm);
        }

        // GET: Places by UserID 
        [Route("userid/{userId}")]
        public ActionResult UserId(string userId)
        {
            ItemViewModel<string> vm = new ItemViewModel<string>();
            vm.Item = userId;
            return View("UserPlaces", vm);
        }
    }
}
