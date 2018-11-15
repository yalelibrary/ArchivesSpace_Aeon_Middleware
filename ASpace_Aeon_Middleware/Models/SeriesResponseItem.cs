using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ASpace_Aeon_Middleware.Models
{
    [DataContract(Namespace = "", Name = "row")]
    public class SeriesResponseItem
    {
        private string _seriesDiv;
        private string _seriesTitle;
        private string _collectionTitle;
        private string _eadLocation;


        [DataMember(Name = "series_id")]  
        public int SeriesId { get; set; }

        [DataMember(Name = "series_div")] //setting EmitDefaultValue = false causes the node to disappear if the value is null
        public string SeriesDiv
        {
            get { return _seriesDiv ?? ""; } //otherwise the xmlserializer sets a null with the property i:nil="true" inside the self-closing tag rather than just a self-closing tag
            set { _seriesDiv = value; }    
        }
        
        [DataMember(Name = "series_title")]  
        public string SeriesTitle
        {
            get { return _seriesTitle ?? ""; }
            set { _seriesTitle = value; }
        }

        [DataMember(Name = "collection_title")]  
        public string CollectionTitle
        {
            get { return _collectionTitle ?? ""; }
            set { _collectionTitle = value; }
        }

        [DataMember(Name = "ead_location")]  
        public string EadLocation
        {
            get { return _eadLocation ?? ""; }
            set { _eadLocation = value; }
        }
    }
}