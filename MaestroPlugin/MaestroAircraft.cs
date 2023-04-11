using Newtonsoft.Json;
using System;
using System.Linq;
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
            if (fdr.Trajectory.Any()) ETO1 = fdr.Trajectory.Last()?.ETO ?? null;
            if (fdr.ParsedRoute != null) ParsedRoute = fdr.ParsedRoute;
        }

        public void FDRUpdate(FDP2.FDR fdr)
        {
            LastSeen = DateTime.UtcNow;

            FlightRules = fdr.FlightRules;
            Airport = fdr.DesAirport;
            Runway = fdr.ArrivalRunway?.Runway?.Name ?? null;
            STAR = fdr.STAR?.Name ?? null;
            if (fdr.Trajectory.Any()) ETO1 = fdr.Trajectory.Last()?.ETO ?? null;
            if (fdr.ParsedRoute != null) ParsedRoute = fdr.ParsedRoute;
        }

        public void RadarUpdate(RDP.RadarTrack radarTrack)
        {
            LastSeen = DateTime.UtcNow;

            GroundSpeed = radarTrack.GroundSpeed;

            if (ParsedRoute == null) return;

            double distanceToGo = 0;
            var lastPos = radarTrack.ActualAircraft.Position;

            foreach (var wpt in ParsedRoute.Where(x => x.ETO > DateTime.UtcNow))
            {
                distanceToGo += Conversions.CalculateDistance(lastPos, wpt.Intersection.LatLong);
                lastPos = wpt.Intersection.LatLong;
            }

            DistanceToGo = distanceToGo;

            if (GroundSpeed == 0) return;

            HoursToGo = DistanceToGo / GroundSpeed;

            ETO2 = DateTime.UtcNow.AddHours(HoursToGo.Value);
        }

        public string Callsign { get; set; }
        public string Type { get; set; }
        public string FlightRules { get; set; }
        public string Wake { get; set; }
        public string Airport { get; set; }
        public string Runway { get; set; }
        public string STAR { get; set; }
        public DateTime? ETO1 { get; set; }
        public DateTime? ETO2 { get; set; }
        public double? HoursToGo { get; set; }
        public double? GroundSpeed { get; set; }
        public double? DistanceToGo { get; set; }
        public DateTime LastSeen { get; set; }
        [JsonIgnore] public ExtractedRoute ParsedRoute { get; set; }

    }
}
