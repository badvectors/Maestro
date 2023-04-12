using static vatsys.FDP2.FDR.ExtractedRoute;

namespace Maestro.Common
{
    public class RoutePoint
    {
        public RoutePoint() { }

        public RoutePoint(Segment segment) : this()
        {
            Name = segment.Intersection.Name;
            Latitude = segment.Intersection.LatLong.Latitude;
            Longitude = segment.Intersection.LatLong.Longitude;
        }

        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
