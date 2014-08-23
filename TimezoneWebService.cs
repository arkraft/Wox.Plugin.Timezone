/*
 * Created by SharpDevelop.
 * User: Artur Kraft
 * Date: 20.08.2014
 * Time: 19:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using RestSharp;
using Wox.Plugin.Timezone.Entities;

namespace Wox.Plugin.Timezone
{
	/// <summary>
	/// Description of TraktWebService.
	/// </summary>
	public class TimezoneWebService {

        private const string SEARCH = "/geocode/json?address={q}&sensor=false";
        private const string TIMEZONE = "/timezone/json?location={latitude},{longtitude}&sensor=false&timestamp=1408641493";
		private RestClient restClient;
		
		public TimezoneWebService() {
            this.restClient = new RestClient("https://maps.googleapis.com/maps/api/");
            restClient.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2049.0 Safari/537.36";
		}
		
		public SearchResult Search(string query) {

            var request = new RestRequest(SEARCH, Method.GET);
            request.RequestFormat = DataFormat.Json;
			request.AddUrlSegment("q", query);

            var response = restClient.Execute<SearchResult>(request);
            return response.Data;
		}
		
		public TimeZoneResult TimeZone(Location location) {
			var request = new RestRequest(TIMEZONE, Method.GET);
			request.AddUrlSegment("latitude", (location.Latitude + "").Replace(',', '.'));
			request.AddUrlSegment("longtitude", (location.Longtitude + "").Replace(',', '.'));
            IRestResponse<TimeZoneResult> response = restClient.Execute<TimeZoneResult>(request);
			return response.Data;
		}
	}
}
