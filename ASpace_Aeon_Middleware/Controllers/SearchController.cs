using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using ArchivesSpace_.Net_Client;
using ArchivesSpace_.Net_Client.Models;
using ASpace_Aeon_Middleware.Interfaces;
using NLog;

namespace ASpace_Aeon_Middleware.Controllers
{
    public class SearchController : ApiController
    {
        private readonly IAspaceSiteServiceHandler _serviceHandler;
        private static Logger _logger;

        public SearchController(IAspaceSiteServiceHandler serviceHandler)
        {
            _serviceHandler = serviceHandler;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [Route("search")]
        [Route("qsearch_atkcache_holdings")]
        [Route("qsearch_atkcache_holdings.ashx")]
        [HttpGet]
        public async Task<HttpResponseMessage> Get(string repo = "", string type = "", string q = "")
        {
            //Type is maintained for signature consistency but is not used
            var reqId = Guid.NewGuid().ToString("D");
            var sw = new Stopwatch();
            sw.Start();
            _logger.Trace("Request for URI [ {0} ] received and assigned ID [ {1} ]", Request.RequestUri, reqId);

            if (repo == "")
            {
                repo = ASpace_Aeon_Middleware.Configuration.DefaultSite;
            }
            var activeService = await _serviceHandler.GetArchivesSpaceServiceAsync(repo);
            var searchEngine = new ArchivesSpaceSearch(activeService);
            var searchOptions = new SearchOptions
            {
                Query = q //note this is not scrubbed for stopwords since the SOLR index will handle that
            };
            if (String.IsNullOrWhiteSpace(q))
            {
                _logger.Trace("Request [ {0} ] finished in [ {1} ] ms", reqId, sw.ElapsedMilliseconds);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            var result = await searchEngine.ResourceSearchAsync(searchOptions, false); //since this is a quick search we only want page one of search results
            if (result.TotalHits < 0)
            {
                _logger.Trace("Request [ {0} ] finished in [ {1} ] ms", reqId, sw.ElapsedMilliseconds);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            var formattedResult = ResultsToPipeSeparatedFormat(result.Results, q);
            _logger.Trace("Request [ {0} ] finished in [ {1} ] ms", reqId, sw.ElapsedMilliseconds);
            return Request.CreateResponse(HttpStatusCode.OK, formattedResult, "text/plain");
        }

        private string ResultsToPipeSeparatedFormat(ICollection<SearchResultEntryResource> resultList, string queryText)
        {
            var resultStringBuilder = new StringBuilder();
            foreach (var resource in resultList)
            {
                var callNumber = Helpers.FormatCallNumber(resource.Identifier);
                var formattedCallNumber = Helpers.AddEmphasis(callNumber, queryText);
                var location = ""; //matches behavior of original service
                var title = Helpers.StripPipes(resource.Title); //since pipe separated could be a problem, only a few places where they may appear
                var formattedTitle = Helpers.AddEmphasis(title, queryText);
                var author = ""; //matches original
                var formattedAuthor = ""; //''
                var published = ""; //''
                var searchString = Helpers.StripPipes(queryText);
                var resourceId = resource.Id;
                var eadId = Helpers.StripPipes(resource.EadId);
                var result = String.Join("|", callNumber, formattedCallNumber, location, title, formattedTitle, author,
                    formattedAuthor, published, searchString, resourceId, eadId, callNumber);
                resultStringBuilder.AppendLine(result);
            }
            return resultStringBuilder.ToString();
        }
    }
}
