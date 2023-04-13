using System;
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

            CalculateDistances(aircraft);
        }

        public string Callsign { get; set; }
        public bool SweatBox { get; set; }
        public double? DistanceToGo { get; set; }

        public void Update(Aircraft aircraft)
        {
            CalculateDistances(aircraft);
        }

        public void CalculateDistances(Aircraft aircraft)
        {
            double distanceToGo = 0;

            var lastPos = new Coordinate(aircraft.Position.Latitude, aircraft.Position.Longitude);

            foreach (var routePoint in aircraft.Route)
            {
                var position = new Coordinate(routePoint.Latitude, routePoint.Longitude);

                distanceToGo += Conversions.CalculateDistance(lastPos, position);

                lastPos = position;
            }

            DistanceToGo = Math.Round(distanceToGo, 2);
        }
    }
}
