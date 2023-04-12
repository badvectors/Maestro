using System;

namespace Maestro.Web.Data
{
    public class MaestroAircraft
    {
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

        public void Update(MaestroAircraft update)
        {
            FlightRules = update.FlightRules;
            Airport = update.Airport;
            Runway = update.Runway;
            STAR = update.STAR;
            ETO1 = update.ETO1;
            ETO2 = update.ETO2;
            HoursToGo = update.HoursToGo;
            GroundSpeed = update.GroundSpeed;
            DistanceToGo = update.DistanceToGo;
            LastSeen = update.LastSeen;
        }

        public bool GetOK()
        {
            if (!GroundSpeed.HasValue || GroundSpeed.Value == 0) return false;
            if (!DistanceToGo.HasValue || DistanceToGo.Value == 0) return false;
            if (string.IsNullOrEmpty(STAR) && FlightRules == "I") return false;
            return true;
        }

        public string GetETO()
        {
            if (ETO2.HasValue) return ETO2.Value.ToString("HHmm");
            if (ETO1.HasValue) return ETO1.Value.ToString("HHmm");
            return string.Empty;
        }

        public string GetDTG()
        {
            if (DistanceToGo.HasValue) return Math.Round(DistanceToGo.Value, 0).ToString();
            return string.Empty;
        }

        public string GetSpeed()
        {
            if (GroundSpeed.HasValue) return Math.Round(GroundSpeed.Value, 0).ToString();
            return string.Empty;
        }
    }
}
