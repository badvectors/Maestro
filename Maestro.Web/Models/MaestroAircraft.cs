using Maestro.Web.Models;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using vatSysServer.Models;

namespace Maestro.Common
{
    public class MaestroAircraft : Aircraft
    {
        public MaestroAircraft() { }

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
            Route = aircraft.Route;
            RoutePoints = aircraft.RoutePoints;
            UpdateUTC = aircraft.UpdateUTC;

            FindFeederFix();
            CalculateDistance();
            CalculateETA();
        }

        public string FeederFix { get; set; }
        public bool FeederPassed => RoutePoints.FirstOrDefault(x => x.Name == FeederFix).Passed;
        public double? DistanceToFeeder { get; set; }
        public double? HoursToFeeder { get; set; }
        public double? DistanceFromFeeder { get; set; }
        public double? HoursFromFeeder { get; set; }
        public double? TotalHours => HoursToFeeder + HoursFromFeeder;
        public double? TotalDistance => DistanceToFeeder + DistanceFromFeeder;
        public DateTime? ETA { get; set; }
        public DateTime? Slot { get; set; }
        public bool SlotLocked { get; set; }
        public TimeSpan? Delta => ETA.HasValue && Slot.HasValue ? ETA.Value.Subtract(Slot.Value) : null;
        public string DeltaDisplay()
        {
            if (Delta == null) return null;
            var delta = Delta;
            if (delta.Value < TimeSpan.Zero) delta = delta.Value * -1;
            return delta.Value.TotalMinutes.ToString();
        }

        public void Update(Aircraft aircraft)
        {
            Type = aircraft.Type;
            FlightRules = aircraft.FlightRules;
            Wake = aircraft.Wake;
            Airport = aircraft.Airport;
            Runway = aircraft.Runway;
            STAR = aircraft.STAR;
            Position = aircraft.Position;
            GroundSpeed = aircraft.GroundSpeed;
            Route = aircraft.Route;
            Altitude = aircraft.Altitude;
            RoutePoints = aircraft.RoutePoints;
            UpdateUTC = aircraft.UpdateUTC;

            FindFeederFix();
        }

