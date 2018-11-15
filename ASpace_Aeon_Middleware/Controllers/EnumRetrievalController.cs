using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ArchivesSpace_.Net_Client;
using ArchivesSpace_.Net_Client.Models;
using ASpace_Aeon_Middleware.Comparers;
using ASpace_Aeon_Middleware.Interfaces;
using ASpace_Aeon_Middleware.Models;
using NLog;
using WebApi.OutputCache.V2;

namespace ASpace_Aeon_Middleware.Controllers
{
    public class EnumRetrievalController : ApiController
    {
        private readonly IAspaceSiteServiceHandler _serviceHandler;
        private static Logger _logger;

        public EnumRetrievalController(IAspaceSiteServiceHandler serviceHandler)
        {
            _serviceHandler = serviceHandler;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [Route("enum")]
        [Route("get_atkcache_enums")]
        [Route("get_atkcache_enums.ashx")]
        [CacheOutput(ClientTimeSpan = 180, ServerTimeSpan = 180, AnonymousOnly = false)]
        [HttpGet]
        public async Task<HttpResponseMessage> Get(string repo = ""
            , [FromUri(Name = "mfhd_id")]int? mfhdIdNullable = 0
            , [FromUri(Name = "bib_id")]int? bibIdNullable = 0
            , [FromUri(Name = "call_no")]string callNumber = ""
            , [FromUri(Name = "series_id")]int? seriesIdNullable = 0)
        {
            var reqId = Guid.NewGuid().ToString("D");
            var sw = new Stopwatch();
            sw.Start();
            _logger.Trace("Request for URI [ {0} ] received and assigned ID [ {1} ]", Request.RequestUri, reqId);
            var mfhdId = mfhdIdNullable ?? 0; //avoid null type errors if a querystring with ?id=&otherId=3 is sent
            var bibId = bibIdNullable ?? 0;
            var seriesId = seriesIdNullable ?? 0;
            repo = repo == "" ? ASpace_Aeon_Middleware.Configuration.DefaultSite : repo;
            var activeService = await _serviceHandler.GetArchivesSpaceServiceAsync(repo); //call this anyway to make sure it's ready for the next request
            var responseList = new List<EnumResponseItem>();
            if (seriesId == 0 && bibId == 0 && mfhdId == 0 && String.IsNullOrWhiteSpace(callNumber))
            {
                _logger.Trace("Request [ {0} ] finished in [ {1} ] ms", reqId, sw.ElapsedMilliseconds);
                return Request.CreateResponse(HttpStatusCode.OK, responseList);
            }
            if (seriesId != 0)
            {
                await GetContainersForSeriesId(activeService, seriesId, responseList);
            }
            else //if a series is provided the bib
            {
                var resourceId = bibId == 0 ? mfhdId : bibId; //original webservice declared bibId and mfhdId to be the same
                if (resourceId != 0)
                {
                    await GetContainersForResourceId(activeService, resourceId, responseList);
                }
                else if (callNumber != "")
                {
                    //resource ID was empty but a call number was provided
                    var aspaceHelper = new AspaceHelpers(activeService);
                    resourceId = await aspaceHelper.GetResourceIdFromCallNumber(callNumber);
                    if (resourceId == 0)
                    {
                        // no matching resource found; give up
                        _logger.Trace("Request [ {0} ] finished in [ {1} ] ms", reqId, sw.ElapsedMilliseconds);
                        return Request.CreateResponse(HttpStatusCode.OK, responseList);
                    }
                    await GetContainersForResourceId(activeService, resourceId, responseList);
                }
                else
                {
                    //still no hits; give up
                    _logger.Trace("Request [ {0} ] finished in [ {1} ] ms", reqId, sw.ElapsedMilliseconds);
                    return Request.CreateResponse(HttpStatusCode.OK, responseList);
                }
            }
            var sortedList = responseList.OrderBy(x => x.BoxNumber, new NaturalComparer()).ToList(); //serializer doesn't like the IOrderedEnumerable returned by orderby
            _logger.Trace("Request [ {0} ] finished in [ {1} ] ms", reqId, sw.ElapsedMilliseconds);
            return Request.CreateResponse(HttpStatusCode.OK, sortedList); 
        }

        private async Task GetContainersForResourceId(ArchivesSpaceService activeService, int resourceId, List<EnumResponseItem> inputList)
        {
            var resourceManager = new ArchivesSpaceResourceManager(activeService);
            var containerList = await resourceManager.GetAllTopContainersForResourceAsync(resourceId);
            await ProcessContainerList(activeService, containerList, inputList);
        }

        private async Task  GetContainersForSeriesId(ArchivesSpaceService activeService, int seriesId, List<EnumResponseItem> inputList )
        {
            var archivalObjectManager = new ArchivesSpaceArchivalObjectManager(activeService);
            var containerList = await archivalObjectManager.GetAllTopContainersForIdAsync(seriesId);
            await ProcessContainerList(activeService, containerList, inputList);
        }

        private async Task ProcessContainerList(ArchivesSpaceService activeService, ICollection<TopContainer> containerList, List<EnumResponseItem> inputList )
        {
            if (containerList == null || containerList.Count < 1)
            {
                return;
            }
            foreach (var topContainer in containerList)
            {
                var locationTask = GetContainerLocationDisplayString(topContainer, activeService);
                var physDescTask = GetContainerProfileDisplayString(topContainer, activeService);
                await Task.WhenAll(locationTask, physDescTask);
                var responseItem = new EnumResponseItem
                {
                    BoxNumber = Helpers.FormatBoxText(topContainer.Indicator),
                    CallNumber = Helpers.GetCallNumberFromTopContainer(topContainer),
                    ItemBarcode = topContainer.Barcode,
                    Location = locationTask.Result,
                    PhysicalDescription = physDescTask.Result,
                    Restricted = topContainer.Restricted ? "Y" : "N",
                    TopContainerId = topContainer.Id
                };
                inputList.Add(responseItem);
            }
        }

        private async Task<string> GetContainerProfileDisplayString(TopContainer container,
            ArchivesSpaceService activeService)
        {
            if (container.ContainerProfile == null)
            {
                return "";
            }
            var profileId = container.ContainerProfile.RefStrippedId;
            var profile = await activeService.GetContainerProfileAsync(profileId);
            return profile.Name; //preferred to display string due to brevity
        }

        private async Task<string> GetContainerLocationDisplayString(TopContainer container, ArchivesSpaceService activeService)
        {
            if (container == null || container.ContainerLocations.Count < 1)
            {
                return "";
            }
            var locId = container.ContainerLocations.First().RefStrippedId;
            var location = await activeService.GetLocationAsync(locId);
            return location.Title;
        }

    }
}
