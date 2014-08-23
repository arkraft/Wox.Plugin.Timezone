using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Wox.Plugin.Timezone.Entities;

namespace Wox.Plugin.Timezone
{
    public class TimeZone : IPlugin {

        private PluginInitContext context;
        private const string seperator = " • ";
        private string PluginPath;
        private List<City> cities;
        private TimezoneWebService webservice;


        public void Init( PluginInitContext context ) {
            this.webservice = new TimezoneWebService();
            this.cities = new List<City>();
            this.context = context;
            PluginPath = context.CurrentPluginMetadata.PluginDirectory;
            if (File.Exists(PluginPath + "/cities.txt")) {
                string[] lines = File.ReadAllLines(PluginPath + "/cities.txt");
                foreach (string s in lines) {
                    string[] components = s.Split(';');
                    cities.Add(new City() {
                        Name = components[0],
                        Country = components[1],
                        Longtitude = float.Parse(components[2]),
                        Latitude = float.Parse(components[3]),
                        Offset = int.Parse(components[4]),
                        TimeZoneId = components[5],
                        TimeZoneName = components[6]
                    });
                }
            } else {
                File.Create(PluginPath + "/cities.txt");
            }
        }

        public List<Result> Query( Query query ) {

            List<Result> result = new List<Result>();
            
            if (query.ActionParameters.Count == 0) {

                foreach (City city in cities) {
                    result.Add(getShowResult(city));
                }
                result.Add(new Result() {
                    Title = "tz add <city_name>",
                    SubTitle = "Add a new city to the list",
                    IcoPath = "kworldclock.png",
                    Action = e => {
                        context.API.ChangeQuery("tz add ");
                        return false;
                    }
                });
                result.Add(new Result() {
                    Title = "tz remove <city_name>",
                    SubTitle = "Remove a city from the list",
                    IcoPath = "kworldclock.png",
                    Action = e => {
                        context.API.ChangeQuery("tz remove ");
                        return false;
                    }
                });
            } else if(query.ActionParameters.Count >= 1) {
                switch (query.ActionParameters[0]) {
                    case "add":
                        if (query.ActionParameters.Count == 1) {
                            result.Add(new Result() {
                                Title = "Add a new city",
                                SubTitle = "Type the name of the city you want to add",
                                IcoPath = "kworldclock.png",
                                Action = e => false
                            });
                        } else {
                            string q = "";
                            for (var i = 1; i < query.ActionParameters.Count; i++) {
                                q += query.ActionParameters[i];
                            }
                            SearchResult searchResult = webservice.Search(q);
                            foreach (CityResult city in searchResult.Results) {
                                float lng = city.Geometry.Location.Longtitude;
                                float lat = city.Geometry.Location.Latitude;
                                List<AddressComponent> components = city.AddressComponents;
                                List<AddressComponent> filtered = components.Where(o => o.Types.Contains("country")).ToList();
                                string country = "";
                                if (filtered.Count > 0) {
                                    country = filtered[0].LongName.ToLower().Replace(' ', '_');
                                }

                                result.Add(new Result() {
                                    Title = city.FormattedAddress,
                                    SubTitle = "Add lng: " + lng + ", lat: " + lat + " to your list",
                                    IcoPath = "flags\\" + country + ".png",
                                    Action = e => {
                                        addCity(city);
                                        context.API.ChangeQuery("tz ");
                                        return false;
                                    }
                                });
                            }
                        }
                        break;
                    case "remove":
                        if (query.ActionParameters.Count == 1) {
                            foreach (City city in cities) {
                                result.Add(buildRemoveResult(city));
                            }
                        } else {
                            string q = "";
                            for (var i = 1; i < query.ActionParameters.Count; i++) {
                                q += query.ActionParameters[i];
                            }
                            List<City> removed = cities.Where(o => o.Name.ToLower().Contains(q.ToLower())).ToList();
                            foreach (City city in removed) {
                                result.Add(buildRemoveResult(city));
                            }
                        }
                        break;
                    default:
                        string search = "";
                        for (var i = 0; i < query.ActionParameters.Count; i++) {
                            search += query.ActionParameters[i];
                        }
                        List<City> filteredSearch = cities.Where(o => o.Name.ToLower().Contains(search.ToLower())).ToList();
                        foreach (City city in filteredSearch) {
                            result.Add(getShowResult(city));
                        }
                        if ("add".Contains(query.ActionParameters[0].ToLower())) {
                            result.Add(new Result() {
                                Title = "tz add <city_name>",
                                SubTitle = "Add a new city to the list",
                                IcoPath = "kworldclock.png",
                                Action = e => {
                                    context.API.ChangeQuery("tz add ");
                                    return false;
                                }
                            });
                        }
                        if ("remove".Contains(query.ActionParameters[0].ToLower())) {
                            result.Add(new Result() {
                                Title = "tz remove <city_name>",
                                SubTitle = "Remove a city from the list",
                                IcoPath = "kworldclock.png",
                                Action = e => {
                                    context.API.ChangeQuery("tz remove ");
                                    return false;
                                }
                            });
                        }
                        break;
                }
            }

            return result;
        }

        public Result buildRemoveResult( City city ) {
            Result r = new Result() {
                Title = city.Name,
                SubTitle = "Remove '" + city.Name + "' from your list",
                IcoPath = city.IcoPath,
                Action = e => {
                    remove(city.Name);
                    save();
                    context.API.ChangeQuery("tz ");
                    return false;
                }
            };
            return r;
        }

        public void remove( string name ) {
            cities = cities.Where(o => o.Name != name).ToList();
        }

        public Result getShowResult( City city ) {
            DateTime time = DateTime.UtcNow.AddHours(city.Offset / 3600);
            Result r = new Result() {
                Title = city.Name + ": " + String.Format("{0:t}", time),
                SubTitle = String.Format("{0:D}", time) + seperator + city.Country + seperator + city.TimeZoneName,
                IcoPath = city.IcoPath,
                Action = e => false
            };
            return r;
        }

        public void addCity( CityResult city ) {
            List<AddressComponent> components = city.AddressComponents;
            List<AddressComponent> countryFiltered = components.Where(o => o.Types.Contains("country")).ToList();
            List<AddressComponent> localityFiltered = components.Where(o => o.Types.Contains("locality")).ToList();

            string country = "";
            if (countryFiltered.Count > 0) {
                country = countryFiltered[0].LongName;
            }
            string name;
            if (localityFiltered.Count > 0) {
                name = localityFiltered[0].LongName;
            } else {
                name = city.FormattedAddress;
            }
            
            TimeZoneResult timezone = webservice.TimeZone(city.Geometry.Location);

            City c = new City() {
                Name = name,
                Country = country,
                Longtitude = city.Geometry.Location.Longtitude,
                Latitude = city.Geometry.Location.Latitude,
                Offset = timezone.DstOffset + timezone.RawOffset,
                TimeZoneId = timezone.TimeZoneId,
                TimeZoneName = timezone.TimeZoneName
            };
            cities.Add(c);
            save();
        }

        public void save() {
            var strings = cities.ConvertAll(o => o.Csv);
            File.WriteAllLines(PluginPath + "\\cities.txt", strings.ToArray());
        }
    }
}
