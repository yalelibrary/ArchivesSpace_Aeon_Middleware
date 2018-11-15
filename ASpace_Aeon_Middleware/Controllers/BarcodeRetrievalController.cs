using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ArchivesSpace_.Net_Client;
using ArchivesSpace_.Net_Client.Models;
using ASpace_Aeon_Middleware.Interfaces;
using ASpace_Aeon_Middleware.Models;

namespace ASpace_Aeon_Middleware.Controllers
{
    public class BarcodeRetrievalController : ApiController
    {
        private readonly IAspaceSiteServiceHandler _serviceHandler;

        public BarcodeRetrievalController(IAspaceSiteServiceHandler serviceHandler)
        {
            _serviceHandler = serviceHandler;
        }

        [Route("barcode")]
        [Route("list_atkcache_barcode_info")]
        [Route("list_atkcache_barcode_info.ashx")]
        [HttpGet]
        public async Task<HttpResponseMessage> Get(string repo = "", string barcode = ""
            , [FromUri(Name = "item_id")]int? topContainerIdNullable = 0)
        {
            var topContainerId = topContainerIdNullable ?? 0; //if a querystring specifies the parameter with no value the binding fails with a "non-nullable type" error even if a default is specified. Int params must be nullable ints.
            if (repo == "")
            {
                repo = ASpace_Aeon_Middleware.Configuration.DefaultSite;
            }
            var activeService = await _serviceHandler.GetArchivesSpaceServiceAsync(repo);
            var tcManager = new ArchivesSpaceTopContainerManager(activeService);
            var resultList = new List<BarcodeResponseItem>();
            if (topContainerId != 0)
            {
                var tc = await tcManager.GetTopContainerByIdAsync(topContainerId);
                var tcEntry = await FormatEntry(tc, activeService);
                resultList.Add(tcEntry);
                return Request.CreateResponse(HttpStatusCode.OK, resultList);    
            }
            if (!String.IsNullOrWhiteSpace(barcode))
            {
                var tc = await tcManager.GetTopContainerByBarcodeAsync(barcode);
                if (tc == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, resultList);
                }
                var tcEntry = await FormatEntry(tc, activeService);
                resultList.Add(tcEntry);
                return Request.CreateResponse(HttpStatusCode.OK, resultList);    
            }
            return Request.CreateResponse(HttpStatusCode.OK, resultList);    
        }

        private async Task<BarcodeResponseItem> FormatEntry(TopContainer container,
            ArchivesSpaceService activeService)
        {
            if (container == null)
            {
                return new BarcodeResponseItem();
            }
            var location = await GetContainerLocationDisplayString(container, activeService);
            var tcEntry = new BarcodeResponseItem
            {
                Author = "",
                BoxNumber = Helpers.FormatBoxText(container.Indicator),
                CallNumber = Helpers.FormatCallNumber(container.Collection.First().Identifier),
                Location = location,
                Title = container.Collection.First().DisplayString,
                ResourceId = container.Collection.First().RefStrippedId
            };
            return tcEntry;
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
