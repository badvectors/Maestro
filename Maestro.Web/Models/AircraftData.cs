using Maestro.Web;
using Maestro.Web.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using vatSysServer.Models;

namespace Maestro.Common
{
    public class AircraftData
    {
        public AircraftData() { }

        public AircraftData(Aircraft aircraft) : this()
        { 
            Callsign = aircraft.Callsign;
            SweatBox = aircraft.SweatBox;

            FindFeederFix(aircraft);

            CalculateDistance(aircraft);
        }

        public string Callsign { get; set; }
        public bool SweatBox { get; set; }
        public string FeederFix { get; set; }
        public bool FeederPassed { get; set; }
        public double? DistanceToFeeder { get; set; }
        public double? HoursToFeeder { get; set; }
        public double? DistanceFromFeeder { get; set; }
        public double? HoursFromFeeder { get; set; }
        public double? TotalHours => HoursToFeeder + HoursFromFeeder;
        public double? TotalDistance => DistanceToFeeder + DistanceFromFeeder;
        public DateTime? ETA
        {
            get
            {
                if (!TotalHours.HasValue || TotalHours.Value == 0) return null;

                return DateTime.UtcNow.AddHours(TotalHours.Value);
            }
        }
        public TrendDirection Trend { get; set; }

        public void Update(Aircraft aircraft)
        {
            FindFeederFix(aircraft);

            CalculateDistance(aircraft);
        }

        private void FindFeederFix(Aircraft aircraft)
        {
            var airportData = Functions.MaestroData.Airport.FirstOrDefault(x => x.ICAO == aircraft.Airport);

            if (airportData == null)
            {
                FeederFix = null;
                return; 
            }

            if (aircraft.STAR != null)
            {
                string starName = Regex.Match(aircraft.STAR, @"^[^0-9]*").Value;

                var feederFix = airportData.FixRunwayRules.FirstOrDefault(x => x.StarName == starName);

                if (feederFix == null) return;

                FeederFix = feederFix.Name;
            }
        }

        private void CalculateDistance(Aircraft aircraft)
        {
            if (aircraft.Position == null || FeederFix == null) return;

            var feederRoutePoint = aircraft.RoutePoints.FirstOrDefault(x => x.Name == FeederFix);

            if (feederRoutePoint == null)
            {
                DistanceToFeeder = null;
                HoursToFeeder = null;

                return;
            }

            var atw = new Performance.AircraftTypeAndWake()
            {
                Type = aircraft.Type,
                WakeCategory = aircraft.Wake
            };

            var performance = Performance.GetPerformanceData(atw);

            if (!feederRoutePoint.Passed)
            {
                var pdLevel = performance.GetNearestDataToLevel(5000);

                double distanceToFeeder = 0;

                var lastPos = new Coordinate(aircraft.Position.Latitude, aircraft.Position.Longitude);

                foreach (var routePoint in aircraft.RoutePoints.Where(x => !x.Passed))
                {
                    var position = new Coordinate(routePoint.Latitude, routePoint.Longitude);

                    distanceToFeeder += Conversions.CalculateDistance(lastPos, position);

                    if (routePoint.Name == FeederFix)
                    {
                        DistanceToFeeder = Math.Round(distanceToFeeder, 2);
                        if (aircraft.GroundSpeed == 0) HoursToFeeder = null;
                        else HoursToFeeder = DistanceToFeeder / aircraft.GroundSpeed;
                        break;
                    }

                    lastPos = position;
                }

                var airportData = Functions.MaestroData.Airport.FirstOrDefault(x => x.ICAO == aircraft.Airport);

                var feederFix = airportData.FixRunwayRules.FirstOrDefault(x => x.Name == FeederFix);

                if (feederFix == null) return;

                var runwayDistances = feederFix.DistanceToRunway.Split(",");

                var runwayDistance = runwayDistances.FirstOrDefault(x => x.Contains(aircraft.Runway));

                if (runwayDistance != null && FeederPassed == false)
                {
                    var distanceOk = double.TryParse(runwayDistance.Split(":")[1], out var distance);

                    if (!distanceOk) return;

                    DistanceFromFeeder = Math.Round(distance, 2);
                    HoursFromFeeder = DistanceFromFeeder / pdLevel.DescentSpeed.Speed;
                }
            }
            else
            {
                var pdLevel = performance.GetNearestDataToLevel(aircraft.Altitude ?? 5000);

                DistanceToFeeder = 0;
                HoursToFeeder = 0;

                double distanceToGo = 0;

                var lastPos = new Coordinate(aircraft.Position.Latitude, aircraft.Position.Longitude);

                foreach (var routePoint in aircraft.RoutePoints.Where(x => !x.Passed))
                {
                    var position = new Coordinate(routePoint.Latitude, routePoint.Longitude);

                    distanceToGo += Conversions.CalculateDistance(lastPos, position);

                    lastPos = position;
                }

                DistanceFromFeeder = Math.Round(distanceToGo, 2);
                HoursFromFeeder = DistanceFromFeeder / pdLevel.DescentSpeed.Speed;
            }
        }

        public enum TrendDirection
        {
            None,
            Stable,
            Slower,
            Faster
        }
    }
}
