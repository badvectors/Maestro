using System.Threading.Tasks;
using System.Xml;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System;
using Maestro.Common;
using System.IO;
using System.Xml.Serialization;

namespace Maestro.Web.Models
{
    public class Functions
    {
        private readonly static HttpClient Client = new();

        public static List<MaestroAircraft> AircraftData { get; set; } = new();
        public static MaestroData MaestroData { get; set; } = new();

        public static event EventHandler AircraftUpdated;
        public static bool UpdatingSlots { get; set; }

        public static void Update(Aircraft aircraft)
        {
            try
            {
                var aircraftData = AircraftData.FirstOrDefault(x => x.Callsign == aircraft.Callsign && x.SweatBox == aircraft.SweatBox);

                if (aircraftData != null)
                {
                    aircraftData.Update(aircraft);
                }
                else
                {
                    AircraftData.Add(new MaestroAircraft(aircraft));
                }

                AircraftUpdated?.Invoke(null, new EventArgs());
            }
            catch { }
        }

        public static void Remove(MaestroAircraft aircraft)
        {
            AircraftData.Remove(aircraft);

            AircraftUpdated?.Invoke(null, new EventArgs());
        }

        public static async Task LoadData()
        {
            vatSysServer.DEM.Load();

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
