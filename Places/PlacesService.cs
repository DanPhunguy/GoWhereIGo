using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sabio.Data.Providers;
using Sabio.Web.Domain;
using Sabio.Web.Models.Requests.Places;
using System.Data.SqlClient;
using System.Data;
using Sabio.Data;
using Sabio.Web.Models.Requests.Tags;
using Sabio.Web.Models.Requests.Addresses;
using Sabio.Web.Enums;
using Sabio.Web.Models.Requests.MyMedia;
using Sabio.Web.Domain.MyMedia;
using Sabio.Web.Services.MyMedia_Services;
using Sabio.Web.Services.Interface;
using Microsoft.Practices.Unity;
using Sabio.Web.Models.Requests.Rating;
using Sabio.Web.Models.Requests.Pagination;

namespace Sabio.Web.Services
{
    public class PlacesService : BaseService, IPlacesService
    {
        [Dependency]
        public IAddressesService _AddressesService { get; set; }

        [Dependency]
        public IMediaService _MediaService { get; set; }


        //Post
        public Places Insert(PlacesAdd model)
        {
            InsertAddressModel InsertAddress = new InsertAddressModel();

            InsertAddress.UserId = model.UserId;
            InsertAddress.Address1 = model.Address1;
            InsertAddress.City = model.City;
            InsertAddress.State = model.State;
            InsertAddress.ZipCode = model.ZipCode;
            InsertAddress.Latitude = model.Latitude;
            InsertAddress.Longitude = model.Longitude;

            Addresses newModelAddress = _AddressesService.InsertAddress(InsertAddress);


            MediaAddRequest InsertMedia = new MediaAddRequest();


            InsertMedia.UserId = model.UserId;
            InsertMedia.MediaType = MediaType.GooglePlaces;
            InsertMedia.Url = model.Url;
            InsertMedia.DataType = "image/jpeg";

            int MediaId = _MediaService.InsertTest(InsertMedia);

            int id = 0;

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Places_Insert"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Name", model.Name);
                   paramCollection.AddWithValue("@Description", model.Description);
                   paramCollection.AddWithValue("@UserId", model.UserId);
                   paramCollection.AddWithValue("@OperatingHours", model.OperatingHours);
                   paramCollection.AddWithValue("@PhoneNumber", model.PhoneNumber);
                   paramCollection.AddWithValue("@Website", model.Website);
                   paramCollection.AddWithValue("@GlobalRating", model.GlobalRating);
                   paramCollection.AddWithValue("@MediaId", MediaId);
                   paramCollection.AddWithValue("@LocationId", newModelAddress.Id);
                   paramCollection.AddWithValue("@CategoryId", model.CategoryId);
                   paramCollection.AddWithValue("@Slug", model.Slug);
                   paramCollection.AddWithValue("@ExtPlaceId", model.ExtPlaceId);
                   paramCollection.AddWithValue("@CityId", model.CityId);
                   paramCollection.AddWithValue("@Price", model.Price);

                   SqlParameter p = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                   p.Direction = System.Data.ParameterDirection.Output;

