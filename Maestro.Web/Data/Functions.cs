using System.Threading.Tasks;
using System.Xml;
using System.Net.Http;
using System.Collections.Generic;
using Maestro.Web.Data;
using System.Linq;
using System;

namespace Maestro.Web
{
    public class Functions
    {
        private readonly static HttpClient Client = new();

        public static List<MaestroAircraft> Aircraft = new();

        public static event EventHandler AircraftUpdated;

        public static void Update(MaestroAircraft maestroAircraft)
        {
            try
            {
                var existing = Aircraft.FirstOrDefault(x => x.Callsign == maestroAircraft.Callsign);

                if (existing != null)
                {
                    existing.Update(maestroAircraft);
                }
                else
                {
                    Aircraft.Add(maestroAircraft);
                }

                AircraftUpdated?.Invoke(null, new EventArgs());
            }
            catch { }
        }

        public static void Remove(MaestroAircraft maestroAircraft)
        {
            Aircraft.Remove(maestroAircraft);

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