        public void Recalculate()
        {
            CalculateDistance();
            CalculateETA();
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

        private void CalculateDistance()
        {
            if (Position == null || FeederFix == null || GroundSpeed == 0) return;

            var feederRoutePoint = RoutePoints.FirstOrDefault(x => x.Name == FeederFix);

            if (feederRoutePoint == null)
            {
                DistanceToFeeder = null;
                HoursToFeeder = null;

                return;
            }

            var performance = Performance.GetPerformanceData(new Performance.AircraftTypeAndWake()
            {
                Type = Type,
                WakeCategory = Wake
            });

            if (FeederPassed == false)
            {
                double distanceToFeeder = 0;

                var lastPos = new Coordinate(Position.Latitude, Position.Longitude);

                foreach (var routePoint in RoutePoints.Where(x => !x.Passed))
                {
                    var position = new Coordinate(routePoint.Latitude, routePoint.Longitude);

                    distanceToFeeder += Conversions.CalculateDistance(lastPos, position);

                    if (routePoint.Name == FeederFix)
                    {
                        DistanceToFeeder = Math.Round(distanceToFeeder, 2);
                        if (GroundSpeed == 0) HoursToFeeder = null;
                        else HoursToFeeder = DistanceToFeeder / GroundSpeed;
                        break;
                    }

                    lastPos = position;
                }

                double distance = 0;

                var distanceOk = false;

                if (Runway == null || STAR == null)
                {
                    var airportData = Functions.MaestroData.Airport.FirstOrDefault(x => x.ICAO == Airport);

                    var feederFix = airportData.FixRunwayRules.FirstOrDefault(x => x.Name == FeederFix);

                    if (feederFix == null) return;

                    var runwayDistances = feederFix.DistanceToRunway.Split(",");

                    var runwayDistance = runwayDistances.FirstOrDefault(x => x.Contains(Runway));

                    distanceOk = double.TryParse(runwayDistance.Split(":")[1], out distance);
                }
                else
                {
                    foreach (var routePoint in RoutePoints.Where(x => !x.Passed).SkipWhile(x => x.Name != FeederFix))
                    {
                        if (routePoint.Name == FeederFix)
                        {
                            lastPos = new Coordinate(routePoint.Latitude, routePoint.Longitude);
                            continue;
                        }

                        var position = new Coordinate(routePoint.Latitude, routePoint.Longitude);

                        distance += Conversions.CalculateDistance(lastPos, position);

                        lastPos = position;
                    }

                    distanceOk = true;
                }

                if (!distanceOk && distance == 0)
                {
                    HoursFromFeeder = 0;
                    return;
                }

                DistanceFromFeeder = Math.Round(distance, 2);

                double hoursFromFeeder = 0;

                var pdLevel1 = performance.GetNearestDataToLevel(20000); // 30nm+ 

                var dist1 = Math.Max(0, DistanceFromFeeder.Value - 30);

                if (dist1 != 0) hoursFromFeeder += dist1 / pdLevel1.DescentSpeed.Speed;

                var pdLevel2 = performance.GetNearestDataToLevel(10000); // 20-30nm

                var dist2 = Math.Max(0, DistanceFromFeeder.Value - dist1 - 10);

                if (dist2 != 0) hoursFromFeeder += dist2 / pdLevel2.DescentSpeed.Speed;

                var pdLevel3 = performance.GetNearestDataToLevel(5000); // 10-20nm

                var dist3 = Math.Max(0, DistanceFromFeeder.Value - dist1 - dist2 - 10);

                if (dist3 != 0) hoursFromFeeder += dist3 / pdLevel3.DescentSpeed.Speed;

                var pdLevel4 = performance.GetNearestDataToLevel(3000); // 0-10nm

                var dist4 = DistanceFromFeeder.Value < 10 ? DistanceFromFeeder.Value : 10;

                if (dist4 != 0) hoursFromFeeder += dist4 / pdLevel4.DescentSpeed.Speed;

                HoursFromFeeder = hoursFromFeeder;
            }
            else
            {
                DistanceToFeeder = 0;

                HoursToFeeder = 0;

                double distanceToGo = 0;

                var lastPos = new Coordinate(Position.Latitude, Position.Longitude);

                foreach (var routePoint in RoutePoints.Where(x => !x.Passed))
                {
                    var position = new Coordinate(routePoint.Latitude, routePoint.Longitude);

                    distanceToGo += Conversions.CalculateDistance(lastPos, position);

                    lastPos = position;
                }

                DistanceFromFeeder = Math.Round(distanceToGo, 2);

                if (distanceToGo == 0)
                {
                    HoursFromFeeder = 0;
                    return;
                }

                double hoursFromFeeder = 0;

                var pdLevel1 = performance.GetNearestDataToLevel(20000); // 30nm+ 

                var dist1 = Math.Max(0, DistanceFromFeeder.Value - 30);

                if (dist1 != 0) hoursFromFeeder += dist1 / (DistanceFromFeeder.Value >= 30 ? GroundSpeed.Value : pdLevel1.DescentSpeed.Speed);

                var pdLevel2 = performance.GetNearestDataToLevel(10000); // 20-30nm

                var dist2 = Math.Max(0, DistanceFromFeeder.Value - dist1 - 10);

                if (dist2 != 0) hoursFromFeeder += dist2 / (DistanceFromFeeder.Value < 30 && DistanceFromFeeder.Value >= 20 ? GroundSpeed.Value : pdLevel2.DescentSpeed.Speed);

                var pdLevel3 = performance.GetNearestDataToLevel(5000); // 10-20nm

                var dist3 = Math.Max(0, DistanceFromFeeder.Value - dist1 - dist2 - 10);

                if (dist3 != 0) hoursFromFeeder += dist3 / (DistanceFromFeeder.Value < 20 && DistanceFromFeeder.Value >= 10 ? GroundSpeed.Value : pdLevel3.DescentSpeed.Speed);

                var pdLevel4 = performance.GetNearestDataToLevel(3000); // 0-10nm

                var dist4 = DistanceFromFeeder.Value < 10 ? DistanceFromFeeder.Value : 10;

                if (dist4 != 0) hoursFromFeeder += dist4 / (DistanceFromFeeder.Value < 10 ? GroundSpeed.Value : pdLevel4.DescentSpeed.Speed);

                HoursFromFeeder = hoursFromFeeder;
            }
        }

        private void CalculateETA()
        {
            if (!TotalHours.HasValue || double.IsInfinity(TotalHours.Value) || TotalHours.Value == 0)
            {
                //Trend = TrendDirection.None;
                ETA = null;
                return;
            }

            ETA = DateTime.UtcNow.AddHours(TotalHours.Value).RoundToMinutes();
        }
    }
}