                   paramCollection.Add(p);

               }, returnParameters: delegate (SqlParameterCollection param)
               {
                   int.TryParse(param["@Id"].Value.ToString(), out id);

               });

            if (id > 0)
            {

                TagsPlacesRequest x = new TagsPlacesRequest();
                x.TagIds = model.TagIds;
                x.PlaceId = id;

                InsertRelationship(x);

                return GetPlace(id);
            }

            else

            {
                return null;

            }
        }

        //Private Static Method That is used to call Get By Id & Get By Slug
        public Places mapPlaceAddress(IDataReader reader, short set, Places p)
        {
            Addresses a = null;
            Media m = null;
            CityPage c = null;

            int startingIndex = 0; //startingOrdinal

            if (set == 0)
            {
                p.Id = reader.GetSafeInt32(startingIndex++);
                p.Created = reader.GetSafeUtcDateTime(startingIndex++);
                p.Name = reader.GetSafeString(startingIndex++);
                p.Description = reader.GetSafeString(startingIndex++);
                p.UserId = reader.GetSafeString(startingIndex++);
                p.OperatingHours = reader.GetSafeString(startingIndex++);
                p.PhoneNumber = reader.GetSafeString(startingIndex++);
                p.Website = reader.GetSafeString(startingIndex++);
                p.GlobalRating = reader.GetSafeInt32(startingIndex++);
                p.MediaId = reader.GetSafeInt32(startingIndex++);
                p.LocationId = reader.GetSafeInt32(startingIndex++);
                p.CategoryId = reader.GetSafeInt32(startingIndex++);
                p.Slug = reader.GetSafeString(startingIndex++);
                p.ExtPlaceId = reader.GetSafeString(startingIndex++);
                p.CityId = reader.GetSafeInt32(startingIndex++);
                p.Price = reader.GetSafeInt32(startingIndex++);
                p.Tags = new List<PlacesTagsDomain>();

                a = new Addresses();

                a.Address1 = reader.GetSafeString(startingIndex++);
                a.City = reader.GetSafeString(startingIndex++);
                a.State = reader.GetSafeString(startingIndex++);
                a.ZipCode = reader.GetSafeString(startingIndex++);
                a.Latitude = reader.GetSafeDecimal(startingIndex++);
                a.Longitude = reader.GetSafeDecimal(startingIndex++);

                p.Address = a;

                m = new Media();

                m.MediaType = reader.GetSafeEnum<MediaType>(startingIndex++);
                m.DataType = reader.GetSafeString(startingIndex++);
                m.Url = reader.GetSafeString(startingIndex++);

                p.Media = m;

                c = new CityPage();

                c.Id = reader.GetSafeInt32(startingIndex++);
                c.Created = reader.GetSafeDateTime(startingIndex++);
                c.Name = reader.GetSafeString(startingIndex++);
                c.Description = reader.GetSafeString(startingIndex++);
                c.MediaId = reader.GetSafeInt32(startingIndex++);
                c.Latitude = reader.GetSafeDecimal(startingIndex++);
                c.Longitude = reader.GetSafeDecimal(startingIndex++);
                c.Address = reader.GetSafeString(startingIndex++);
                c.Radius = reader.GetSafeInt32(startingIndex++);
                c.Slug = reader.GetSafeString(startingIndex++);


                p.City = c;

            }
            else if (set == 1)
            {
                PlacesTagsDomain Tags = new PlacesTagsDomain();

                Tags.Id = reader.GetSafeInt32(startingIndex++);
                Tags.ParentTagId = reader.GetSafeInt32(startingIndex++);
                Tags.Created = reader.GetSafeDateTime(startingIndex++);
                Tags.TagName = reader.GetSafeString(startingIndex++);
                Tags.TagSlug = reader.GetSafeString(startingIndex++);
                Tags.AuthorId = reader.GetSafeInt32(startingIndex++);
                Tags.TypeId = reader.GetSafeEnum<TagTypes>(startingIndex++);
                Tags.Img = reader.GetSafeString(startingIndex++);
                Tags.Url = reader.GetSafeString(startingIndex++);

                p.Tags.Add(Tags);

            }
            return p;

        }

        public List<Places> GetPlaces(IEnumerable<int> ids)
        {
            var places = new List<Places>();

            DataProvider.ExecuteCmd(GetConnection, "dbo.Places_SelectByIds", param =>
            {
                var p = new SqlParameter("@Ids", SqlDbType.Structured);
                p.Value = new IntIdTable(ids);

                param.Add(p);
            }, (reader, set) =>
            {
                int startingIndex = 0;
                Places p = new Places
                {
                    Id = reader.GetSafeInt32(startingIndex++),
                    Created = reader.GetSafeUtcDateTime(startingIndex++),
                    Name = reader.GetSafeString(startingIndex++),
                    Description = reader.GetSafeString(startingIndex++),
                    UserId = reader.GetSafeString(startingIndex++),
                    OperatingHours = reader.GetSafeString(startingIndex++),
                    PhoneNumber = reader.GetSafeString(startingIndex++),
                    Website = reader.GetSafeString(startingIndex++),
                    GlobalRating = reader.GetSafeInt32(startingIndex++),
                    MediaId = reader.GetSafeInt32(startingIndex++),
                    LocationId = reader.GetSafeInt32(startingIndex++),
                    CategoryId = reader.GetSafeInt32(startingIndex++),
                    Slug = reader.GetSafeString(startingIndex++),
                    ExtPlaceId = reader.GetSafeString(startingIndex++),
                    CityId = reader.GetSafeInt32(startingIndex++),
                    Price = reader.GetSafeInt32(startingIndex++),

                    Tags = new List<PlacesTagsDomain>()
                };

                Addresses a = new Addresses
                {
                    Address1 = reader.GetSafeString(startingIndex++),
                    City = reader.GetSafeString(startingIndex++),
                    State = reader.GetSafeString(startingIndex++),
                    ZipCode = reader.GetSafeString(startingIndex++),
                    Latitude = reader.GetSafeDecimal(startingIndex++),
                    Longitude = reader.GetSafeDecimal(startingIndex++)
                };

                p.Address = a;

                Media m = new Media
                {
                    MediaType = reader.GetSafeEnum<MediaType>(startingIndex++),
                    DataType = reader.GetSafeString(startingIndex++),
                    Url = reader.GetSafeString(startingIndex++)
                };

                p.Media = m;

                TagsDomain t = new TagsDomain
                {
                    Id = reader.GetSafeInt32(startingIndex++),
                    ParentTagId = reader.GetSafeInt32(startingIndex++),
                    Created = reader.GetSafeDateTime(startingIndex++),
                    TagName = reader.GetSafeString(startingIndex++),
                    TagSlug = reader.GetSafeString(startingIndex++),
                    AuthorId = reader.GetSafeInt32(startingIndex++),
                    TypeId = reader.GetSafeEnum<TagTypes>(startingIndex++),
                    Img = reader.GetSafeString(startingIndex++),
                    Url = reader.GetSafeString(startingIndex++)
                };

                p.Category = t;

                p.City = new CityPage
                {
                    Id = reader.GetSafeInt32(startingIndex++),
                    Created = reader.GetSafeDateTime(startingIndex++),
                    Name = reader.GetSafeString(startingIndex++),
                    Description = reader.GetSafeString(startingIndex++),
                    MediaId = reader.GetSafeInt32(startingIndex++),
                    Latitude = reader.GetSafeDecimal(startingIndex++),
                    Longitude = reader.GetSafeDecimal(startingIndex++),
                    Address = reader.GetSafeString(startingIndex++),
                    Radius = reader.GetSafeInt32(startingIndex++),
                    Slug = reader.GetSafeString(startingIndex++)
                };

                places.Add(p);
            });

            var following = CheckUserFollowingPlaces(UserService.GetCurrentUserId(), places.Select(p => p.Id));

            foreach (var place in places)
            {
                place.isFollower = following.Contains(place.Id);
            }

            return places;
        }

        //Get By ID
        public Places GetPlace(int id)
        {
            Places p = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Places_SelectById"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Id", id);
               },
                map: delegate (IDataReader reader, short set)
                {
                    if (p == null)
                    {
                        p = new Places();
                    }

                    mapPlaceAddress(reader, set, p);
                }

               );

            return (p);
        }

        public Places GetPlaceByExternalPlaceId(string id)
        {
            Places p = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Places_SelectByExtPlaceId", param =>
            {
                param.AddWithValue("@ExtPlaceId", id);
            }, (reader, set) =>
            {
                int startingIndex = 0;

                if (set == 0)
                {
                    p = new Places
                    {
                        Id = reader.GetSafeInt32(startingIndex++),
                        Created = reader.GetSafeUtcDateTime(startingIndex++),
                        Name = reader.GetSafeString(startingIndex++),
                        Description = reader.GetSafeString(startingIndex++),
                        UserId = reader.GetSafeString(startingIndex++),
                        OperatingHours = reader.GetSafeString(startingIndex++),
                        PhoneNumber = reader.GetSafeString(startingIndex++),
                        Website = reader.GetSafeString(startingIndex++),
                        GlobalRating = reader.GetSafeInt32(startingIndex++),
                        MediaId = reader.GetSafeInt32(startingIndex++),
                        LocationId = reader.GetSafeInt32(startingIndex++),
                        CategoryId = reader.GetSafeInt32(startingIndex++),
                        Slug = reader.GetSafeString(startingIndex++),
                        ExtPlaceId = reader.GetSafeString(startingIndex++),
                        CityId = reader.GetSafeInt32(startingIndex++),
                        Price = reader.GetSafeInt32(startingIndex++),

                        Tags = new List<PlacesTagsDomain>()
                    };

                    Addresses a = new Addresses
                    {
                        Address1 = reader.GetSafeString(startingIndex++),
                        City = reader.GetSafeString(startingIndex++),
                        State = reader.GetSafeString(startingIndex++),
                        ZipCode = reader.GetSafeString(startingIndex++),
                        Latitude = reader.GetSafeDecimal(startingIndex++),
                        Longitude = reader.GetSafeDecimal(startingIndex++)
                    };

                    p.Address = a;

                    Media m = new Media
                    {
                        MediaType = reader.GetSafeEnum<MediaType>(startingIndex++),
                        DataType = reader.GetSafeString(startingIndex++),
                        Url = reader.GetSafeString(startingIndex++)
                    };

                    p.Media = m;

                    TagsDomain t = new TagsDomain
                    {
                        Id = reader.GetSafeInt32(startingIndex++),
                        ParentTagId = reader.GetSafeInt32(startingIndex++),
                        Created = reader.GetSafeDateTime(startingIndex++),
                        TagName = reader.GetSafeString(startingIndex++),
                        TagSlug = reader.GetSafeString(startingIndex++),
                        AuthorId = reader.GetSafeInt32(startingIndex++),
                        TypeId = reader.GetSafeEnum<TagTypes>(startingIndex++),
                        Img = reader.GetSafeString(startingIndex++),
                        Url = reader.GetSafeString(startingIndex++)
                    };

                    p.Category = t;

                    p.City = new CityPage

                    {
                        Id = reader.GetSafeInt32(startingIndex++),
                        Created = reader.GetSafeDateTime(startingIndex++),
                        Name = reader.GetSafeString(startingIndex++),
                        Description = reader.GetSafeString(startingIndex++),
                        MediaId = reader.GetSafeInt32(startingIndex++),
                        Latitude = reader.GetSafeDecimal(startingIndex++),
                        Longitude = reader.GetSafeDecimal(startingIndex++),
                        Address = reader.GetSafeString(startingIndex++),
                        Radius = reader.GetSafeInt32(startingIndex++),
                        Slug = reader.GetSafeString(startingIndex++)

                    };

                }
                else if (set == 1)
                {
                    PlacesTagsDomain Tags = new PlacesTagsDomain();

                    Tags.Id = reader.GetSafeInt32(startingIndex++);
                    Tags.ParentTagId = reader.GetSafeInt32(startingIndex++);
                    Tags.Created = reader.GetSafeDateTime(startingIndex++);
                    Tags.TagName = reader.GetSafeString(startingIndex++);
                    Tags.TagSlug = reader.GetSafeString(startingIndex++);
                    Tags.AuthorId = reader.GetSafeInt32(startingIndex++);
                    Tags.TypeId = reader.GetSafeEnum<TagTypes>(startingIndex++);
                    Tags.Img = reader.GetSafeString(startingIndex++);
                    Tags.Url = reader.GetSafeString(startingIndex++);

                    p.Tags.Add(Tags);
                }
            });

            return p;
        }

        //Get By Slug
        public Places GetPlaceSlug(string Slug)
        {
            Places p = null;


            DataProvider.ExecuteCmd(GetConnection, "dbo.Places_SelectBySlug"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Slug", Slug);
               },
                map: delegate (IDataReader reader, short set)
                {
                    Addresses a = null;
                    Media m = null;
                    TagsDomain t = null;
                    CityPage c = null;                  
                    int startingIndex = 0; //startingOrdinal

                    if (set == 0)
                    {
                        p = new Places();

                        p.Id = reader.GetSafeInt32(startingIndex++);
                        p.Created = reader.GetSafeUtcDateTime(startingIndex++);
                        p.Name = reader.GetSafeString(startingIndex++);
                        p.Description = reader.GetSafeString(startingIndex++);
                        p.UserId = reader.GetSafeString(startingIndex++);
                        p.OperatingHours = reader.GetSafeString(startingIndex++);
                        p.PhoneNumber = reader.GetSafeString(startingIndex++);
                        p.Website = reader.GetSafeString(startingIndex++);
                        p.GlobalRating = reader.GetSafeInt32(startingIndex++);
                        p.MediaId = reader.GetSafeInt32(startingIndex++);
                        p.LocationId = reader.GetSafeInt32(startingIndex++);
                        p.CategoryId = reader.GetSafeInt32(startingIndex++);
                        p.Slug = reader.GetSafeString(startingIndex++);
                        p.ExtPlaceId = reader.GetSafeString(startingIndex++);
                        p.CityId = reader.GetSafeInt32(startingIndex++);
                        p.Price = reader.GetSafeInt32(startingIndex++);

                        p.Tags = new List<PlacesTagsDomain>();

                        a = new Addresses();

                        a.Address1 = reader.GetSafeString(startingIndex++);
                        a.City = reader.GetSafeString(startingIndex++);
                        a.State = reader.GetSafeString(startingIndex++);
                        a.ZipCode = reader.GetSafeString(startingIndex++);
                        a.Latitude = reader.GetSafeDecimal(startingIndex++);
                        a.Longitude = reader.GetSafeDecimal(startingIndex++);

                        p.Address = a;

                        m = new Media();

                        m.MediaType = reader.GetSafeEnum<MediaType>(startingIndex++);
                        m.DataType = reader.GetSafeString(startingIndex++);
                        m.Url = reader.GetSafeString(startingIndex++);

                        p.Media = m;

                        t = new TagsDomain();

                        t.Id = reader.GetSafeInt32(startingIndex++);
                        t.ParentTagId = reader.GetSafeInt32(startingIndex++);
                        t.Created = reader.GetSafeDateTime(startingIndex++);
                        t.TagName = reader.GetSafeString(startingIndex++);
                        t.TagSlug = reader.GetSafeString(startingIndex++);
                        t.AuthorId = reader.GetSafeInt32(startingIndex++);
                        t.TypeId = reader.GetSafeEnum<TagTypes>(startingIndex++);
                        t.Img = reader.GetSafeString(startingIndex++);

                        p.Category = t;

                        c = new CityPage();

                        c.Id = reader.GetSafeInt32(startingIndex++);
                        c.Created = reader.GetSafeDateTime(startingIndex++);
                        c.Name = reader.GetSafeString(startingIndex++);
                        c.Description = reader.GetSafeString(startingIndex++);
                        c.MediaId = reader.GetSafeInt32(startingIndex++);
                        c.Latitude = reader.GetSafeDecimal(startingIndex++);
                        c.Longitude = reader.GetSafeDecimal(startingIndex++);
                        c.Address = reader.GetSafeString(startingIndex++);
                        c.Radius = reader.GetSafeInt32(startingIndex++);
                        c.Slug = reader.GetSafeString(startingIndex++);


                        p.City = c;

                    }
                    else if (set == 1)
                    {
                        PlacesTagsDomain Tags = new PlacesTagsDomain();

                        Tags.Id = reader.GetSafeInt32(startingIndex++);
                        Tags.ParentTagId = reader.GetSafeInt32(startingIndex++);
                        Tags.Created = reader.GetSafeDateTime(startingIndex++);
                        Tags.TagName = reader.GetSafeString(startingIndex++);
                        Tags.TagSlug = reader.GetSafeString(startingIndex++);
                        Tags.AuthorId = reader.GetSafeInt32(startingIndex++);
                        Tags.TypeId = reader.GetSafeEnum<TagTypes>(startingIndex++);
                        Tags.Img = reader.GetSafeString(startingIndex++);
                        Tags.Url = reader.GetSafeString(startingIndex++);

                        p.Tags.Add(Tags);

                    }
                    else if (set == 2)
                    {
                        if (p.PlaceRating == null)
                        {
                            p.PlaceRating = new List<PlacesRatingDomain>();
                        }

                        PlacesRatingDomain pR = new PlacesRatingDomain();

                        pR.Rating = reader.GetSafeDecimal(startingIndex++);
                        pR.RatingType = reader.GetSafeEnum<RatingType>(startingIndex++);

                        p.PlaceRating.Add(pR);
                    }
                }
               );

            if (UserService.IsLoggedIn())
            {
                string currentUserId = UserService.GetCurrentUserId();

                //p.isFollower = FollowingPlacesService.FollowingPlacesGetUserByIdAndPlacesId(p.Id, userId);

                p.isFollower = DecorateisFollowing(p.Id, currentUserId);

            }
            else
            {
                p.isFollower = false;
            }

            return p;
        }

        //Get By List
        public List<Places> GetList(PaginatedRequest model, ref int TotalCount)
        {
            List<Places> list = null;

            int totalCount = 0;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Places_SearchAll"
                , inputParamMapper: delegate (SqlParameterCollection ParamCollection)

                {
                    ParamCollection.AddWithValue("@currentPage", model.CurrentPage);
                    ParamCollection.AddWithValue("@itemsPerPage", model.ItemsPerPage);
                    ParamCollection.AddWithValue("@searchQuery", model.SearchQuery);
                    ParamCollection.AddWithValue("@sortColumn", model.SortColumn);
                    ParamCollection.AddWithValue("@sortOrder", model.SortOrder);
                }

                , map: delegate (IDataReader reader, short set)
                {
                    Places p = new Places();

                    int startingIndex = 0;
                    totalCount = reader.GetSafeInt32(startingIndex++);
                    long ROWNUM = reader.GetSafeInt64(startingIndex++);
                    p.Id = reader.GetSafeInt32(startingIndex++);
                    p.Created = reader.GetSafeUtcDateTime(startingIndex++);
                    p.Name = reader.GetSafeString(startingIndex++);
                    p.Description = reader.GetSafeString(startingIndex++);
                    p.UserId = reader.GetSafeString(startingIndex++);
                    p.OperatingHours = reader.GetSafeString(startingIndex++);
                    p.PhoneNumber = reader.GetSafeString(startingIndex++);
                    p.Website = reader.GetSafeString(startingIndex++);
                    p.GlobalRating = reader.GetSafeInt32(startingIndex++);
                    p.MediaId = reader.GetSafeInt32(startingIndex++);
                    p.LocationId = reader.GetSafeInt32(startingIndex++);
                    p.CategoryId = reader.GetSafeInt32(startingIndex++);
                    p.Slug = reader.GetSafeString(startingIndex++);
                    p.ExtPlaceId = reader.GetSafeString(startingIndex++);
                    p.CityId = reader.GetSafeInt32(startingIndex++);
                    p.Price = reader.GetSafeInt32(startingIndex++);

                    if (list == null)
                    {
                        list = new List<Places>();
                    }

                    list.Add(p);
                }

               );

            TotalCount = totalCount;

            return list;
        }

        // Update By ID 
        public void Update(PlacesUpdate model, int id)
        {

            UpdateAddressModel UpdateAddress = new UpdateAddressModel();


            UpdateAddress.Address1 = model.Address1;
            UpdateAddress.City = model.City;
            UpdateAddress.State = model.State;
            UpdateAddress.ZipCode = model.ZipCode;
            UpdateAddress.Latitude = model.Latitude;
            UpdateAddress.Longitude = model.Longitude;

            _AddressesService.UpdateAddress(UpdateAddress, model.LocationId);

            MediaUpdateRequest UpdateMedia = new MediaUpdateRequest();

            UpdateMedia.UserId = model.UserId;
            UpdateMedia.MediaType = MediaType.GooglePlaces;
            UpdateMedia.Url = model.Url;
            UpdateMedia.DataType = "image/jpeg";

            _MediaService.Update(UpdateMedia, model.MediaId);

            int updateid = id;


            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Places_UpdateById"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Id", updateid);
                   paramCollection.AddWithValue("@Name", model.Name);
                   paramCollection.AddWithValue("@Description", model.Description);
                   paramCollection.AddWithValue("@OperatingHours", model.OperatingHours);
                   paramCollection.AddWithValue("@PhoneNumber", model.PhoneNumber);
                   paramCollection.AddWithValue("@Website", model.Website);
                   paramCollection.AddWithValue("@GlobalRating", model.GlobalRating);
                   //paramCollection.AddWithValue("@MediaId", model.MediaId);
                   paramCollection.AddWithValue("@CategoryId", model.CategoryId);
                   paramCollection.AddWithValue("@Slug", model.Slug);
                   paramCollection.AddWithValue("@ExtPlaceID", model.ExtPlaceId);
               }

               );

            TagsPlacesRequest TagsUpdate = new TagsPlacesRequest();

            TagsUpdate.PlaceId = updateid;

            TagsUpdate.TagIds = model.TagIds;

            InsertRelationship(TagsUpdate);

        }

        //Delete By ID
        public void Delete(int id)
        {
            int deleteid = id;

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Places_DeleteById"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Id", deleteid);

               }

               );
        }

        public void InsertRelationship(TagsPlacesRequest model)
        {

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.TagPlaces_Relationship_Insert"
              , inputParamMapper: delegate (SqlParameterCollection paramCollection)
              {
                  paramCollection.AddWithValue("@placesId", model.PlaceId);

                  SqlParameter s = new SqlParameter("@TagsId", SqlDbType.Structured);
                  if (model.TagIds != null && model.TagIds.Any())
                  {
                      s.Value = new IntIdTable(model.TagIds);
                  }
                  paramCollection.Add(s);
              });

        }

        public void InsertMediaAndPlacesById(MediaPlacesRequest model)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.MediaPlaces_Relationship_Insert"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@placesId", model.placesId);

                    SqlParameter s = new SqlParameter("@MediaId", SqlDbType.Structured);
                    if (model.MediaId != null && model.MediaId.Any())
                    {
                        s.Value = new IntIdTable(model.MediaId);
                    }
                    paramCollection.Add(s);
                }
                );
        }

        public PaginatedDomain<MediaPlacesDomain> GetMediaByPlacesId(int placesId, PaginatedRequest model)
        {
            PaginatedDomain<MediaPlacesDomain> MediaPlaces = new PaginatedDomain<MediaPlacesDomain>();
            DataProvider.ExecuteCmd(GetConnection, "dbo.MediaPlaces_SelectByplacesId_V2"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                 {
                     paramCollection.AddWithValue("@placesId", placesId);
                     paramCollection.AddWithValue("@currentPage", model.CurrentPage);
                     paramCollection.AddWithValue("@itemsPerPage", model.ItemsPerPage);
                 }, map: delegate (IDataReader reader, short set)
                 {
                     if (set == 0)
                     {
                         int startingIndex = 0;

                         MediaPlacesDomain p = new MediaPlacesDomain();

                         p.MediaId = reader.GetSafeInt32(startingIndex++);
                         p.placesId = reader.GetSafeInt32(startingIndex++);
                         p.Url = reader.GetSafeString(startingIndex++);
                         p.userId = reader.GetSafeString(startingIndex++);
                         p.ThumbnailUrl = reader.GetSafeString(startingIndex++);
                         p.MediaType = reader.GetSafeEnum<MediaType>(startingIndex++);
                         p.reviewPointScore = reader.GetSafeInt32(startingIndex++);
                         p.Created = reader.GetSafeDateTime(startingIndex++);

                         if (MediaPlaces.Items == null)
                         {
                             MediaPlaces.Items = new List<MediaPlacesDomain>();
                         }

                         MediaPlaces.Items.Add(p);
                     }
                     if (set == 1)
                     {
                         MediaPlaces.TotalItems = reader.GetSafeInt32(0);
                     }
                 }

                );
            return Decorate(MediaPlaces);
        }

        private PaginatedDomain<MediaPlacesDomain> Decorate(PaginatedDomain<MediaPlacesDomain> ReviewsByPlaceId)
        {
            if (UserService.IsLoggedIn() && ReviewsByPlaceId != null)
            {
                List<ReviewsByPlaces> ReviewsVotedList = new List<ReviewsByPlaces>();

                List<int> ReviewIdList = new List<int>();

                foreach (MediaPlacesDomain Review in ReviewsByPlaceId.Items)
                {
                    ReviewIdList.Add(Review.MediaId);
                }

                string userId = UserService.GetCurrentUserId();

                DataProvider.ExecuteCmd(GetConnection, "dbo.PointScore_GetByUserIdAndContentId"
                    , inputParamMapper: delegate (SqlParameterCollection param)
                    {
                        param.AddWithValue("@VoterId", userId);

                        SqlParameter s = new SqlParameter("@ContentId", SqlDbType.Structured);

                        if (ReviewIdList != null && ReviewIdList.Any())
                        {
                            s.Value = new IntIdTable(ReviewIdList);
                        }
                        param.Add(s);
                    }, map: delegate (IDataReader reader, short set)
                    {
                        ReviewsByPlaces lol = new ReviewsByPlaces();
                        int startingIndex = 0;
                        lol.NetVote = reader.GetSafeInt32(startingIndex++);
                        lol.ContentId = reader.GetSafeInt32(startingIndex++);
                        lol.ReviewId = reader.GetSafeInt32(startingIndex++);
                        lol.VoterId = reader.GetSafeString(startingIndex++);
                        lol.placesId = reader.GetSafeInt32(startingIndex++);
                        ReviewsVotedList.Add(lol);
                    });
                if (ReviewsVotedList != null)
                {
                    foreach (MediaPlacesDomain Review in ReviewsByPlaceId.Items)
                    {
                        foreach (ReviewsByPlaces ReviewByPlace in ReviewsVotedList)
                        {
                            if (ReviewByPlace.ContentId == Review.MediaId)
                            {
                                Review.hasVoted = true;
                                Review.NetVote = ReviewByPlace.NetVote;
                            }
                        }
                    }
                }
                return ReviewsByPlaceId;
            }
            else
            {
                return ReviewsByPlaceId;
            }
        }

        public MediaPlacesDomain getHighestMediaPlaesId(int placesId)
        {
            MediaPlacesDomain MediaPlaces = null;
            DataProvider.ExecuteCmd(GetConnection, "dbo.MediaPlaces_SelectHighestMedia"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@placesId", placesId);
                }, map: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;

                    MediaPlaces = new MediaPlacesDomain();

                    MediaPlaces.MediaId = reader.GetSafeInt32(startingIndex++);
                    MediaPlaces.placesId = reader.GetSafeInt32(startingIndex++);
                    MediaPlaces.Url = reader.GetSafeString(startingIndex++);
                    MediaPlaces.userId = reader.GetSafeString(startingIndex++);
                    MediaPlaces.ThumbnailUrl = reader.GetSafeString(startingIndex++);
                    MediaPlaces.MediaType = reader.GetSafeEnum<MediaType>(startingIndex++);
                    MediaPlaces.reviewPointScore = reader.GetSafeInt32(startingIndex++);
                    MediaPlaces.Created = reader.GetSafeDateTime(startingIndex++);

                });
            return MediaPlaces;
        }
        //Get By UserID
        public List<Places> GetUserIdPlace(string userId)
        {
            // DECLARE: p && a Objects
            Places p = null;
            Addresses a = null;
            CityPage c = null;

            // DECLARE: pList && pTagsList(?)
            List<Places> pList = null;
            List<PlacesTagsDomain> pTagsList = null;

            // CONNECTS: Stored Proc -dbo.Places_SelectByUserId- to Service Methods
            DataProvider.ExecuteCmd(GetConnection, "dbo.Places_SelectByUserId"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@UserId", userId);
               },
                map: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0; //startingOrdinal

                    if (set == 0)
                    {
                        // DECLARE: Places Object
                        p = new Places();

                        p.Id = reader.GetSafeInt32(startingIndex++);
                        p.Created = reader.GetSafeUtcDateTime(startingIndex++);
                        p.Name = reader.GetSafeString(startingIndex++);
                        p.Description = reader.GetSafeString(startingIndex++);
                        p.UserId = reader.GetSafeString(startingIndex++);
                        p.OperatingHours = reader.GetSafeString(startingIndex++);
                        p.PhoneNumber = reader.GetSafeString(startingIndex++);
                        p.Website = reader.GetSafeString(startingIndex++);
                        p.GlobalRating = reader.GetSafeInt32(startingIndex++);
                        p.MediaId = reader.GetSafeInt32(startingIndex++);
                        p.LocationId = reader.GetSafeInt32(startingIndex++);
                        p.CategoryId = reader.GetSafeInt32(startingIndex++);
                        p.Slug = reader.GetSafeString(startingIndex++);
                        p.ExtPlaceId = reader.GetSafeString(startingIndex++);
                        p.CityId = reader.GetSafeInt32(startingIndex++);
                        p.Price = reader.GetSafeInt32(startingIndex++);

                        // DECLARE: Address Object
                        a = new Addresses();

                        a.Address1 = reader.GetSafeString(startingIndex++);
                        a.City = reader.GetSafeString(startingIndex++);
                        a.State = reader.GetSafeString(startingIndex++);
                        a.ZipCode = reader.GetSafeString(startingIndex++);
                        a.Latitude = reader.GetSafeDecimal(startingIndex++);
                        a.Longitude = reader.GetSafeDecimal(startingIndex++);

                        p.Address = a;

                        c = new CityPage();

                        c.Id = reader.GetSafeInt32(startingIndex++);
                        c.Created = reader.GetSafeDateTime(startingIndex++);
                        c.Name = reader.GetSafeString(startingIndex++);
                        c.Description = reader.GetSafeString(startingIndex++);
                        c.MediaId = reader.GetSafeInt32(startingIndex++);
                        c.Latitude = reader.GetSafeDecimal(startingIndex++);
                        c.Longitude = reader.GetSafeDecimal(startingIndex++);
                        c.Address = reader.GetSafeString(startingIndex++);
                        c.Radius = reader.GetSafeInt32(startingIndex++);
                        c.Slug = reader.GetSafeString(startingIndex++);


                        p.City = c;


                        p.Tags = new List<PlacesTagsDomain>();

                        // CONDITIONAL: Create List<Places>
                        if (pList == null)
                        {
                            pList = new List<Places>();
                        }

                        // ADD: p object to pList
                        pList.Add(p);

                    }
                    else if (set == 1)
                    {
                        if (pTagsList == null)
                        {
                            pTagsList = new List<PlacesTagsDomain>();
                        }

                        PlacesTagsDomain Tags = new PlacesTagsDomain();

                        Tags.Id = reader.GetSafeInt32(startingIndex++);
                        Tags.ParentTagId = reader.GetSafeInt32(startingIndex++);
                        Tags.Created = reader.GetSafeDateTime(startingIndex++);
                        Tags.TagName = reader.GetSafeString(startingIndex++);
                        Tags.TagSlug = reader.GetSafeString(startingIndex++);
                        Tags.AuthorId = reader.GetSafeInt32(startingIndex++);
                        Tags.TypeId = reader.GetSafeEnum<TagTypes>(startingIndex++);
                        Tags.Img = reader.GetSafeString(startingIndex++);
                        Tags.Url = reader.GetSafeString(startingIndex++);
                        Tags.PlacesId = reader.GetSafeInt32(startingIndex++);

                        pTagsList.Add(Tags);
                    }
                }
               );

            if (pList != null)
            {
                foreach (Places places in pList)
                {
                    foreach (PlacesTagsDomain t in pTagsList)
                    {
                        if (places.Id == t.PlacesId)
                        {
                            if (places.Tags == null)
                            {
                                places.Tags = new List<PlacesTagsDomain>();
                            }

                            places.Tags.Add(t);
                        }
                    }
                }
            }

            return (pList);

        }
        //Get By CategoryId 

        public List<Places> GetCategoryId(DiscoverPlace model) //change to GetByCategoryId
        {
            List<Places> list = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Places_discover"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@CategoryId", model.CategoryId);
               }

               , map: delegate (IDataReader reader, short set)
               {
                   Places p = new Places();

                   // look through the service, check for list look for the two lines of code
                   int startingIndex = 0;
                   p.Id = reader.GetSafeInt32(startingIndex++);
                   p.Created = reader.GetSafeUtcDateTime(startingIndex++);
                   p.Name = reader.GetSafeString(startingIndex++);
                   p.Description = reader.GetSafeString(startingIndex++);
                   p.UserId = reader.GetSafeString(startingIndex++);
                   p.OperatingHours = reader.GetSafeString(startingIndex++);
                   p.PhoneNumber = reader.GetSafeString(startingIndex++);
                   p.Website = reader.GetSafeString(startingIndex++);
                   p.GlobalRating = reader.GetSafeInt32(startingIndex++);
                   p.MediaId = reader.GetSafeInt32(startingIndex++);
                   p.LocationId = reader.GetSafeInt32(startingIndex++);
                   p.CategoryId = reader.GetSafeInt32(startingIndex++);
                   p.Slug = reader.GetSafeString(startingIndex++);
                   p.ExtPlaceId = reader.GetSafeString(startingIndex++);
                   p.CityId = reader.GetSafeInt32(startingIndex++);
                   p.Price = reader.GetSafeInt32(startingIndex++);
                   Addresses a = new Addresses();


                   a.Address1 = reader.GetSafeString(startingIndex++);
                   a.City = reader.GetSafeString(startingIndex++);
                   a.State = reader.GetSafeString(startingIndex++);
                   a.ZipCode = reader.GetSafeString(startingIndex++);
                   a.Latitude = reader.GetSafeDecimal(startingIndex++);
                   a.Longitude = reader.GetSafeDecimal(startingIndex++);

                   if (p.LocationId != 0)
                   {
                       p.Address = a;
                   }

                   Media m = new Media();

                   m.MediaType = reader.GetSafeEnum<MediaType>(startingIndex++);
                   m.DataType = reader.GetSafeString(startingIndex++);
                   m.Url = reader.GetSafeString(startingIndex++);

                   if (p.MediaId != 0)
                   {
                       p.Media = m;
                   }

                   CityPage c = new CityPage();

                   c.Id = reader.GetSafeInt32(startingIndex++);
                   c.Created = reader.GetSafeDateTime(startingIndex++);
                   c.Name = reader.GetSafeString(startingIndex++);
                   c.Description = reader.GetSafeString(startingIndex++);
                   c.MediaId = reader.GetSafeInt32(startingIndex++);
                   c.Latitude = reader.GetSafeDecimal(startingIndex++);
                   c.Longitude = reader.GetSafeDecimal(startingIndex++);
                   c.Address = reader.GetSafeString(startingIndex++);
                   c.Radius = reader.GetSafeInt32(startingIndex++);
                   c.Slug = reader.GetSafeString(startingIndex++);


                   TagsDomain Tags = new TagsDomain();

                   Tags.Id = reader.GetSafeInt32(startingIndex++);
                   Tags.ParentTagId = reader.GetSafeInt32(startingIndex++);
                   Tags.Created = reader.GetSafeDateTime(startingIndex++);
                   Tags.TagName = reader.GetSafeString(startingIndex++);
                   Tags.TagSlug = reader.GetSafeString(startingIndex++);
                   Tags.AuthorId = reader.GetSafeInt32(startingIndex++);
                   Tags.TypeId = reader.GetSafeEnum<TagTypes>(startingIndex++);
                   Tags.Img = reader.GetSafeString(startingIndex++);
                   Tags.Url = reader.GetSafeString(startingIndex++);

                   p.Category = Tags;

                   if (list == null)
                   {
                       list = new List<Places>();
                   }

                   list.Add(p);
               }


              );
            return list;
        }

        // Get by UserFavoritePlacesType and UserId
        public List<UserFavoritePlacesTypeDomain> GetByUserIdAndFavoritePlacesType(string userId, int favoriteType) // ,int favoriteType
        {
            List<UserFavoritePlacesTypeDomain> FavoriteTypePlaces = null;
            DataProvider.ExecuteCmd(GetConnection, "dbo.Places_SelectByFavoritePlaceType"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@userId", userId);
                    paramCollection.AddWithValue("@favoriteType", favoriteType);

                }, map: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;

                    UserFavoritePlacesTypeDomain p = new UserFavoritePlacesTypeDomain();

                    p.Id = reader.GetSafeInt32(startingIndex++);
                    p.Created = reader.GetSafeUtcDateTime(startingIndex++);
                    p.Name = reader.GetSafeString(startingIndex++);
                    p.Description = reader.GetSafeString(startingIndex++);
                    p.UserId = reader.GetSafeString(startingIndex++);
                    p.OperatingHours = reader.GetSafeString(startingIndex++);
                    p.PhoneNumber = reader.GetSafeString(startingIndex++);
                    p.Website = reader.GetSafeString(startingIndex++);
                    p.GlobalRating = reader.GetSafeInt32(startingIndex++);
                    p.MediaId = reader.GetSafeInt32(startingIndex++);
                    p.LocationId = reader.GetSafeInt32(startingIndex++);
                    p.CategoryId = reader.GetSafeInt32(startingIndex++);
                    p.Slug = reader.GetSafeString(startingIndex++);
                    p.ExtPlaceId = reader.GetSafeString(startingIndex++);
                    p.CityId = reader.GetSafeInt32(startingIndex++);

                    Media m = new Media();

                    m.MediaType = reader.GetSafeEnum<MediaType>(startingIndex++);
                    m.DataType = reader.GetSafeString(startingIndex++);
                    m.Url = reader.GetSafeString(startingIndex++);

                    if (p.MediaId != 0)
                    {
                        p.Media = m;
                    }

                    Addresses a = new Addresses();

                    a.Address1 = reader.GetSafeString(startingIndex++);
                    a.City = reader.GetSafeString(startingIndex++);
                    a.State = reader.GetSafeString(startingIndex++);
                    a.ZipCode = reader.GetSafeString(startingIndex++);
                    a.Latitude = reader.GetSafeDecimal(startingIndex++);
                    a.Longitude = reader.GetSafeDecimal(startingIndex++);

                    if (p.LocationId != 0)
                    {
                        p.Address = a;
                    }

                    CityPage c = new CityPage();

                    c.Id = reader.GetSafeInt32(startingIndex++);
                    c.Created = reader.GetSafeDateTime(startingIndex++);
                    c.Name = reader.GetSafeString(startingIndex++);
                    c.Description = reader.GetSafeString(startingIndex++);
                    c.MediaId = reader.GetSafeInt32(startingIndex++);
                    c.Latitude = reader.GetSafeDecimal(startingIndex++);
                    c.Longitude = reader.GetSafeDecimal(startingIndex++);
                    c.Address = reader.GetSafeString(startingIndex++);
                    c.Radius = reader.GetSafeInt32(startingIndex++);
                    c.Slug = reader.GetSafeString(startingIndex++);

                    if (p.CityId == c.Id)
                    {
                        p.CityObject = c;
                    }

                    UserFavoritePlacesDomain f = new UserFavoritePlacesDomain();

                    f.Id = reader.GetSafeInt32(startingIndex++);
                    f.Created = reader.GetSafeUtcDateTime(startingIndex++);
                    f.UserId = reader.GetSafeString(startingIndex++);
                    f.PlaceId = reader.GetSafeInt32(startingIndex++);
                    f.FavoriteType = reader.GetSafeEnum<UserFavoritePlacesType>(startingIndex++);
                    f.PointScore = reader.GetSafeInt32(startingIndex++);

                    if (p.Id != 0)
                    {
                        p.FavoriteTypePlace = f;
                    }

                    if (FavoriteTypePlaces == null)
                    {
                        FavoriteTypePlaces = new List<UserFavoritePlacesTypeDomain>();
                    }

                    FavoriteTypePlaces.Add(p);
                }
                );
            return FavoriteTypePlaces;
        }

        public bool DecorateisFollowing(int PlacesId, string UserId)
        {
            bool isFollower = false;

            DataProvider.ExecuteCmd(GetConnection, "dbo.FollowingPlaces_SelectByUserIdAndplacesId"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@PlacesId", PlacesId);
                   paramCollection.AddWithValue("@UserId", UserId);

               }, map: delegate (IDataReader reader, short set)
               {
                   isFollower = true;

               }
       );
            return isFollower;
        }

        //To Support Task Event Managment
        public List<Places> GetList()

        {
            int TotalCount = 0;

            return GetList(new PaginatedRequest()
            {

                CurrentPage = 1,
                ItemsPerPage = 100


            }, ref TotalCount);



        }

        public List<int> CheckUserFollowingPlaces(string userId, IEnumerable<int> placeIds)
        {
            var places = new List<int>();

            DataProvider.ExecuteCmd(GetConnection, "dbo.Places_CheckUserIsFollowing", param =>
            {
                param.AddWithValue("@UserId", userId);

                SqlParameter p = new SqlParameter("@PlaceIds", SqlDbType.Structured);
                p.Value = new IntIdTable(placeIds);

                param.Add(p);
            }, (reader, set) =>
            {
                int startingIndex = 0;

                int placeId = reader.GetSafeInt32(startingIndex++);

                places.Add(placeId);
            });

            return places;
        }
    }
}

