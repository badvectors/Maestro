using System;
using System.Collections.Generic;

namespace Maestro.Common
{
    public class Aircraft
    {
        public Aircraft() 
        {
            UpdateUTC = DateTime.UtcNow;   
        }

        public string Callsign { get; set; }
        public bool SweatBox { get; set; }
        public string Type { get; set; }
        public string FlightRules { get; set; }
        public string Wake { get; set; }
        public string Airport { get; set; }
        public string Runway { get; set; }
        public string STAR { get; set; }
        public Position Position { get; set; }
        public int RFL { get; set; }
        public int? Altitude { get; set; }
        public int? GroundSpeed { get; set; }
        public string Route { get; set; }
        public List<RoutePoint> RoutePoints { get; set; } = new List<RoutePoint>();
        public DateTime UpdateUTC { get; set; }
    }
}
