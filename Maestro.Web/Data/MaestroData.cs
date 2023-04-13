namespace Maestro.Web.Data
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class MaestroData
    {

        private MAESTROAirport[] airportField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Airport")]
        public MAESTROAirport[] Airport
        {
            get
            {
                return this.airportField;
            }
            set
            {
                this.airportField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class MAESTROAirport
    {

        private MAESTROAirportRunway[] runwaysField;

        private MAESTROAirportMode[] runwayModesField;

        private MAESTROAirportSector[] sectorsField;

        private MAESTROAirportFix[] fixRunwayRulesField;

        private string iCAOField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Runway", IsNullable = false)]
        public MAESTROAirportRunway[] Runways
        {
            get
            {
                return this.runwaysField;
            }
            set
            {
                this.runwaysField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Mode", IsNullable = false)]
        public MAESTROAirportMode[] RunwayModes
        {
            get
            {
                return this.runwayModesField;
            }
            set
            {
                this.runwayModesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Sector", IsNullable = false)]
        public MAESTROAirportSector[] Sectors
        {
            get
            {
                return this.sectorsField;
            }
            set
            {
                this.sectorsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Fix", IsNullable = false)]
        public MAESTROAirportFix[] FixRunwayRules
        {
            get
            {
                return this.fixRunwayRulesField;
            }
            set
            {
                this.fixRunwayRulesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ICAO
        {
            get
            {
                return this.iCAOField;
            }
            set
            {
                this.iCAOField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class MAESTROAirportRunway
    {

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class MAESTROAirportMode
    {

        private string nameField;

        private string runwayRateField;

        private string dependenciesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RunwayRate
        {
            get
            {
                return this.runwayRateField;
            }
            set
            {
                this.runwayRateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Dependencies
        {
            get
            {
                return this.dependenciesField;
            }
            set
            {
                this.dependenciesField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class MAESTROAirportSector
    {

        private string nameField;

        private string fixesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Fixes
        {
            get
            {
                return this.fixesField;
            }
            set
            {
                this.fixesField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class MAESTROAirportFix
    {

        private string nameField;

        private string starNameField;

        private string airwayField;

        private string typeField;

        private string preferredRunwayField;

        private string distanceToRunwayField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StarName
        {
            get
            {
                return this.starNameField;
            }
            set
            {
                this.starNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Airway
        {
            get
            {
                return this.airwayField;
            }
            set
            {
                this.airwayField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PreferredRunway
        {
            get
            {
                return this.preferredRunwayField;
            }
            set
            {
                this.preferredRunwayField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DistanceToRunway
        {
            get
            {
                return this.distanceToRunwayField;
            }
            set
            {
                this.distanceToRunwayField = value;
            }
        }
    }


}
