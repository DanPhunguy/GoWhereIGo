using Sabio.Web.Domain;
using Sabio.Web.Models.Requests.MyMedia;
using Sabio.Web.Models.Requests.Pagination;
using Sabio.Web.Models.Requests.Places;
using Sabio.Web.Models.Requests.Tags;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Sabio.Web.Services.Interface
{
    public interface IPlacesService
    {


        //Post
        Places Insert(PlacesAdd model);

        //Private Static Method That is used to call Get By Id & Get By Slug
        Places mapPlaceAddress(IDataReader reader, short set, Places p);

        //Get By ID
        Places GetPlace(int id);

        List<Places> GetPlaces(IEnumerable<int> ids);

        Places GetPlaceByExternalPlaceId(string id);

        //Get By Slug
        Places GetPlaceSlug(string Slug);

        //Get By List
        List<Places> GetList(PaginatedRequest model , ref int TotalCount);

        List<Places> GetList();

        // Update By ID 
        void Update(PlacesUpdate model, int id);

        //Delete By ID
        void Delete(int id);

        PaginatedDomain<MediaPlacesDomain> GetMediaByPlacesId(int placesId, PaginatedRequest model);

        List<UserFavoritePlacesTypeDomain> GetByUserIdAndFavoritePlacesType(string userId, int favoriteType);

        MediaPlacesDomain getHighestMediaPlaesId(int placesId);

        void InsertRelationship(TagsPlacesRequest model);
    
        void InsertMediaAndPlacesById(MediaPlacesRequest model);

        //Get By UserID
        List<Places> GetUserIdPlace(string userId);

        //Get By CategoryId 

        List<Places> GetCategoryId(DiscoverPlace model);

        List<int> CheckUserFollowingPlaces(string userId, IEnumerable<int> placeIds);
    }
}