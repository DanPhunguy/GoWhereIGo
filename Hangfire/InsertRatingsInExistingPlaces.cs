using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Sabio.Web.Domain;
using Sabio.Web.Enums;
using Sabio.Web.Models.Requests.Pagination;
using Sabio.Web.Models.Requests.Rating;
using Sabio.Web.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YelpSharp.Data;
using static Sabio.Web.Services.GooglePlacesService;

namespace Sabio.Web.Background.Tasks
{
    public class InsertRatingsInExistingPlaces
    {
        private readonly IRatingService _ratingService;
        private readonly IYelpService _yelpService;
        private readonly IPlacesService _placesSerivce;
        private readonly IGooglePlacesService _googleService;
        private readonly IMediaService _mediaService;

        public InsertRatingsInExistingPlaces(
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
        public void Run(PerformContext context, IJobCancellationToken cancellationToken)
        {
            PaginatedRequest model = new PaginatedRequest();
            int i = 1;
            int totalCount = 0;

            do
            {
                model.CurrentPage = i;
                model.ItemsPerPage = 10;
                List<Places> placesDomain = _placesSerivce.GetList(model, ref totalCount);
                if (placesDomain == null || !placesDomain.Any()) {
                    break;
                }
                for (int x = 0; x < placesDomain.Count; x++)
                {
                    List<PlacesRatingDomain> PlacesRatingList = _ratingService.GetPlacesRatingSelectByplaceId(placesDomain[x].Id);
                    if (PlacesRatingList == null || !PlacesRatingList.Any())
                    {
                        context.WriteLine(x);
                        GooglePlaceDetailsResponse googleResult = _googleService.GetDetails(placesDomain[x].ExtPlaceId).Result;
                        var result = googleResult.Result;

                        if (result == null)
                        {
                            context.WriteLine($"Could not find google place for {placesDomain[x].Name}.");

                            continue;
                        }

                        PlacesRatingsRequest googleModel = new PlacesRatingsRequest();

                        googleModel.PlaceId = placesDomain[x].Id;
                        googleModel.RatingType = RatingType.GooglePlaces;
                        googleModel.Rating = Convert.ToDecimal(result.Rating);
                        googleModel.UserId = placesDomain[x].UserId;

                        int placesRatingGoogleId = _ratingService.PostPlacesRatingInsert(googleModel);

                        Places place = _placesSerivce.GetPlaceByExternalPlaceId(placesDomain[x].ExtPlaceId);
                        if (string.IsNullOrEmpty(place.Address.Address1))
                        {
                            continue;
                        }
                        SearchResults yelpResult = _yelpService.Search(placesDomain[x].Name, place.Address.Address1, 1, 1).Result;
                        if (yelpResult.businesses == null || !yelpResult.businesses.Any())
                        {
                            continue;
                        }
                        PlacesRatingsRequest yelpModel = new PlacesRatingsRequest();

                        yelpModel.PlaceId = placesDomain[x].Id;
                        yelpModel.RatingType = RatingType.Yelp;
                        yelpModel.Rating = Convert.ToDecimal(yelpResult.businesses[0].rating);
                        yelpModel.UserId = placesDomain[x].UserId;

                        int placesRatingId = _ratingService.PostPlacesRatingInsert(yelpModel);
                    }
                    
                }

                i++;


            } while (true);

        }
    }
}