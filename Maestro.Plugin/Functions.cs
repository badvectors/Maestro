using Maestro.Common;
using System;
using vatsys;
using static vatsys.FDP2.FDR;

namespace Maestro.Plugin
{
    public static class Functions
    {
        public static Aircraft Create(FDP2.FDR fdr, bool isSweatBox)
        {
            var aircraft = new Aircraft
            {
                Callsign = fdr.Callsign,
                Type = fdr.AircraftTypeAndWake?.Type,
                Wake = fdr.AircraftTypeAndWake?.WakeCategory,
                FlightRules = fdr.FlightRules,
                Route = fdr.Route,
                Airport = fdr.DesAirport,
                Runway = fdr.ArrivalRunway?.Runway?.Name ?? null,
                STAR = fdr.STAR?.Name ?? null
            };

            if (isSweatBox) aircraft.SweatBox = true;

            if (fdr.ParsedRoute != null) aircraft = RouteUpdate(aircraft, fdr.ParsedRoute);

            return aircraft;
        }

        public static Aircraft FDRUpdate(Aircraft aircraft, FDP2.FDR fdr)
        {
            aircraft.UpdateUTC = DateTime.UtcNow;

            aircraft.FlightRules = fdr.FlightRules;
            aircraft.Route = fdr.Route;
            aircraft.Airport = fdr.DesAirport;
            aircraft.Runway = fdr.ArrivalRunway?.Runway?.Name ?? null;
            aircraft.STAR = fdr.STAR?.Name ?? null;

            if (fdr.ParsedRoute != null) aircraft = RouteUpdate(aircraft, fdr.ParsedRoute);

            return aircraft;
        }

        public static Aircraft RadarUpdate(Aircraft aircraft, RDP.RadarTrack radarTrack)
        {
            aircraft.UpdateUTC = DateTime.UtcNow;

            aircraft.GroundSpeed = Convert.ToInt32(Math.Round(radarTrack.GroundSpeed, 0));

            aircraft.Position = new Position()
            {
                Latitude = radarTrack.LatLong.Latitude,
                Longitude = radarTrack.LatLong.Longitude
            };

            return aircraft;
        }

        public static Aircraft RouteUpdate(Aircraft aircraft, ExtractedRoute parsedRoute)
        {
            aircraft.RoutePoints.Clear();

            foreach (var wpt in parsedRoute)
            {
                var routePoint = new RoutePoint()
                {
                    Name = wpt.Intersection.Name,
                    ETO = wpt.ETO,
                    Latitude = wpt.Intersection.LatLong.Latitude,
                    Longitude = wpt.Intersection.LatLong.Longitude
                };

                aircraft.RoutePoints.Add(routePoint);
            }

            return aircraft;
        }
    }
}
