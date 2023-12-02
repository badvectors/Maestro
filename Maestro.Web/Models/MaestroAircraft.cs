using Maestro.Web.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using vatSysServer;
using vatSysServer.Models;
using Performance = vatSysServer.Performance;

namespace Maestro.Common
{
    public class MaestroAircraft : Aircraft
    {
        public MaestroAircraft() { }

        public double? PreviousAltitude { get; set; }
        public string FeederFix { get; set; } 
        public double DistanceToGo { get; set; }
        public double HeightToGo { get; set; }
        public double GlidePathToGo => Math.Round(DistanceToGo * 1000 / HeightToGo, 2);
        public Performance.PerformanceData PerformanceData { get; set; }
        public Situation Mode()
        {
            if (!Altitude.HasValue) return Situation.Unknown;
            else if (PreviousAltitude > Altitude) return Situation.Descent;
            else if (PreviousAltitude == Altitude) return Situation.Cruise;
            else if (PreviousAltitude < Altitude) return Situation.Climb;
            else if (Altitude.Value == RFL) return Situation.Cruise;
            else if (Altitude.Value < RFL && GlidePathToGo <= 3.5) return Situation.Descent;
            else if (Altitude.Value < RFL && GlidePathToGo > 3.5) return Situation.Cruise;
            return Situation.Unknown;
        }

        public enum Situation
        {
            Cruise,
            Climb,
            Descent,
            Unknown
        }

        public MaestroAircraft(Aircraft aircraft) : this()
        { 
            Callsign = aircraft.Callsign;
            SweatBox = aircraft.SweatBox;
            Type = aircraft.Type;
            FlightRules = aircraft.FlightRules;
            Wake = aircraft.Wake;
            Airport = aircraft.Airport;
            Runway = aircraft.Runway;
            STAR = aircraft.STAR;
            Position = aircraft.Position;
            GroundSpeed = aircraft.GroundSpeed;
            Altitude = aircraft.Altitude;
            PreviousAltitude = aircraft.Altitude;
            Route = aircraft.Route;
            RoutePoints = aircraft.RoutePoints;
            RFL = aircraft.RFL;
            UpdateUTC = aircraft.UpdateUTC;
            TypeChange();
        }

        private void TypeChange()
        {
            PerformanceData = Performance.GetPerformanceData(new Performance.AircraftTypeAndWake()
            {
                Type = Type,
                WakeCategory = Wake
            });
        }

        public void Update(Aircraft aircraft)
        {
            if (aircraft.Type != Type)
            {
                Type = aircraft.Type;
                Wake = aircraft.Wake;
                TypeChange();
            }
            FlightRules = aircraft.FlightRules;
            Runway = aircraft.Runway;
            STAR = aircraft.STAR;
            Position = aircraft.Position;
            GroundSpeed = aircraft.GroundSpeed;
            Route = aircraft.Route;
            PreviousAltitude = Altitude;
            Altitude = aircraft.Altitude;
            RoutePoints = aircraft.RoutePoints;
            RFL = aircraft.RFL;
            UpdateUTC = aircraft.UpdateUTC;
            FindFeederFix();
            CalculateDistance();
            CalculateHeight();
        }

        private void CalculateHeight()
        {
            if (Altitude == null) return;

            var airportData = Airspace.GetAirport(Airport);

            if (airportData == null) return;

            var destinationHeight = DEM.GetHeight(airportData.LatLong);

            HeightToGo = Math.Round(Altitude.Value - destinationHeight, 0);
        }

        private void CalculateDistance()
        {
            if (Position == null) return;

            var lastCoord = new Coordinate(Position.Latitude, Position.Longitude);

            double distanceToGo = 0;

            var lastPos = new Coordinate(Position.Latitude, Position.Longitude);

            foreach (var routePoint in RoutePoints.Where(x => !x.Passed))
            {
                var position = new Coordinate(routePoint.Latitude, routePoint.Longitude);

                distanceToGo += Conversions.CalculateDistance(lastPos, position);

                lastPos = position;
            }

            DistanceToGo = Math.Round(distanceToGo, 2);
        }

        private void FindFeederFix()
        {
            var airportData = Functions.MaestroData.Airport?.FirstOrDefault(x => x.ICAO == Airport);

            if (airportData == null)
            {
                FeederFix = null;
                return;
            }

            if (STAR != null)
            {
                string starName = Regex.Match(STAR, @"^[^0-9]*").Value;

                var feederFix = airportData.FixRunwayRules.FirstOrDefault(x => x.StarName == starName);

                if (feederFix == null) return;

                FeederFix = feederFix.Name;
            }
            else if (Route != null)
            {
                var route = Route.Split(" ").Reverse();

                foreach (var point in route)
                {
                    var feederFix = airportData.FixRunwayRules.FirstOrDefault(x => x.Airway == point);

                    if (feederFix == null) continue;

                    FeederFix = feederFix.Name;

                    Runway ??= feederFix.DistanceToRunway.Split(",")[0].Split(":")[0];

                    return;
                }
            }
        }
    }
}
