using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sabio.Web.Models.Responses
{
    public class FacebookResponse
    {
            public string access_token { get; set; }

            public string token_type { get; set; }

            public string expires_in { get; set; }
        
    }
}