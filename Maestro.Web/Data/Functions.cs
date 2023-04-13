using System.Threading.Tasks;
using System.Xml;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System;
using Maestro.Common;
using System.IO;
using System.Xml.Serialization;
using Maestro.Web.Data;
using Maestro.Web.Models;

namespace Maestro.Web
{
    public class Functions
    {
        private readonly static HttpClient Client = new();

        public static List<Aircraft> Aircraft { get; set; } = new();
        public static List<AircraftData> AircraftData { get; set; } = new();
        public static MaestroData MaestroData { get; set;} = new();

        public static event EventHandler AircraftUpdated;

        public static void Update(Aircraft aircraft)
        {
            try
            {
                var existing = Aircraft.FirstOrDefault(x => x.Callsign == aircraft.Callsign && x.SweatBox == aircraft.SweatBox);

                if (existing != null)
                {
                    existing.Type = aircraft.Type;
                    existing.FlightRules = aircraft.FlightRules;
                    existing.Wake = aircraft.Wake;
                    existing.Airport = aircraft.Airport;
                    existing.Runway = aircraft.Runway;
                    existing.STAR = aircraft.STAR;
                    existing.Position = aircraft.Position;
                    existing.GroundSpeed = aircraft.GroundSpeed;
                    existing.RoutePoints = aircraft.RoutePoints;
                    existing.UpdateUTC = aircraft.UpdateUTC;

                    var aircraftData = AircraftData.FirstOrDefault(x => x.Callsign == aircraft.Callsign);

                    aircraftData?.Update(aircraft);
                }
                else
                {
                    Aircraft.Add(aircraft);

                    AircraftData.Add(new AircraftData(aircraft));
                }

                AircraftUpdated?.Invoke(null, new EventArgs());
            }
            catch { }
        }

        public static void Remove(Aircraft aircraft)
        {
            Aircraft.Remove(aircraft);

            var aircraftData = AircraftData.FirstOrDefault(x => x.Callsign == aircraft.Callsign);

            if (aircraftData != null) AircraftData.Remove(aircraftData);

            AircraftUpdated?.Invoke(null, new EventArgs());
        }

        public static async Task LoadData()
        {
            var url = "https://raw.githubusercontent.com/vatSys/australia-dataset/master/Airspace.xml";

            try
            {
                var response = Client.GetAsync(url).Result;
                if (!response.IsSuccessStatusCode) return;
                var result = await response.Content.ReadAsStringAsync();
                var xmldocument = new XmlDocument();
                xmldocument.LoadXml(result);
                Airspace.LoadNavData(xmldocument, false);
            }
            catch { }

            url = "https://raw.githubusercontent.com/vatSys/australia-dataset/master/Performance.xml";

            try
            {
                var response = Client.GetAsync(url).Result;
                if (!response.IsSuccessStatusCode) return;
                var result = await response.Content.ReadAsStringAsync();
                var xmldocument = new XmlDocument();
                xmldocument.LoadXml(result);
                Performance.LoadPerformance(xmldocument, false);
            }
            catch { }

            var path = $@"{AppContext.BaseDirectory}Data\Maestro.xml";

            if (File.Exists(path))
            {
                string readText = File.ReadAllText(path);
                var serializer = new XmlSerializer(typeof(MaestroData));
                using var reader = new StringReader(readText);
                MaestroData = (MaestroData)serializer.Deserialize(reader);
            }
        }
    }
}
