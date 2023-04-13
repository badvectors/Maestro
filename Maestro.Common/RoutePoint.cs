using System;

namespace Maestro.Common
{
    public class RoutePoint
    {
        public string Name { get; set; }
        public DateTime ETO { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool Passed => ETO < DateTime.UtcNow;
    }
}
