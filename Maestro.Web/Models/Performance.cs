using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Maestro.Web.Models
{
    public static class Performance
    {
        public class CalculatedProfileData
        {
            public double Altitude;

            public SpeedData Speed;
        }

        public class PerformanceData
        {
            public class DataAtLevel : IComparable
            {
                public int Altitude = -1;

                public short ClimbRate = -1;

                public short DescentRate = -1;

                public SpeedData CruiseSpeed = new SpeedData();

                public SpeedData ClimbSpeed = new SpeedData();

                public SpeedData DescentSpeed = new SpeedData();

                public int CompareTo(object obj)
                {
                    DataAtLevel dataAtLevel = (DataAtLevel)obj;
                    return Altitude.CompareTo(dataAtLevel.Altitude);
                }
            }

            public List<AircraftTypeAndWake> AircraftTypes = new List<AircraftTypeAndWake>();

            public List<DataAtLevel> AltitudeData = new List<DataAtLevel>();

            public string PerfCategory = "";

            public int TakeoffSpeed = -1;

            public int MaxSpeed = -1;

            public int MaxAltitude = -1;

            public bool FasterThanMach;

            public bool IsJet;

            public DataAtLevel GetNearestDataToLevel(int DesiredAltitude)
            {
                DesiredAltitude /= 100;
                DataAtLevel dataAtLevel = new DataAtLevel();
                dataAtLevel.Altitude = DesiredAltitude;
                int num = AltitudeData.BinarySearch(dataAtLevel);
                if (num < 0)
                {
                    int num2 = ~num;
                    if (num2 == AltitudeData.Count)
                    {
                        num = AltitudeData.Count - 1;
                    }
                    else if (num2 == 0)
                    {
                        num = 0;
                    }
                    else
                    {
                        int value = AltitudeData[num2].Altitude - DesiredAltitude;
                        num = ((Math.Abs(AltitudeData[num2 - 1].Altitude - DesiredAltitude) >= Math.Abs(value)) ? num2 : (num2 - 1));
                    }
                }

                return AltitudeData[num];
            }

            public AircraftTypeAndWake GetAircraftTypeAndWake(string Type)
            {
                foreach (AircraftTypeAndWake aircraftType in AircraftTypes)
                {
                    if (aircraftType.Type == Type)
                    {
                        return aircraftType;
                    }
                }

                return null;
            }
        }

        public class AircraftTypeAndWake
        {
            public string Type = "";

            public string WakeCategory = "";
        }

        public class SpeedData
        {
            public enum SpeedTypes
            {
                IAS,
                TAS,
                MACH
            }

            public SpeedTypes Type;

            public double Speed = -1.0;

            public SpeedData()
            {
            }

            public SpeedData(SpeedTypes type, double speed)
            {
                Type = type;
                Speed = speed;
            }
        }

        public static bool Loaded = false;

        public static List<PerformanceData> AircraftPerformanceData = new List<PerformanceData>();

        public static void LoadPerformance(XmlDocument xmlDocument, bool clearExisting = true)
        {
            if (clearExisting) AircraftPerformanceData.Clear();

            XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("PerformanceData");
            for (int i = 0; i < elementsByTagName.Count; i++)
            {
                XmlElement xmlElement = (XmlElement)elementsByTagName[i];
                PerformanceData performanceData = new PerformanceData();
                performanceData.PerfCategory = xmlElement.GetAttribute("PerfCat");
                int.TryParse(xmlElement.GetAttribute("TakeoffSpeed"), out performanceData.TakeoffSpeed);
                int.TryParse(xmlElement.GetAttribute("MaxSpeed"), out performanceData.MaxSpeed);
                int.TryParse(xmlElement.GetAttribute("MaxAlt"), out performanceData.MaxAltitude);
                bool.TryParse(xmlElement.GetAttribute("Mach1"), out performanceData.FasterThanMach);
                bool.TryParse(xmlElement.GetAttribute("IsJet"), out performanceData.IsJet);
                XmlNodeList elementsByTagName2 = xmlElement.GetElementsByTagName("Types");
                if (elementsByTagName2.Count > 0)
                {
                    string[] array = elementsByTagName2[0]!.InnerText.Split(new char[1] { ',' });
                    for (int j = 0; j < array.Length; j++)
                    {
                        string text = array[j].Trim();
                        AircraftTypeAndWake aircraftTypeAndWake = new AircraftTypeAndWake();
                        int num = text.IndexOf('/');
                        if (num != -1)
                        {
                            aircraftTypeAndWake.Type = text.Substring(0, num);
                            aircraftTypeAndWake.WakeCategory = text.Substring(num + 1);
                            performanceData.AircraftTypes.Add(aircraftTypeAndWake);
                        }
                    }
                }

                List<int> list = new List<int>();
                XmlNodeList elementsByTagName3 = xmlElement.GetElementsByTagName("Values");
                if (elementsByTagName3.Count > 0)
                {
                    string[] array = elementsByTagName3[0]!.Attributes!["Levels"]!.Value.Split(new char[1] { ',' });
                    for (int j = 0; j < array.Length; j++)
                    {
                        if (int.TryParse(array[j].Trim(), out var result))
                        {
                            list.Add(result);
                        }
                    }

                    XmlElement xmlElement2 = (XmlElement)elementsByTagName3[0];
                    List<short> list2 = new List<short>();
                    XmlNodeList elementsByTagName4 = xmlElement2.GetElementsByTagName("ClimbRates");
                    if (elementsByTagName4.Count > 0)
                    {
                        array = elementsByTagName4[0]!.InnerText.Split(new char[1] { ',' });
                        for (int j = 0; j < array.Length; j++)
                        {
                            if (short.TryParse(array[j].Trim(), out var result2))
                            {
                                list2.Add(result2);
                            }
                        }
                    }

                    List<short> list3 = new List<short>();
                    elementsByTagName4 = xmlElement2.GetElementsByTagName("DescentRates");
                    if (elementsByTagName4.Count > 0)
                    {
                        array = elementsByTagName4[0]!.InnerText.Split(new char[1] { ',' });
                        for (int j = 0; j < array.Length; j++)
                        {
                            if (short.TryParse(array[j].Trim(), out var result3))
                            {
                                list3.Add(result3);
                            }
                        }
                    }

                    List<SpeedData> list4 = new List<SpeedData>();
                    elementsByTagName4 = xmlElement2.GetElementsByTagName("CruiseSpeeds");
                    if (elementsByTagName4.Count > 0)
                    {
                        array = elementsByTagName4[0]!.InnerText.Split(new char[1] { ',' });
                        for (int j = 0; j < array.Length; j++)
                        {
                            string text2 = array[j].Trim();
                            if (text2.Length <= 2)
                            {
                                continue;
                            }

                            SpeedData speedData = new SpeedData();
                            if (text2[0] == 'M')
                            {
                                speedData.Type = SpeedData.SpeedTypes.MACH;
                            }
                            else
                            {
                                speedData.Type = SpeedData.SpeedTypes.IAS;
                            }

                            if (double.TryParse(text2.Substring(1), out speedData.Speed))
                            {
                                if (speedData.Type == SpeedData.SpeedTypes.MACH)
                                {
                                    speedData.Speed /= 100.0;
                                }

                                list4.Add(speedData);
                            }
                        }
                    }

                    List<SpeedData> list5 = new List<SpeedData>();
                    elementsByTagName4 = xmlElement2.GetElementsByTagName("ClimbSpeeds");
                    if (elementsByTagName4.Count > 0)
                    {
                        array = elementsByTagName4[0]!.InnerText.Split(new char[1] { ',' });
                        for (int j = 0; j < array.Length; j++)
                        {
                            string text3 = array[j].Trim();
                            if (text3.Length <= 2)
                            {
                                continue;
                            }

                            SpeedData speedData2 = new SpeedData();
                            if (text3[0] == 'M')
                            {
                                speedData2.Type = SpeedData.SpeedTypes.MACH;
                            }
                            else
                            {
                                speedData2.Type = SpeedData.SpeedTypes.IAS;
                            }

                            if (double.TryParse(text3.Substring(1), out speedData2.Speed))
                            {
                                if (speedData2.Type == SpeedData.SpeedTypes.MACH)
                                {
                                    speedData2.Speed /= 100.0;
                                }

                                list5.Add(speedData2);
                            }
                        }
                    }

                    List<SpeedData> list6 = new List<SpeedData>();
                    elementsByTagName4 = xmlElement2.GetElementsByTagName("DescentSpeeds");
                    if (elementsByTagName4.Count > 0)
                    {
                        array = elementsByTagName4[0]!.InnerText.Split(new char[1] { ',' });
                        for (int j = 0; j < array.Length; j++)
                        {
                            string text4 = array[j].Trim();
                            if (text4.Length <= 2)
                            {
                                continue;
                            }

                            SpeedData speedData3 = new SpeedData();
                            if (text4[0] == 'M')
                            {
                                speedData3.Type = SpeedData.SpeedTypes.MACH;
                            }
                            else
                            {
                                speedData3.Type = SpeedData.SpeedTypes.IAS;
                            }

                            if (double.TryParse(text4.Substring(1), out speedData3.Speed))
                            {
                                if (speedData3.Type == SpeedData.SpeedTypes.MACH)
                                {
                                    speedData3.Speed /= 100.0;
                                }

                                list6.Add(speedData3);
                            }
                        }
                    }

                    for (int k = 0; k < list.Count; k++)
                    {
                        PerformanceData.DataAtLevel dataAtLevel = new PerformanceData.DataAtLevel();
                        dataAtLevel.Altitude = list[k];
                        if (k < list2.Count && k < list3.Count && k < list4.Count && k < list5.Count && k < list6.Count)
                        {
                            dataAtLevel.ClimbRate = list2[k];
                            dataAtLevel.DescentRate = list3[k];
                            dataAtLevel.CruiseSpeed = list4[k];
                            dataAtLevel.ClimbSpeed = list5[k];
                            dataAtLevel.DescentSpeed = list6[k];
                            performanceData.AltitudeData.Add(dataAtLevel);
                        }
                    }
                }

                if (performanceData.AircraftTypes.Count > 0 && performanceData.AltitudeData.Count > 0)
                {
                    foreach (var type in performanceData.AircraftTypes)
                    {
                        var existingData = AircraftPerformanceData.FirstOrDefault(x => x.AircraftTypes.Any(x => x.Type == type.Type));
                        if (existingData == null) continue;
                        AircraftPerformanceData.Remove(existingData);
                    }

                    AircraftPerformanceData.Add(performanceData);
                }
            }

            Loaded = true;
        }

        public static PerformanceData GetPerformanceData(AircraftTypeAndWake type)
        {
            foreach (PerformanceData aircraftPerformanceDatum in AircraftPerformanceData)
            {
                if (aircraftPerformanceDatum.AircraftTypes.Any((AircraftTypeAndWake data) => data.Type == type.Type))
                {
                    return aircraftPerformanceDatum;
                }
            }

            return null;
        }

        public static PerformanceData GetPerformanceData(string type)
        {
            AircraftTypeAndWake atw = GetAircraftFromType(type);
            if (atw == null)
            {
                return null;
            }

            foreach (PerformanceData aircraftPerformanceDatum in AircraftPerformanceData)
            {
                if (aircraftPerformanceDatum.AircraftTypes.Any((AircraftTypeAndWake data) => data.Type == atw.Type))
                {
                    return aircraftPerformanceDatum;
                }
            }

            return null;
        }

        public static int CheckDataExists(AircraftTypeAndWake type)
        {
            for (int i = 0; i < AircraftPerformanceData.Count; i++)
            {
                if (AircraftPerformanceData[i].AircraftTypes.Any((AircraftTypeAndWake data) => data.Type == type.Type))
                {
                    return i;
                }
            }

            return -1;
        }

        public static int CheckDataExists(string type)
        {
            for (int i = 0; i < AircraftPerformanceData.Count; i++)
            {
                if (AircraftPerformanceData[i].AircraftTypes.Any((AircraftTypeAndWake data) => data.Type == type))
                {
                    return i;
                }
            }

            return -1;
        }

        public static AircraftTypeAndWake GetAircraftFromType(string type)
        {
            int num = CheckDataExists(type);
            if (num == -1)
            {
                return null;
            }

            return AircraftPerformanceData[num].GetAircraftTypeAndWake(type);
        }

        private static List<CalculatedProfileData> CalculateProfile(PerformanceData pd, bool descent)
        {
            List<CalculatedProfileData> list = new List<CalculatedProfileData>();
            double num = 0.0;
            CalculatedProfileData calculatedProfileData = new CalculatedProfileData();
            calculatedProfileData.Altitude = num;
            calculatedProfileData.Speed = new SpeedData(SpeedData.SpeedTypes.IAS, pd.TakeoffSpeed);
            list.Add(calculatedProfileData);
            for (int i = 0; i < pd.AltitudeData.Count; i++)
            {
                PerformanceData.DataAtLevel dataAtLevel = pd.AltitudeData[i];
                double num2 = (double)(dataAtLevel.Altitude * 100) - num;
                int num3 = ((!descent) ? dataAtLevel.ClimbRate : dataAtLevel.DescentRate);
                double num4 = num2 / (double)num3;
                for (int j = 1; (double)j < num4 * 2.0; j++)
                {
                    num += (double)num3 * 0.5;
                    CalculatedProfileData calculatedProfileData2 = new CalculatedProfileData();
                    calculatedProfileData2.Altitude = num;
                    if (descent)
                    {
                        calculatedProfileData2.Speed = dataAtLevel.DescentSpeed;
                    }
                    else
                    {
                        calculatedProfileData2.Speed = dataAtLevel.ClimbSpeed;
                    }

                    list.Add(calculatedProfileData2);
                }
            }

            if (descent)
            {
                list.Reverse();
            }

            return list;
        }
    }
}