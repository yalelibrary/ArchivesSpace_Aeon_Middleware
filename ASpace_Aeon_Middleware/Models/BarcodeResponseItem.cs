using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ASpace_Aeon_Middleware.Models
{
    [DataContract(Namespace = "", Name = "row")]
    public class BarcodeResponseItem
    {
        private string _boxNumber;
        private string _title;
        private string _author;
        private string _location;
        private string _callNumber;

        [DataMember(Name = "mfhd_id")]
        public int ResourceId { get; set; }

        [DataMember(Name = "call_no")]
        public string CallNumber
        {
            get { return _callNumber ?? ""; }
            set { _callNumber = value; }
        }


        [DataMember(Name="collection")] //These don't align at all, unsure why an entry titled collection is returning location
        public string Location
        {
            get { return _location ?? ""; }
            set { _location = value; }
        }

        [DataMember(Name = "author")] //This is always blank in the legacy service
        public string Author
        {
            get { return _author ?? ""; }
            set { _author = value; }
        }

        [DataMember(Name = "title")]
        public string Title
        {
            get { return _title ?? ""; }
            set { _title = value; }
        }

        [DataMember(Name = "enumeration")] //Seems to also be blank in legacy service
        public string BoxNumber
        {
            get { return _boxNumber ?? ""; }
            set { _boxNumber = value; }
        }
    }
}