using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Newtonsoft.Json;
using Sabio.Web.Domain;
using Sabio.Web.Enums;
using Sabio.Web.Models.Requests.MyMedia;
using Sabio.Web.Models.Requests.Rating;
using Sabio.Web.Services;
using Sabio.Web.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using YelpSharp;
using YelpSharp.Data;
using static Sabio.Web.Services.GooglePlacesService;

namespace Sabio.Web.Background.Tasks
{
    public class PlacesRatingTask
    {
        private readonly IRatingService _ratingService;
        private readonly IYelpService _yelpService;
        private readonly IPlacesService _placesSerivce;
        private readonly IGooglePlacesService _googleService;
        private readonly IMediaService _mediaService;

        public PlacesRatingTask(
            IRatingService ratingService,
            IPlacesService placesService,
            IYelpService yelpSerivce,
            IGooglePlacesService googleService,
            IMediaService mediaService
            )
        {
            _ratingService = ratingService;
            _placesSerivce = placesService;
            _yelpService = yelpSerivce;
            _googleService = googleService;
            _mediaService = mediaService;
        }

        [AutomaticRetry(Attempts = 0)]
        public void Run(int id, PerformContext context, IJobCancellationToken cancellationToken)
        {
            Places PlacesDomain = _placesSerivce.GetPlace(id);

            if (PlacesDomain == null)
            {
                context.WriteLine($"Could not find a place with id {id}!");

                return;
            }

            if (PlacesDomain.Address == null)
            {
                context.WriteLine($"Place {PlacesDomain.Name} does not have an address associated with it!");

                return;
            }

            SearchResults yelpResult = _yelpService.Search(PlacesDomain.Name, PlacesDomain.Address.Address1, 1, 1).Result;

            PlacesRatingsRequest model = new PlacesRatingsRequest();

            model.PlaceId = PlacesDomain.Id;
            model.RatingType = RatingType.Yelp;
            model.Rating = Convert.ToDecimal(yelpResult.businesses[0].rating);
            model.UserId = PlacesDomain.UserId;
            //model.GroupId = 0;
            //model.AspectId = 0;

            int placesRatingId = _ratingService.PostPlacesRatingInsert(model);

            GooglePlaceDetailsResponse googleResult = _googleService.GetDetails(PlacesDomain.ExtPlaceId).Result;

            var result = googleResult.Result;

            PlacesRatingsRequest googleModel = new PlacesRatingsRequest();

            googleModel.PlaceId = PlacesDomain.Id;
            googleModel.RatingType = RatingType.GooglePlaces;
            googleModel.Rating = Convert.ToDecimal(result.Rating);
            googleModel.UserId = PlacesDomain.UserId;

            int placesRatingGoogleId = _ratingService.PostPlacesRatingInsert(googleModel);
            //GooglePlaceSearchResponse googleResult = _googleService.TextSearch(PlacesDomain.Name + " " + PlacesDomain.Address.Address1).Result;

            if (result.Photos != null)
            {
                for (int i = 0; i < result.Photos.Count; i++)
                {
                    int height = result.Photos[i].Height;
                    int width = result.Photos[i].Width;

                    string PhotoReference = result.Photos[i].PhotoReference;

                    WebRequest webRequest = WebRequest.Create($"https://maps.googleapis.com/maps/api/place/photo?maxwidth={width}&maxheight={height}&photoreference={PhotoReference}&key={ConfigService.GoogleApiKey}");
                    webRequest.Method = "GET";
                    WebResponse Response = webRequest.GetResponse();
                    //  Response.ContentType = "image/png";


                    MediaAddRequest MediaModel = new MediaAddRequest();
                    MediaModel.UserId = "Hangfire";
                    MediaModel.MediaType = MediaType.GooglePlaces;
                    MediaModel.DataType = "image/png";
                    MediaModel.Title = PlacesDomain.Name + " photo Number: " + i;
                    MediaModel.Description = "Photo of " + PlacesDomain.Name + " at " + PlacesDomain.Address.Address1;
                    MediaModel.Url = Response.ResponseUri.ToString();
                    MediaModel.ExternalMediaId = result.Photos[i].PhotoReference;
                    int mediaId = _mediaService.InsertTest(MediaModel);
                }
            }
        }
    }
}