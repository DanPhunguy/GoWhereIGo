using Sabio.Web.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using YelpSharp;
using YelpSharp.Data;
using YelpSharp.Data.Options;

namespace Sabio.Web.Services
{
    public class YelpService : IYelpService
    {
        private readonly Yelp _yelp;

        public YelpService()
        {
            var opts = new Options();
            opts.AccessToken = ConfigService.yelpApiTokenKey;
            opts.AccessTokenSecret = ConfigService.yelpApiTokenSecretKey;
            opts.ConsumerKey = ConfigService.yelpApiCustomerKey;
            opts.ConsumerSecret = ConfigService.yelpApiSecretCustomerKey;

            _yelp = new Yelp(opts);
        }

        public Task<SearchResults> Search(string term, int limit, double latitude, double longitude)
        {
            SearchOptions searchOptions = new SearchOptions();

            CoordinateOptions coordinates = new CoordinateOptions();
            coordinates.latitude = latitude;
            coordinates.longitude = longitude;

            GeneralOptions general = new GeneralOptions();
            general.limit = limit;
            general.term = term;
            searchOptions.GeneralOptions = general;
            searchOptions.LocationOptions = coordinates;

            return _yelp.Search(searchOptions);
        }

        public Task<SearchResults> Search(string term, int limit, int offset, double latitude, double longitude)
        {
            SearchOptions searchOptions = new SearchOptions();

            CoordinateOptions coordinates = new CoordinateOptions();
            coordinates.latitude = latitude;
            coordinates.longitude = longitude;

            GeneralOptions general = new GeneralOptions();
            general.term = term;
            general.sort = 2; // highest rated
            general.limit = limit;
            general.offset = offset;
            general.radius_filter = 2000;
            searchOptions.GeneralOptions = general;
            searchOptions.LocationOptions = coordinates;

            return _yelp.Search(searchOptions);
        }
        public Task<SearchResults> Search(string term, string address, int limit, int offset)
        {
            SearchOptions searchOptions = new SearchOptions();

            LocationOptions location = new LocationOptions();
            location.location = address;

            GeneralOptions general = new GeneralOptions();
            general.sort = 2;
            general.limit = limit;
            general.offset = offset;
            searchOptions.GeneralOptions = general;
            searchOptions.LocationOptions = location;

            return _yelp.Search(searchOptions);
        }


    }
}