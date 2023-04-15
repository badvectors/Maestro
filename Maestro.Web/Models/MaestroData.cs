namespace Maestro.Web.Models
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
    public partial class MaestroData
    {

        private MAESTROAirport[] airportField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Airport")]
        public MAESTROAirport[] Airport
        {
            get
            {
                return airportField;
            }
            set
            {
                airportField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    public partial class MAESTROAirport
    {

        private MAESTROAirportRunway[] runwaysField;

        private MAESTROAirportMode[] runwayModesField;

        private MAESTROAirportSector[] sectorsField;

        private MAESTROAirportFix[] fixRunwayRulesField;

        private string iCAOField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("Runway", IsNullable = false)]
        public MAESTROAirportRunway[] Runways
        {
            get
            {
                return runwaysField;
            }
            set
            {
                runwaysField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("Mode", IsNullable = false)]
        public MAESTROAirportMode[] RunwayModes
        {
            get
            {
                return runwayModesField;
            }
            set
            {
                runwayModesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("Sector", IsNullable = false)]
        public MAESTROAirportSector[] Sectors
        {
            get
            {
                return sectorsField;
            }
            set
            {
                sectorsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("Fix", IsNullable = false)]
        public MAESTROAirportFix[] FixRunwayRules
        {
            get
            {
                return fixRunwayRulesField;
            }
            set
            {
                fixRunwayRulesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string ICAO
        {
            get
            {
                return iCAOField;
            }
            set
            {
                iCAOField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    public partial class MAESTROAirportRunway
    {

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    public partial class MAESTROAirportMode
    {

        private string nameField;

        private string runwayRateField;

        private string dependenciesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string RunwayRate
        {
            get
            {
                return runwayRateField;
            }
            set
            {
                runwayRateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string Dependencies
        {
            get
            {
                return dependenciesField;
            }
            set
            {
                dependenciesField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    public partial class MAESTROAirportSector
    {

        private string nameField;

        private string fixesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string Fixes
        {
            get
            {
                return fixesField;
            }
            set
            {
                fixesField = value;
            }
        }
    }

    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    public partial class MAESTROAirportFix
    {

        private string nameField;

        private string starNameField;

        private string airwayField;

        private string typeField;

        private string preferredRunwayField;

        private string distanceToRunwayField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string StarName
        {
            get
            {
                return starNameField;
            }
            set
            {
                starNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string Airway
        {
            get
            {
                return airwayField;
            }
            set
            {
                airwayField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string Type
        {
            get
            {
                return typeField;
            }
            set
            {
                typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string PreferredRunway
        {
            get
            {
                return preferredRunwayField;
            }
            set
            {
                preferredRunwayField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string DistanceToRunway
        {
            get
            {
                return distanceToRunwayField;
            }
            set
            {
                distanceToRunwayField = value;
            }
        }
    }


}
