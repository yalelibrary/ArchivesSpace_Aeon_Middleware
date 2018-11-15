using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ArchivesSpace_.Net_Client;
using ArchivesSpace_.Net_Client.Models;
using ASpace_Aeon_Middleware.Interfaces;

namespace ASpace_Aeon_Middleware
{
    public class AspaceHelpers
    {
        private readonly ArchivesSpaceService _siteServiceHandler;

        public AspaceHelpers(ArchivesSpaceService siteServiceHandler)
        {
            _siteServiceHandler = siteServiceHandler;
        }

        public async Task<int> GetResourceIdFromCallNumber(string callNumber)
        {
            var searchParams = new SearchOptions { Query = callNumber };
            var searchEngine = new ArchivesSpaceSearch(_siteServiceHandler);
            var searchResults = await searchEngine.ResourceSearchAsync(searchParams);
            if (searchResults.TotalHits < 1)
            {
                return 0;
            }
            return searchResults.Results.First().IdStripped;
        }

    }
}