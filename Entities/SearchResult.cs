using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wox.Plugin.Timezone.Entities {
    
    public class SearchResult {

        public List<CityResult> Results { get; set; }
    
    }

    public class CityResult {

        [DeserializeAs(Name = "address_components")]   
        public List<AddressComponent> AddressComponents { get; set; }

        [DeserializeAs(Name = "formatted_address")]
        public string FormattedAddress { get; set; }

        public Geometry Geometry {get;set;}

    }

    public class AddressComponent {

        [DeserializeAs(Name = "long_name")]
        public string LongName { get; set; }

        [DeserializeAs(Name = "short_name")]
        public string ShortName { get; set; }

        public List<string> Types { get; set; }

    }

    public class Geometry {
        public Location Location { get; set; }
    }

    public class Location {

        [DeserializeAs(Name = "lng")]
        public float Longtitude { get; set; }
        [DeserializeAs(Name = "lat")]
        public float Latitude { get; set; }
    }

    public class TimeZoneResult {
        public int DstOffset {get;set;}
        public int RawOffset {get;set;}
        public string Status {get;set;}
        public string TimeZoneId {get;set;}
        public string TimeZoneName {get;set;}
    }

    public class City {
        public string Name { get; set; }
        public string Country { get; set; }
        public float Longtitude { get; set; }
        public float Latitude { get; set; }
        public int Offset { get; set; }
        public string TimeZoneId { get; set; }
        public string TimeZoneName { get; set; }

        public string DisplayOffset {
            get {
                if (Offset >= 0) {
                    return "(UTC+" + Offset / 3600 + ")";
                } else {
                    return "(UTC-" + Offset / 3600 + ")";
                }
            }
        }

        public string Csv {
            get {
                StringBuilder builder = new StringBuilder();
                builder.Append(Name).Append(";")
                    .Append(Country).Append(";")
                    .Append(Longtitude).Append(";")
                    .Append(Latitude).Append(";")
                    .Append(Offset).Append(";")
                    .Append(TimeZoneId).Append(";")
                    .Append(TimeZoneName);
                return builder.ToString();
            }
        }

        public string IcoPath {
            get {
                if (Country != null && Country != "") {
                    return "flags\\" + Country.ToLower().Replace(' ', '_') + ".png";
                }
                return "flags\\_no_flag.png";
            }
        }
    }

}
