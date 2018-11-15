using System.Runtime.Serialization;

namespace ASpace_Aeon_Middleware.Models
{
    [DataContract(Namespace = "", Name = "row")]
    public class CombinedBarcodeResponseItem
    {
        [DataMember]
        public string Origin { get; set; }

        [DataMember]
        public string ResourceTitle { get; set; }

        [DataMember]
        public int ResourceId { get; set; }

        [DataMember]
        public string Author { get; set; }

        [DataMember]
        public string CallNumber { get; set; }

        [DataMember]
        public string SeriesTitle { get; set; }

        [DataMember]
        public string SeriesDivision { get; set; }

        [DataMember]
        public string Restriction { get; set; }

        [DataMember]
        public string BoxNumber { get; set; }

        [DataMember]
        public string Location { get; set; }

        [DataMember]
        public string Handle { get; set; }
    }
}