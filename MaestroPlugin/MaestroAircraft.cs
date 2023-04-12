using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using vatsys;
using static vatsys.FDP2.FDR;

namespace MaestroPlugin
{
    public class MaestroAircraft
    {
        public MaestroAircraft() 
        {
            LastSeen = DateTime.UtcNow;
        }

        public MaestroAircraft(FDP2.FDR fdr) : this()
        {
            Callsign = fdr.Callsign;
            Type = fdr.AircraftTypeAndWake?.Type;
            Wake = fdr.AircraftTypeAndWake?.WakeCategory;
            FlightRules = fdr.FlightRules;
            Airport = fdr.DesAirport;
            Runway = fdr.ArrivalRunway?.Runway?.Name ?? null;
            STAR = fdr.STAR?.Name ?? null;
            if (fdr.ParsedRoute != null) RouteUpdate(fdr.ParsedRoute);
        }

        public void FDRUpdate(FDP2.FDR fdr)
        {
            LastSeen = DateTime.UtcNow;

            FlightRules = fdr.FlightRules;
            Airport = fdr.DesAirport;
            Runway = fdr.ArrivalRunway?.Runway?.Name ?? null;
            STAR = fdr.STAR?.Name ?? null;
            if (fdr.ParsedRoute != null) RouteUpdate(fdr.ParsedRoute);
        }

        public void RadarUpdate(RDP.RadarTrack radarTrack)
        {
            LastSeen = DateTime.UtcNow;

            GroundSpeed = Convert.ToInt32(Math.Round(radarTrack.GroundSpeed, 0));

            if (ParsedRoute == null) return;

            double distanceToGo = 0;
            var lastPos = radarTrack.ActualAircraft.Position;

            foreach (var wpt in ParsedRoute.Where(x => x.ETO > DateTime.UtcNow))
            {
                distanceToGo += Conversions.CalculateDistance(lastPos, wpt.Intersection.LatLong);
                lastPos = wpt.Intersection.LatLong;
            }

            DistanceToGo = Math.Round(distanceToGo, 2);
        }

        private void RouteUpdate(ExtractedRoute parsedRoute)
        {
            ParsedRoute = parsedRoute;

            Route.Clear();

            foreach (var wpt in ParsedRoute.Where(x => x.ETO > DateTime.UtcNow))
            {
                Route.Add(new RoutePoint(wpt));
            }
        }

        public string Callsign { get; set; }
        public string Type { get; set; }
        public string FlightRules { get; set; }
        public string Wake { get; set; }
        public string Airport { get; set; }
        public string Runway { get; set; }
        public string STAR { get; set; }
        public int? GroundSpeed { get; set; }
        public double? DistanceToGo { get; set; }
        public List<RoutePoint> Route { get; set; } = new List<RoutePoint>();
        public DateTime LastSeen { get; set; }

        [JsonIgnore] public ExtractedRoute ParsedRoute { get; set; }

    }
}
