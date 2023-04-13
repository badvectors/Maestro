using System.Threading.Tasks;
using System.Xml;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System;
using Maestro.Common;

namespace Maestro.Web
{
    public class Functions
    {
        private readonly static HttpClient Client = new();

        public static List<Aircraft> Aircraft = new();
        public static List<AircraftData> AircraftData = new();

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
                    existing.Route = aircraft.Route;
                    existing.UpdateUTC = aircraft.UpdateUTC;

                    var aircraftData = AircraftData.FirstOrDefault(x => x.Callsign == aircraft.Callsign);

                    if (aircraftData != null) aircraftData.Update(aircraft);
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
        }
    }
}
