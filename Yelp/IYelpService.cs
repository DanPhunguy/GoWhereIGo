using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using YelpSharp.Data;

namespace Sabio.Web.Services.Interface
{
    public interface IYelpService
    {
        Task<SearchResults> Search(string term, int limit, double latitude, double longitude);

        Task<SearchResults> Search(string term, int limit, int offset, double latitude, double longitude);

        Task<SearchResults> Search(string term, string address, int limit, int offset);
    }
}