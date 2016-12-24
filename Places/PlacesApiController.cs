using Sabio.Web.Domain;
using Sabio.Web.Models.Requests.MyMedia;
using Sabio.Web.Models.Requests.Places;
using Sabio.Web.Models.Responses;
using Sabio.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Sabio.Web.Core.Extensions;
using Sabio.Web.Domain.ES.Query;
using Sabio.Web.Enums;
using Sabio.Web.Models.SystemEvents;
using Sabio.Web.Services.ES;
using Sabio.Web.Services.Interface;
using Sabio.Web.Utils;
using Hangfire;
using Sabio.Web.Background.Tasks;
using WebApi.OutputCache.V2;
using Sabio.Web.Models.Requests.Pagination;

namespace Sabio.Web.Controllers.Api
{
    [RoutePrefix("api/places")]
    public class PlacesApiController : BaseApiController
    {
        public const string EntityIndex = "places";

        [Dependency]
        public IPlacesService _PlacesService { get; set; }

        [Dependency]
        public IEntityEsService EntityService { get; set; }

        [Dependency]
        public IAttributeValueService _attributeValueService { get; set; }

        [Dependency]
        public IAttributeValueService AttributeValueService { get; set; }

        [Dependency]
        public ISystemEventsService SystemEventsService { get; set; }

        [Dependency]
        public ICityPageService CityPageService { get; set; }

        //Post
        [Route(), HttpPost]
        [Authorize]
        public HttpResponseMessage Add(PlacesAdd Model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            Model.UserId = UserService.GetCurrentUserId();

            Places newplace = _PlacesService.Insert(Model);

            ItemResponse<Places> Response = new ItemResponse<Places>();

            Response.Item = newplace;

            var place = _PlacesService.GetPlaceByExternalPlaceId(Model.ExtPlaceId);
            var extraAttributes = AttributeValueService.GetAttributeValues(place.Category.TagName, place.Id)
                .ToDictionary(p => p.AttributeName, p => (object)p.Value).ToDynamic();

            EntityService.AddEntityObject(EntityIndex, place.Category.TagName, place.ExtPlaceId, Mapper.Map<PlacesDto>(place), extraAttributes);

            SystemEventsService.AddSystemEvent(new AddSystemEventModel
            {
                ActorUserId = Model.UserId,
                ActorType = ActorType.User,
                EventType = SystemEventType.UserDiscoverNewPlace,
                TargetId = newplace.Id,
                TargetType = TargetType.Place
            });
            BackgroundJob.Enqueue<PlacesRatingTask>(task => task.Run(place.Id, null, JobCancellationToken.Null));
            return Request.CreateResponse(HttpStatusCode.OK, Response);
        }

        //Get By ID
        [Route("{id:int}"), HttpGet]
        [CacheOutput(ServerTimeSpan = 120)]
        public HttpResponseMessage Get(int id)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            Places newplace = _PlacesService.GetPlace(id);

            ItemResponse<Places> Response = new ItemResponse<Places>();

            Response.Item = newplace;

            return Request.CreateResponse(HttpStatusCode.OK, Response.Item);
        }

        //Get By Slug
        [Route("{Slug}"), HttpGet]
        //[CacheOutput(ServerTimeSpan = 120)]
        public HttpResponseMessage GetSlug(string Slug)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            Places newPlaceSlug = _PlacesService.GetPlaceSlug(Slug);

            ItemResponse<Places> Response = new ItemResponse<Places>();

            Response.Item = newPlaceSlug;

