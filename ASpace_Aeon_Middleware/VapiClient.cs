using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Deserializers;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASpace_Aeon_Middleware
{
    public class VoyagerApiClient
    {
        private static Uri _apiBaseUrl;
        private static RestClient _restClient;

        public VoyagerApiClient()
        {
            _apiBaseUrl = Configuration.VapiUri;
            _restClient = new RestClient(_apiBaseUrl);

            //Because VAPI in both old and new doesn't respect the "accept" header and always returns what it wants for content type
            _restClient.AddHandler("text/html", new JsonDeserializer());
            _restClient.AddHandler("*", new JsonDeserializer());
        }

        public VoyagerApiClient(Uri apiBaseUrl)
        {
            _apiBaseUrl = apiBaseUrl;
            _restClient = new RestClient(_apiBaseUrl);

            //Because VAPI in both old and new doesn't respect the "accept" header and always returns what it wants for content type
            _restClient.AddHandler("text/html", new JsonDeserializer());
            _restClient.AddHandler("*", new JsonDeserializer());
        }

        public async Task<VapiBibRecord> GetBibAsync(int bibId)
        {
            return await GetAllBibItemsByBibIdAsync(bibId);
        }

        public async Task<VapiItemRecord> GetItemAsync(string barcode)
        {
            var vapiItemRecord = await GetItemByBarcodeOrItemIdAsync(barcode, null);
            return vapiItemRecord;
        }

        public async Task<VapiItemRecord> GetItemAsync(int itemId)
        {
            var vapiItemRecord = await GetItemByBarcodeOrItemIdAsync(null, itemId);
            return vapiItemRecord;
        }

        public async Task<VapiBibRecord> GetBibItemByBarcodeAsync(string barcode)
        {
            var itemRecord = await GetItemByBarcodeOrItemIdAsync(barcode, null);
            var bibRecord = await GetBibItemByBibIdAndBarcodeAsync(itemRecord.bibid, barcode);
            return bibRecord;
        }

        public async Task<VapiBibRecord> GetBibItemByBarcodeAsync(string barcode, int bibId)
        {
            var bibRecord = await GetBibItemByBibIdAndBarcodeAsync(bibId, barcode);
            return bibRecord;
        }

        public async Task<VapiBibRecord> GetBibItemByItemIdAsync(int itemId)
        {
            var itemRecord = await GetItemByBarcodeOrItemIdAsync(null, itemId);
            var bibRecord = await GetBibItemByBibIdAndItemIdAsync(itemRecord.bibid, itemId);
            return bibRecord;
        }

        private async Task<VapiBibRecord> GetBibItemByBibIdAndBarcodeAsync(int bibId, string barcode)
        {
            var bibList = await GetAllBibItemsByBibIdAsync(bibId);
            bibList.items = bibList.items.Where(i => i.barcode == barcode).ToList();
            return bibList;
        }

        private async Task<VapiBibRecord> GetBibItemByBibIdAndItemIdAsync(int bibId, int itemId)
        {
            var bibList = await GetAllBibItemsByBibIdAsync(bibId);
            bibList.items = bibList.items.Where(i => i.itemid == itemId).ToList();
            return bibList;
        }

        private async Task<VapiBibRecord> GetAllBibItemsByBibIdAsync(int bibId)
        {
            var bibList = await GetBibListAsync(bibId);
            return bibList.record.FirstOrDefault();
        }


        private async Task<VapiBibList> GetBibListAsync(int bibId)
        {
            var request = new RestRequest("GetBibItem", Method.GET);
            request.AddHeader("Accept", "application/json");
            request.AddParameter("bibid", bibId);

            //Because the API returns "NA" instead of "" the serializer chokes on the string. Our alternatives are to implement a custom deserializer or use an external one. I opted for the latter.
            //Also, rather than use a serializer properly it seems that the VAPI manually constructs JSON (or something) but the end result is that an item with an empty MFHD can lead to an array starting with a comma
            var response = await _restClient.ExecuteTaskAsync(request);
            var responseText = response.Content;
            responseText = responseText.Replace("\"NA\"", "\"\"");
            responseText = responseText.Replace(":[\r\n,\r\n{", @":[{");
            responseText = responseText.Replace(":[\n,\n{", @":[{");
            responseText = responseText.Replace("}\r\n,\r\n,\r\n{", @"},{");
            responseText = responseText.Replace("}\n,\n,\n{", @"},{");
            var responseData = JsonConvert.DeserializeObject<VapiBibList>(responseText);

            return responseData;
        }

        private async Task<VapiItemRecord> GetItemByBarcodeOrItemIdAsync(string barcode, int? itemId)
        {
            var itemList = await GetItemListByBarcodeOrItemIdAsync(barcode, itemId);
            var item = itemList.items.FirstOrDefault(i => i.barcode == barcode);
            return item;
        }

        private async Task<VapiItemList> GetItemListByBarcodeOrItemIdAsync(string barcode, int? itemId)
        {
            var request = new RestRequest("GetItem", Method.GET);
            request.AddHeader("Accept", "application/json");
            request.AddParameter("barcode", barcode);
            request.AddParameter("item_id", itemId);
            //		request.RequestFormat = DataFormat.Json;

            var response = await _restClient.ExecuteTaskAsync<VapiItemList>(request);

            return response.Data;
        }

    }

    public class VapiBibList
    {
        //The getbibitem response is defined as an array of bib records. note that due to database design there should only ever be a single bib record returned
        public List<VapiBibRecord> record { get; set; }
    }

    public class VapiBibRecord
    {
        //The bib record itself from getbibitem
        public string title { get; set; }
        public string author { get; set; }
        public string pdescription { get; set; }
        public string publisher { get; set; }
        public string pubplace { get; set; }
        public string pubdate { get; set; }
        public string isxn { get; set; }
        public string oclcmrn { get; set; }
        public int bibid { get; set; }
        public List<VapiBibItemRecord> items { get; set; }
    }

    public class VapiBibItemRecord
    {
        //Abridged item records are different for the VAPI getitem and getbibitem endpoints
        public string itypecode { get; set; }
        public string availdate { get; set; }
        public string itypename { get; set; }
        public string locname { get; set; }
        public string barcode { get; set; }
        public string itemformat { get; set; }
        public string callno { get; set; }
        public string loccode { get; set; }
        public int? itemid { get; set; } //Empty MFHD records are represented by an Item with no ItemID
        public int mfhdid { get; set; }
        public string itemenum { get; set; }
        public string itemstatus { get; set; }
        public string barcodestatus { get; set; }
        public string itemchron { get; set; }
        public string itemspinelabel { get; set; }
        public string itemstat { get; set; }   
    }

    public class VapiItemList
    {
        //The array object of item records returned by getitem
        public List<VapiItemRecord> items { get; set; }
    }

    public class VapiItemRecord
    {
        //The enumerated item records returned by getitem
        public string itypecode { get; set; }
        public int bibid { get; set; }
        public string availdate { get; set; }
        public string itypename { get; set; }
        public string locname { get; set; }
        public string barcode { get; set; }
        public string callno { get; set; }
        public string loccode { get; set; }
        public string itemenum { get; set; }
        public string itemstatus { get; set; }
        public string barcodestatus { get; set; }
        public string itemchron { get; set; }
        public string itemspinelabel { get; set; }
        public string itemstat { get; set; }
        public int? mfhdid { get; set; }
    }
}

