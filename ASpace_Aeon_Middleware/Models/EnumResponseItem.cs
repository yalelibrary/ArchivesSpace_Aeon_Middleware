using System;
using System.Runtime.Serialization;

namespace ASpace_Aeon_Middleware.Models
{
    [DataContract(Namespace = "", Name = "row")]
    public class EnumResponseItem
    {
        private string _boxNumber;
        private string _itemBarcode;
        private string _restricted;
        private string _location;
        private string _physicalDescription;
        private string _callNumber;

        [DataMember(Name = "item_id")]
        public int TopContainerId { get; set; }

        [DataMember(Name = "enumeration")]
        public string BoxNumber
        {
            get { return _boxNumber ?? ""; }
            set { _boxNumber = value; }
        }

        [DataMember(Name = "item_barcode")]
        public string ItemBarcode
        {
            get { return _itemBarcode ?? ""; }
            set { _itemBarcode = value; }
        }

        [DataMember(Name = "suppress_in_opac")]
        public string Restricted //inexplicably not a bool, uses a Y or N
        {
            get { return _restricted ?? ""; }
            set
            {
                if ((value.ToUpper() != "Y") && (value.ToUpper() != "N"))
                {
                    throw new ArgumentOutOfRangeException("value", value, "restricted property must be either 'Y' or 'N'");
                }
                _restricted = value.ToUpper();
            }
        }

        [DataMember(Name="location")]
        public string Location
        {
            get { return _location ?? ""; }
            set { _location = value; }
        }

        [DataMember(Name = "subLocation")] //doesn't follow snake-case formatting convention
        public string PhysicalDescription //name sublocation doesn't correspond to data, using more clear property name
        {
            get { return _physicalDescription ?? ""; }
            set { _physicalDescription = value; }
        }

        [DataMember(Name = "callNumber")] //same story as subLocation
        public string CallNumber
        {
            get { return _callNumber ?? ""; }
            set { _callNumber = value; }
        }
        
    }
}