            return Request.CreateResponse(HttpStatusCode.OK, Response.Item);
        }

        //Get By List
        [Route(), HttpGet]
        public HttpResponseMessage GetAll([FromUri]PaginatedRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            int TotalCount = 0;

            List<Places> Listofplaces = _PlacesService.GetList(model , ref TotalCount);

            PagedItemsResponse<Places> Response = new PagedItemsResponse<Places>();

            Response.Items = Listofplaces;
            Response.CurrentPage = model.CurrentPage;
            Response.ItemsPerPage = model.ItemsPerPage;
            Response.TotalItemCount = TotalCount;

            return Request.CreateResponse(HttpStatusCode.OK, Response);
        }

        //Update By ID
        [Route("{id:int}"), HttpPut]
        public HttpResponseMessage Put(PlacesUpdate Model, int id)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            _PlacesService.Update(Model, id);

            ItemResponse<bool> Response = new ItemResponse<bool>();

            Response.IsSuccessful = true;

            var place = _PlacesService.GetPlaceByExternalPlaceId(Model.ExtPlaceId);
            var extraAttributes = AttributeValueService.GetAttributeValues(place.Category.TagName, place.Id)
                .ToDictionary(p => p.AttributeName, p => (object)p.Value).ToDynamic();

            EntityService.AddEntityObject(EntityIndex, place.Category.TagName, place.ExtPlaceId, Mapper.Map<PlacesDto>(place), extraAttributes);

            RemoveCacheItem((PlacesApiController c) => c.Get(id));
            RemoveCacheItem((PlacesApiController c) => c.GetSlug(place.Slug));

            return Request.CreateResponse(HttpStatusCode.OK, Response);
        }

        //Delete By ID
        [Route("{id:int}"), HttpDelete]
        public BaseResponse Delete(int id)
        {
            var place = _PlacesService.GetPlace(id);

            if (place == null)
                return Fail($"Place with id {id} does not exist!");

            //TODO: Retrieve category in GetPlace(id) so we don't have to do this extra call
            var placeWithCategory = _PlacesService.GetPlaceByExternalPlaceId(place.ExtPlaceId);

            EntityService.DeleteEntityObject(EntityIndex, placeWithCategory.Category.TagName, placeWithCategory.ExtPlaceId);

            AttributeValueService.DeleteAttributeValues(placeWithCategory.Category.TagName, id);

            _PlacesService.Delete(id);

            return Success();
        }

        //Get By UserID
        [Route("userid/{userId}"), HttpGet]
        //[Authorize]
        public HttpResponseMessage SelectByUserId(string userId)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            List<Places> newUserIdPlaces = _PlacesService.GetUserIdPlace(userId);

            ItemsResponse<Places> Response = new ItemsResponse<Places>();

            Response.Items = newUserIdPlaces;

            return Request.CreateResponse(HttpStatusCode.OK, Response);
        }
        //get by category ID

        [Route("discover")]
        [HttpGet]
        public ItemsResponse<dynamic> Discover([FromUri] DiscoverPlace model)
        {
            if(model.City != null && model.City.ToLower() != "all")
            {
                CityPage City = CityPageService.GetCitySlug(model.City);
                model.Latitude = double.Parse(City.Latitude.ToString());
                model.Longitude = double.Parse(City.Longitude.ToString());
                model.Distance = City.Radius;
            }

            if (model.ExtPlaceIds?.Length > 0)
            {
                var entitiesById = EntityService.GetEntities(EntityIndex, model.ExtPlaceIds);

                return Items(entitiesById);   
            }

            var dto = Mapper.Map<DiscoverPlaceDto>(model);
            var query = EntityUtils.BuildQuery(dto);
            var entities = EntityService.QueryEntities(EntityIndex, query, (model.CurrentPage - 1) * model.ItemsPerPage, model.ItemsPerPage);

            return Items(entities);
        }

        [Route("media"), HttpPost]
        public HttpResponseMessage InsertMedia(MediaPlacesRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            _PlacesService.InsertMediaAndPlacesById(model);

            ItemResponse<bool> Response = new ItemResponse<bool>();

            Response.IsSuccessful = true;

            return Request.CreateResponse(HttpStatusCode.OK, Response);
        }

        [Route("media/{placesId:int}"), HttpGet]
        public HttpResponseMessage GetplacesMedia(int placesId, [FromUri] PaginatedRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
    
            PaginatedDomain<MediaPlacesDomain> Media = _PlacesService.GetMediaByPlacesId(placesId, model);

            PagedItemsResponse<MediaPlacesDomain> response = new PagedItemsResponse<MediaPlacesDomain>();

            response.Items = Media.Items;
            response.TotalItemCount = Media.TotalItems;
            response.CurrentPage = model.CurrentPage;
            response.ItemsPerPage = model.ItemsPerPage;

            return Request.CreateResponse(HttpStatusCode.OK, response);

        }

        [Route("media/header/{placesId:int}"), HttpGet]
        public HttpResponseMessage GetHeaderPlacesMedia(int placesId)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            MediaPlacesDomain Media = _PlacesService.getHighestMediaPlaesId(placesId);

            ItemResponse<MediaPlacesDomain> response = new ItemResponse<MediaPlacesDomain>();

            response.Item = Media;

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        // GET: Favorite Places by UserID
        [Route("favoriteType/{favoriteType:int}"), HttpGet]
        [Authorize]
        public HttpResponseMessage GetByCurrentUserIdUserFavoritePlaces(int favoriteType)
        {
            var UserId = UserService.GetCurrentUserId();

            List<UserFavoritePlacesTypeDomain> ListDomain = _PlacesService.GetByUserIdAndFavoritePlacesType(UserId, favoriteType);

            ItemsResponse<UserFavoritePlacesTypeDomain> response = new ItemsResponse<UserFavoritePlacesTypeDomain>();

            response.Items = ListDomain;

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}
