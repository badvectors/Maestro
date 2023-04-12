using System.Xml;
using System.Text.RegularExpressions;
using vatSysServer.Models;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Maestro.Web
{
    public static class Airspace
    {
        private static Dictionary<string, Airport> Airports = new Dictionary<string, Airport>();
        private static Dictionary<string, Airway> Airways = new Dictionary<string, Airway>();
        private static Dictionary<string, List<Intersection>> Intersections = new Dictionary<string, List<Intersection>>();
        private static List<SystemRunway> SystemRunways = new List<SystemRunway>();

        private static List<SIDSTAR> SidStars = new List<SIDSTAR>();
        private static List<Approach> Apchs = new List<Approach>();

        public static bool Loaded = false;

        public static void LoadNavData(XmlDocument doc, bool clearExisting = true)
        {
            if (clearExisting)
            {
                Loaded = false;
                Airports.Clear();
                Airways.Clear();
                Intersections.Clear();
                SystemRunways.Clear();
                SidStars.Clear();
                Apchs.Clear();
            }

            List<string> sidLines = new();
            List<string> starLines = new();
            List<string> apchLines = new();

            XmlNodeList eList = doc.GetElementsByTagName("Airspace");
            if (eList.Count < 1)
            {
                Loaded = true;
                return;
            }

            XmlElement airspace = (XmlElement)eList[0];

            eList = airspace.GetElementsByTagName("Intersections");
            if (eList.Count > 0)
            {
                XmlElement intersectionsSection = (XmlElement)eList[0];
                XmlNodeList points = intersectionsSection.GetElementsByTagName("Point");
                foreach (XmlElement point in points)
                {
                    if (!point.HasAttribute("Name"))
                        continue;
                    Intersection wp = new Intersection(point.GetAttribute("Name").Trim().ToUpperInvariant(), new Coordinate(point.InnerText.Trim()));
                    if (point.GetAttribute("Type") == "Navaid")
                        wp.NavaidType = (Intersection.NavaidTypes)Enum.Parse(typeof(Intersection.NavaidTypes), point.GetAttribute("NavaidType"));

                    if (wp.Name.Length == 0 || wp.LatLong == null)
                        continue;

                    if (Intersections.ContainsKey(wp.Name))
                        Intersections[wp.Name].Add(wp);
                    else
                        Intersections.Add(wp.Name, new List<Intersection> { wp });
                }
            }
            eList = airspace.GetElementsByTagName("Airports");
            if (eList.Count > 0)
            {
                XmlElement airportsSection = (XmlElement)eList[0];
                XmlNodeList airports = airportsSection.GetElementsByTagName("Airport");
                foreach (XmlElement airport in airports)
                {
                    if (!airport.HasAttribute("ICAO") || !airport.HasAttribute("Position"))
                        continue;

                    Airport port = new Airport();
                    port.ICAOName = airport.GetAttribute("ICAO").Trim().ToUpperInvariant();
                    port.LatLong = new Coordinate(airport.GetAttribute("Position").Trim().ToUpperInvariant());

                    if (airport.GetAttribute("Elevation") != "")
                        port.Elevation = int.Parse(airport.GetAttribute("Elevation").Trim().ToUpperInvariant());
                    if (airport.GetAttribute("FullName") != "")
                        port.FullName = airport.GetAttribute("FullName").Trim();

                    XmlNodeList rwyList = airport.GetElementsByTagName("Runway");
                    foreach (XmlElement rwy in rwyList)
                    {
                        if (!rwy.HasAttribute("Name") || !rwy.HasAttribute("Position"))
                            continue;

                        Airport.Runway runway = new Airport.Runway(rwy.GetAttribute("Name").Trim().ToUpperInvariant(), new Coordinate(rwy.GetAttribute("Position").Trim().ToUpperInvariant()));
                        port.Runways.Add(runway);
                    }

                    Airport exists = GetAirport(port.ICAOName);
                    if (exists != null)
                    {
                        exists.LatLong = port.LatLong;
                        if (port.Elevation != -1)
                            exists.Elevation = port.Elevation;
                        if (port.FullName != "")
                            exists.FullName = port.FullName;
                        if (port.Runways.Count > 0)
                            exists.Runways = port.Runways;
                    }
                    else
                    {
                        Airports.Add(port.ICAOName, port);
                    }
                }
            }

            eList = airspace.GetElementsByTagName("Airways");
            if (eList.Count > 0)
            {
                XmlElement airwaysSection = (XmlElement)eList[0];
                XmlNodeList airways = airwaysSection.GetElementsByTagName("Airway");
                foreach (XmlElement airway in airways)
                {
                    Airway awy = new Airway();
                    awy.Name = airway.GetAttribute("Name").Trim().ToUpperInvariant();
                    string inner = airway.InnerText.Replace('\r', ' ').Replace('\n', ' ').Trim();

                    if (inner == "")
                        continue;

                    string[] inner_s = inner.Split('/');
                    bool failed = false;
                    foreach (string wp in inner_s)
                    {
                        string name = wp.Trim().ToUpperInvariant();
                        Intersection.NavaidTypes flag = Intersection.NavaidTypes.None;
                        if (Regex.IsMatch(name, @"^\w{1,5}\sVOR$"))
                        {
                            flag = Intersection.NavaidTypes.VOR;
                            name = Regex.Replace(name, @"\sVOR$", "");
                        }
                        else if (Regex.IsMatch(name, @"^\w{1,5}\sNDB$"))
                        {
                            flag = Intersection.NavaidTypes.NDB;
                            name = Regex.Replace(name, @"\sNDB$", "");
                        }
                        Intersection w = GetIntersection(name, null, flag);
                        if (w == null)
                        {
                            failed = true;
                            break;
                        }
                        awy.Intersections.Add(w);
                    }

                    if (!failed)
                        Airways.Add(awy.Name, awy);
                }
            }

            //load sid/star/apch data for this airport

            eList = airspace.GetElementsByTagName("SIDSTARs");
            if (eList.Count > 0)
            {
                XmlElement sidstarsSection = (XmlElement)eList[0];
                XmlNodeList sidsList = sidstarsSection.GetElementsByTagName("SID");
                XmlNodeList starsList = sidstarsSection.GetElementsByTagName("STAR");
                XmlNodeList appsList = sidstarsSection.GetElementsByTagName("Approach");

                foreach (XmlElement ss in sidsList.Cast<XmlNode>().Concat(starsList.Cast<XmlNode>().Concat(appsList.Cast<XmlNode>())))
                {
                    if (!ss.HasAttribute("Name") || !ss.HasAttribute("Airport"))
                        continue;

                    Airport airport = GetAirport(ss.GetAttribute("Airport").Trim().ToUpperInvariant());

                    if (airport == null)
                        continue;

                    string name = ss.GetAttribute("Name").Trim().ToUpperInvariant();

                    SIDSTAR sidstar = null;
                    Approach approach = null;

                    if (ss.Name == "Approach")
                    {
                        approach = new Approach();
                        approach.Name = name;
                        approach.Airport = airport;
                        approach.Runway = airport.Runways.SingleOrDefault(r => r.Name == ss.GetAttribute("Runway").Trim().ToUpperInvariant());
                        if (approach.Runway == null)
                            continue;
                    }
                    else
                    {
                        sidstar = new SIDSTAR();
                        sidstar.Name = name;
                        sidstar.Airport = airport;
                        string[] rwys = ss.GetAttribute("Runways").Trim().ToUpperInvariant().Split(',');
                        foreach (string r in rwys)
                        {
                            Airport.Runway rwy = airport.Runways.SingleOrDefault(rr => rr.Name == r.Trim().ToUpperInvariant());
                            if (rwy == null)
                                throw new Exception(name + " runway not found: " + r);
                            if (!sidstar.Runways.Contains(rwy))
                                sidstar.Runways.Add(rwy);
                        }
                    }

                    XmlNodeList rtes = ss.GetElementsByTagName("Route");
                    foreach (XmlElement rte in rtes)
                    {
                        List<Intersection> ints = new List<Intersection>();
                        string[] pts = rte.InnerText.Trim().ToUpperInvariant().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string pt in pts)
                        {
                            Intersection i;
                            if (pt.Length > 5 && !pt.Contains("VOR"))//lat long?
                            {
                                Coordinate pos = new Coordinate(pt);
                                i = new Intersection(Conversions.ConvertToFlightplanLatLong(pos), pos);
                            }
                            else
                                i = GetIntersection(pt);

                            if (i == null)
                                continue;

                            ints.Add(i);
                        }

                        if (sidstar != null)
                        {
                            if (rte.HasAttribute("Runway"))
                            {
                                string rwy = rte.GetAttribute("Runway").Trim().ToUpperInvariant();
                                if (sidstar == null || !sidstar.Runways.Exists(r => r.Name == rwy))
                                    continue;

                                sidstar.RunwaySpecificRoute.Add(rwy, ints);
                            }
                            else
                                sidstar.Route = ints;
                        }
                        else if (approach != null)
                            approach.Route = ints;
                    }
                    XmlNodeList trans = ss.GetElementsByTagName("Transition");
                    foreach (XmlElement tran in trans)
                    {
                        if (!tran.HasAttribute("Name"))
                            continue;

                        List<Intersection> ints = new List<Intersection>();
                        string[] pts = tran.InnerText.Trim().ToUpperInvariant().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string pt in pts)
                        {
                            var ptUpdate = pt.Replace(" VOR", "");

                            Intersection i;
                            if (ptUpdate.Length > 5)//lat long?
                            {
                                Coordinate pos = new Coordinate(ptUpdate);
                                i = new Intersection(Conversions.ConvertToFlightplanLatLong(pos), pos);
                            }
                            else
                                i = GetIntersection(ptUpdate);

                            if (i == null)
                                continue;

                            ints.Add(i);
                        }

                        if (sidstar != null)
                            sidstar.Transitions.Add(tran.GetAttribute("Name").Trim().ToUpperInvariant(), ints);
                        else if (approach != null)
                            approach.Transitions.Add(tran.GetAttribute("Name").Trim().ToUpperInvariant(), ints);
                    }

                    if (ss.Name == "SID")
                        sidstar.Type = SIDSTAR.Types.SID;
                    else if (ss.Name == "STAR")
                        sidstar.Type = SIDSTAR.Types.STAR;

                    if (sidstar != null)
                        SidStars.Add(sidstar);
                    else if (approach != null)
                        Apchs.Add(approach);
                }
            }
            eList = airspace.GetElementsByTagName("SystemRunways");
            if (eList.Count > 0)
            {

                XmlElement sysRunwaysElement = (XmlElement)eList[0];
                XmlNodeList airports = sysRunwaysElement.GetElementsByTagName("Airport");

                foreach (XmlElement airport in airports)
                {
                    Airport ap = GetAirport(airport.GetAttribute("Name"));
                    if (ap == null)
                        continue;

                    if (!SidStars.Exists(s => s.Airport == ap))//use navigraph data
                    {
                        int apIndex = sidLines.IndexOf("[" + ap.ICAOName + "]");
                        if (apIndex != -1)
                        {
                            int endIndex = sidLines.FindIndex(apIndex + 1, s => s.Contains('['));
                            if (endIndex == -1)
                                endIndex = sidLines.Count;
                            string[] sidsarray = sidLines.GetRange(apIndex, endIndex - apIndex).ToArray();
                            try
                            {
                                SidStars.AddRange(CreateSIDSTARsFromData(sidsarray, SIDSTAR.Types.SID));
                            }
                            catch { }
                        }
                        apIndex = starLines.IndexOf("[" + ap.ICAOName + "]");
                        if (apIndex != -1)
                        {
                            int endIndex = starLines.FindIndex(apIndex + 1, s => s.Contains('['));
                            if (endIndex == -1)
                                endIndex = starLines.Count;
                            string[] starsarray = starLines.GetRange(apIndex, endIndex - apIndex).ToArray();
                            try
                            {
                                SidStars.AddRange(CreateSIDSTARsFromData(starsarray, SIDSTAR.Types.STAR));
                            }
                            catch { }
                        }
                        apIndex = apchLines.IndexOf("[" + ap.ICAOName + "]");
                        if (apIndex != -1)
                        {
                            int endIndex = apchLines.FindIndex(apIndex + 1, s => s.Contains('['));
                            if (endIndex == -1)
                                endIndex = apchLines.Count;
                            string[] apchsarray = apchLines.GetRange(apIndex, endIndex - apIndex).ToArray();
                            try
                            {
                                Apchs.AddRange(CreateApproachesFromData(apchsarray));
                            }
                            catch { }
                        }
                    }

                    XmlNodeList systemRunways = airport.GetElementsByTagName("Runway");

                    foreach (XmlElement systemRunway in systemRunways)
                    {
                        if (!systemRunway.HasAttribute("Name") || !systemRunway.HasAttribute("DataRunway"))
                            continue;

                        SystemRunway runway = new SystemRunway();
                        runway.Name = systemRunway.GetAttribute("Name");
                        runway.Airport = ap;
                        runway.Runway = runway.Airport.Runways.Find(r => r.Name == systemRunway.GetAttribute("DataRunway"));

                        XmlNodeList sidsNodes = systemRunway.GetElementsByTagName("SID");
                        foreach (XmlElement sidElement in sidsNodes)
                        {
                            if (!sidElement.HasAttribute("Name"))
                                continue;

                            string name = sidElement.GetAttribute("Name");
                            List<SIDSTAR> sidMatches = SidStars.FindAll(s => s.Type == SIDSTAR.Types.SID && Regex.IsMatch(s.Name, name) && s.Airport == runway.Airport && s.Runways.Contains(runway.Runway)).ToList();

                            foreach (SIDSTAR sidMatch in sidMatches)
                            {
                                SystemRunway.AircraftType typerest = SystemRunway.AircraftType.All;
                                if (sidElement.HasAttribute("Type"))
                                    typerest = (SystemRunway.AircraftType)Enum.Parse(typeof(SystemRunway.AircraftType), sidElement.GetAttribute("Type"));

                                bool def = false;
                                if (sidElement.HasAttribute("Default"))
                                    def = bool.Parse(sidElement.GetAttribute("Default"));

                                string flag = sidElement.GetAttribute("OpDataFlag");

                                runway.SIDs.Add(new SystemRunway.SIDSTARKey(sidMatch, typerest, def, flag));
                            }
                        }

                        XmlNodeList starNodes = systemRunway.GetElementsByTagName("STAR");
                        foreach (XmlElement starElement in starNodes)
                        {
                            if (!starElement.HasAttribute("Name"))
                                continue;

                            string name = starElement.GetAttribute("Name");
                            List<SIDSTAR> starMatches = SidStars.FindAll(s => s.Type == SIDSTAR.Types.STAR && Regex.IsMatch(s.Name, name) && s.Airport == runway.Airport && s.Runways.Contains(runway.Runway)).ToList();

                            foreach (SIDSTAR starMatch in starMatches)
                            {
                                SystemRunway.AircraftType typerest = SystemRunway.AircraftType.All;
                                if (starElement.HasAttribute("Type"))
                                    typerest = (SystemRunway.AircraftType)Enum.Parse(typeof(SystemRunway.AircraftType), starElement.GetAttribute("Type"));

                                string flag = starElement.GetAttribute("OpDataFlag");

                                bool def = false;
                                if (starElement.HasAttribute("Default"))
                                    def = bool.Parse(starElement.GetAttribute("Default"));

                                Approach apch = null;
                                if (starElement.HasAttribute("ApproachName"))
                                    apch = Apchs.FirstOrDefault(a => a.Name == starElement.GetAttribute("ApproachName") && a.Airport == ap && a.Runway == runway.Runway);

                                runway.STARApproaches.Add(new SystemRunway.SIDSTARKey(starMatch, typerest, def, flag), apch);
                            }
                        }
                        SystemRunways.Add(runway);
                    }
                }
            }

            Loaded = true;
        }

        private static List<SIDSTAR> CreateSIDSTARsFromData(string[] data, SIDSTAR.Types type)
        {
            Airport airport = null;
            List<SIDSTAR> sidstars = new List<SIDSTAR>();

            for (int i = 0; i < data.Length; i++)
            {
                string line = data[i];

                int bracketIndex = line.IndexOf('[');
                if (bracketIndex != -1)//airport
                {
                    airport = GetAirport(line.Substring(bracketIndex + 1, 4));//new string(line.SkipWhile(c => c != '[').Skip(1).TakeWhile(c => c != ']').ToArray()));
                }

                if (airport == null)
                    continue;

                string[] splitLine = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (splitLine.Length == 4 && splitLine[0] == "T")//T LINE
                {
                    SIDSTAR sidstar = new SIDSTAR();
                    sidstar.Name = splitLine[1];
                    sidstar.Type = type;
                    sidstar.Airport = airport;
                    string[] rwys = splitLine[3].Split(',');
                    foreach (string rwy in rwys)
                    {
                        if (rwy.Length <= 0)
                            continue;

                        sidstar.Runways.Add(airport.Runways.Find(r => r.Name == rwy));
                    }

                    if (sidstar.Runways.Count > 0)
                        sidstars.Add(sidstar);
                }
                else if (splitLine.Length >= 6)
                {
                    string lat = "";
                    string lon = "";
                    string runway = "";
                    if (splitLine[2].Contains('/'))
                    {
                        runway = splitLine[4];
                        lat = splitLine[5];
                        lon = splitLine[6];
                    }
                    else
                    {
                        runway = splitLine[3];
                        lat = splitLine[4];
                        lon = splitLine[5];
                    }

                    if (lat == "---" || lon == "---")//disregard these points
                        continue;

                    if (splitLine[1].Length > 0 && splitLine[1][0] == '/')
                        continue;

                    string name = splitLine[0];
                    string trans = "";
                    int periodIndex = name.IndexOf('.');
                    if (periodIndex != -1)
                    {
                        if (type == SIDSTAR.Types.SID)
                        {
                            trans = name.Substring(periodIndex + 1);//new string(name.SkipWhile(c => c != '.').Skip(1).ToArray());
                            name = name.Substring(0, periodIndex);//new string(name.TakeWhile(c => c != '.').ToArray());
                        }
                        else
                        {
                            trans = name.Substring(0, periodIndex);//new string(name.TakeWhile(c => c != '.').ToArray());
                            name = name.Substring(periodIndex + 1);//new string(name.SkipWhile(c => c != '.').Skip(1).ToArray());
                        }
                    }
                    SIDSTAR sidstar = sidstars.Find(ss => ss.Name == name && ss.Airport == airport);

                    Intersection isec = new Intersection(splitLine[1], new Coordinate(double.Parse(lat), double.Parse(lon)));

                    if (trans != "")
                    {
                        if (sidstar.Transitions.ContainsKey(trans))
                            sidstar.Transitions[trans].Add(isec);
                        else
                            sidstar.Transitions.Add(trans, new List<Intersection>() { isec });
                    }
                    else if (runway != "---")
                    {
                        string[] rwys;
                        if (runway.Contains(','))
                            rwys = runway.Split(',');
                        else
                            rwys = new string[] { runway };

                        foreach (string s in rwys)
                        {
                            if (sidstar.RunwaySpecificRoute.ContainsKey(s))
                                sidstar.RunwaySpecificRoute[s].Add(isec);
                            else
                                sidstar.RunwaySpecificRoute.Add(s, new List<Intersection>() { isec });
                        }
                    }
                    else
                    {
                        sidstar.Route.Add(isec);
                    }
                }
            }

            return sidstars;
        }

        private static List<Approach> CreateApproachesFromData(string[] data)
        {
            Airport airport = null;

            List<Approach> apchs = new List<Approach>();

            bool mapReached = false;

            for (int i = 0; i < data.Length; i++)
            {
                string line = data[i];

                int bracketIndex = line.IndexOf('[');
                if (bracketIndex != -1)//airport
                {
                    airport = GetAirport(line.Substring(bracketIndex + 1, 4));//new string(line.SkipWhile(c => c != '[').Skip(1).TakeWhile(c => c != ']').ToArray()));
                }

                if (airport == null)
                    continue;

                string[] splitLine = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (splitLine.Length >= 8)
                {
                    string lat = "";
                    string lon = "";
                    string runway = splitLine[0];
                    string name = splitLine[1];
                    string isecName = splitLine[2];
                    string trans = "";
                    string type = "";
                    if (splitLine[3].Contains('/') && splitLine.Length > 8)//indicies plus one
                    {
                        isecName += splitLine[3];
                        type = splitLine[4];
                        lat = splitLine[6];
                        lon = splitLine[7];
                        trans = splitLine[8];
                    }
                    else
                    {
                        type = splitLine[3];
                        lat = splitLine[5];
                        lon = splitLine[6];
                        trans = splitLine[7];
                    }

                    if (lat == "---" || lon == "---")//disregard these points
                        continue;

                    Approach lastApch = null;
                    if (apchs.Count > 0)
                        lastApch = apchs[apchs.Count - 1];

                    Approach apch = null;

                    if (lastApch == null || lastApch.Runway.Name != runway || lastApch.Name != name)
                    {
                        apch = new Approach();
                        apch.Name = name;
                        apch.Airport = airport;
                        apch.Runway = airport.Runways.Find(r => r.Name == runway);
                        apchs.Add(apch);
                        mapReached = false;//reset
                    }
                    else
                        apch = lastApch;

                    if (type == "MAP" || mapReached) //bug out until next approach
                    {
                        mapReached = true;
                        continue;
                    }

                    Intersection isec = new Intersection(isecName, new Coordinate(double.Parse(lat), double.Parse(lon)));

                    if (trans != "----")
                    {
                        if (apch.Transitions.ContainsKey(trans))
                            apch.Transitions[trans].Add(isec);
                        else
                            apch.Transitions.Add(trans, new List<Intersection>() { isec });
                    }
                    else
                    {
                        apch.Route.Add(isec);
                    }
                }
            }

            return apchs;
        }

        public static Airport GetAirport(string icaoname)
        {
            if (string.IsNullOrWhiteSpace(icaoname)) return null;
            Airport apt;
            if (Airports.TryGetValue(icaoname, out apt))
                return apt;
            else
                return null;
        }

        public static List<string> GetAirports()
        {
            return Airports.Keys.ToList();
        }

        public static List<SystemRunway> GetRunways(string airport)
        {
            return SystemRunways.FindAll(r => r.Airport.ICAOName == airport).ToList();
        }

        public static SystemRunway GetRunway(string airport, string rwy)
        {
            return SystemRunways.Find(r => r.Airport.ICAOName == airport && r.Name == rwy);
        }

        public static List<SIDSTAR> GetSIDSTARs(string airport)
        {
            return SidStars.FindAll(r => r.Airport.ICAOName == airport).ToList();
        }

        public static Airport FindClosestAirport(Coordinate pos)
        {
            return Airports.Values.FirstOrDefault(a => Conversions.CalculateDistance(pos, a.LatLong) < 3);
        }

        public static Intersection GetIntersection(string name, Coordinate latlongQualify = null, Intersection.NavaidTypes navaidType = Intersection.NavaidTypes.None)
        {
            List<Intersection> ints;
            if (Intersections.TryGetValue(name, out ints))
            {
                if (navaidType != Intersection.NavaidTypes.None)
                    ints = ints.Where(i => i.NavaidType == navaidType).ToList();
                if (latlongQualify != null)
                    return ints.OrderBy(i => Conversions.CalculateDistance(latlongQualify, i.LatLong)).FirstOrDefault();
                else
                    return ints.FirstOrDefault();
            }
            else
                return null;
        }

        public static Airway GetAirway(string name)
        {
            Airway awy;
            if (Airways.TryGetValue(name, out awy))
                return awy;
            else
                return null;
        }

        #region Classes
        public class Airway
        {
            public string Name;
            public List<Intersection> Intersections = new List<Intersection>();
        }
        [Serializable()]
        public class Intersection
        {
            public enum Types
            {
                Fix,
                Navaid,
                Airport,
                Unknown
            }

            public enum NavaidTypes
            {
                None,
                VOR,
                NDB,
                TAC
            }
            public string Name = "";
            public string FullName = "";
            public Types Type = Types.Unknown;
            public NavaidTypes NavaidType = NavaidTypes.None;

            public Coordinate LatLong = new Coordinate();

            public Intersection()
            {
            }
            public Intersection(string name, Coordinate latLong)
            {
                Name = name;
                LatLong = latLong;
            }
        }
        public class Airport
        {
            public string ICAOName = "";
            public string FullName = "";
            public int Elevation = -1;
            public Coordinate LatLong = new Coordinate();
            public List<Runway> Runways = new List<Runway>();

            public class Runway
            {
                public string Name = "";
                public Coordinate LatLong = new Coordinate();
                public Runway()
                { }
                public Runway(string name, Coordinate latLong)
                {
                    Name = name;
                    LatLong = latLong;
                }
            }
        }

        public class SIDSTAR
        {
            public enum Types
            {
                SID,
                STAR
            }
            public string Name;
            public Airport Airport;

            public Dictionary<string, List<Intersection>> Transitions = new Dictionary<string, List<Intersection>>();
            public List<Intersection> Route = new List<Intersection>();
            public Types Type;
            public List<Airport.Runway> Runways = new List<Airport.Runway>();
            public Dictionary<string, List<Intersection>> RunwaySpecificRoute = new Dictionary<string, List<Intersection>>();
        }

        public class Approach
        {
            public string Name;
            public Airport Airport;

            public Dictionary<string, List<Intersection>> Transitions = new Dictionary<string, List<Intersection>>();
            public List<Intersection> Route = new List<Intersection>();
            public Airport.Runway Runway;
        }

        public class SystemRunway
        {
            public enum AircraftType
            {
                Jet,
                NonJet,
                All
            }
            public string Name;
            public Airport Airport;
            public Airport.Runway Runway;
            public List<SIDSTARKey> SIDs = new List<SIDSTARKey>();
            public Dictionary<SIDSTARKey, Approach> STARApproaches = new Dictionary<SIDSTARKey, Approach>();

            public struct SIDSTARKey
            {
                public readonly SIDSTAR sidStar;
                public readonly AircraftType typeRestriction;
                public readonly string opDataFlag;
                public readonly bool Default;
                public SIDSTARKey(SIDSTAR Sidstar, AircraftType TypeRestriction, bool def, string OpDataFlag = "")
                {
                    sidStar = Sidstar;
                    typeRestriction = TypeRestriction;
                    opDataFlag = OpDataFlag;
                    Default = def;
                }
            }
        }
        #endregion
    }
}
