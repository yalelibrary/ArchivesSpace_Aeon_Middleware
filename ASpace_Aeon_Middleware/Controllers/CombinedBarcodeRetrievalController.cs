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
    public class CombinedBarcodeRetrievalController : ApiController
    {
        private readonly IAspaceSiteServiceHandler _serviceHandler;

        public CombinedBarcodeRetrievalController(IAspaceSiteServiceHandler serviceHandler)
        {
            _serviceHandler = serviceHandler;
        }

        [Route("combinedBarcodeSearch")]
        [HttpGet]
        public async Task<HttpResponseMessage> Get(string repo = "", string barcode = "")
        {
            if (repo == "")
            {
                repo = ASpace_Aeon_Middleware.Configuration.DefaultSite;
            }
            var activeService = await _serviceHandler.GetArchivesSpaceServiceAsync(repo);
            var tcManager = new ArchivesSpaceTopContainerManager(activeService);
            var vapiClient = new VoyagerApiClient();
            var resultList = new List<CombinedBarcodeResponseItem>();
            if (!String.IsNullOrWhiteSpace(barcode))
            {
                var tc = tcManager.GetTopContainerByBarcodeAsync(barcode);
                var vi = vapiClient.GetBibItemByBarcodeAsync(barcode);
                await Task.WhenAll(tc, vi);
                var tcEntry = await FormatTcEntryAsync(tc.Result, activeService);
                var viEntry = FormatViEntry(vi.Result, barcode);
                if (!String.IsNullOrWhiteSpace(tcEntry.Origin)) // this always set if there's a result
                {
                    resultList.Add(tcEntry);
                }
                if (!String.IsNullOrWhiteSpace(viEntry.Origin))
                {
                    resultList.Add(viEntry);
                }                
                return Request.CreateResponse(HttpStatusCode.OK, resultList);
            }
            return Request.CreateResponse(HttpStatusCode.OK, resultList);
        }

        private async Task<CombinedBarcodeResponseItem> FormatTcEntryAsync(TopContainer container,
            ArchivesSpaceService activeService)
        {
            if (container == null)
            {
                return new CombinedBarcodeResponseItem();
            }
            var location = await GetContainerLocationDisplayString(container, activeService);
            var resourceManager = new ArchivesSpaceResourceManager(activeService);

            var resource = await resourceManager.GetResourceByIdAsync(container.Collection.First().RefStrippedId);
            var hasSeries = container.Series.Count > 0;
            var seriesDivision = hasSeries ? container.Series.First().Identifier.FormatSeriesDivision() : "";
            var seriesTitle = hasSeries ? container.Series.First().DisplayString : "";
            var tcEntry = new CombinedBarcodeResponseItem
            {
                Origin = "ArchivesSpace",
                Author = "",
                BoxNumber = Helpers.FormatBoxText(container.Indicator),
                CallNumber = Helpers.FormatCallNumber(container.Collection.First().Identifier),
                Location = location,
                ResourceTitle = container.Collection.First().DisplayString,
                ResourceId = container.Collection.First().RefStrippedId,
                Handle = resource.EadLocation, //This seems to be null pretty often even if EadId (not a URL) is not
                Restriction = (container.Restricted ? "Y" : "N"),
                SeriesDivision = seriesDivision,
                SeriesTitle = seriesTitle
            };
            return tcEntry;
        }

        private CombinedBarcodeResponseItem FormatViEntry(VapiBibRecord record, string barcode)
        {
            if (record == null)
            {
                return new CombinedBarcodeResponseItem();
            }
            var item = record.items.FirstOrDefault(x => x.barcode == barcode);
            if (item == null)
            {
                return new CombinedBarcodeResponseItem();
            }
            var tcEntry = new CombinedBarcodeResponseItem
            {
                Origin = "Voyager",
                Author = record.author,
                BoxNumber = item.itemenum,
                CallNumber = item.callno,
                Location = item.locname,
                ResourceTitle = record.title,
                ResourceId = record.bibid,
                Handle = String.Format("http://hdl.handle.net/10079/bibid/{0}", record.bibid),
                Restriction = item.itemstat,
                SeriesDivision = item.itemchron,
                SeriesTitle = ""
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
