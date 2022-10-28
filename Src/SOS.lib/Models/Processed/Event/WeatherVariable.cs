using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SOS.Lib.Models.Processed.Event
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class WeatherVariable
    {
        /// <summary>
        /// States the snow conditions on the ground during the survey event.
        /// </summary>
        public enum SnowCoverEnum
        {
            /// <summary>
            /// barmark
            /// </summary>
            [EnumMember(Value = "barmark")]
            Barmark = 0,
            /// <summary>
            /// snötäckt mark
            /// </summary>
            [EnumMember(Value = "snötäckt mark")]
            SnötäcktMark = 1,
            /// <summary>
            /// mycket tunt snötäcke eller fläckvis snötäcke
            /// </summary>
            [EnumMember(Value = "mycket tunt snötäcke eller fläckvis snötäcke")]
            MycketTuntSnötäckeEllerFläckvisSnötäcke = 2
        }

        /// <summary>
        /// States the snow conditions on the ground during the survey event.
        /// </summary>
        [DataMember(Name = "snowCover")]
        public SnowCoverEnum? SnowCover { get; set; }

        /// <summary>
        /// States the amount of sunshine during the survey event.
        /// </summary>
        [DataMember(Name = "sunshine")]
        public WeatherMeasuring Sunshine { get; set; }

        /// <summary>
        /// States the air temperature during the survey event.
        /// </summary>
        [DataMember(Name = "airTemperature")]
        public WeatherMeasuring AirTemperature { get; set; }

        /// <summary>
        /// States the wind direction during the survey event as a compass direction.
        /// </summary>
        public enum WindDirectionCompassEnum
        {
            /// <summary>
            /// nord
            /// </summary>
            [EnumMember(Value = "nord")]
            Nord = 0,
            /// <summary>
            /// nordost
            /// </summary>
            [EnumMember(Value = "nordost")]
            Nordost = 1,
            /// <summary>
            /// nordväst
            /// </summary>
            [EnumMember(Value = "nordväst")]
            Nordväst = 2,
            /// <summary>
            /// ost
            /// </summary>
            [EnumMember(Value = "ost")]
            Ost = 3,
            /// <summary>
            /// syd
            /// </summary>
            [EnumMember(Value = "syd")]
            Syd = 4,
            /// <summary>
            /// sydost
            /// </summary>
            [EnumMember(Value = "sydost")]
            Sydost = 5,
            /// <summary>
            /// sydväst
            /// </summary>
            [EnumMember(Value = "sydväst")]
            Sydväst = 6,
            /// <summary>
            /// väst
            /// </summary>
            [EnumMember(Value = "väst")]
            Väst = 7
        }

        /// <summary>
        /// States the wind direction during the survey event as a compass direction.
        /// </summary>
        [DataMember(Name = "windDirectionCompass")]
        public WindDirectionCompassEnum? WindDirectionCompass { get; set; }

        /// <summary>
        /// States the wind direction during the survey event as a number of degrees.
        /// </summary>
        [DataMember(Name = "windDirectionDegrees")]
        public WeatherMeasuring WindDirectionDegrees { get; set; }

        /// <summary>
        /// WindSpeed
        /// </summary>
        [DataMember(Name = "windSpeed")]
        public WeatherMeasuring WindSpeed { get; set; }

        /// <summary>
        /// States the strength of the wind during the survey event.
        /// </summary>
        public enum WindStrengthEnum
        {
            /// <summary>
            ///  vindstilla, <1 m/s
            /// </summary>
            [EnumMember(Value = "vindstilla, <1 m/s")]
            Vindstilla1Ms = 0,
            /// <summary>
            ///  svag vind, 1-3 m/s
            /// </summary>
            [EnumMember(Value = "svag vind, 1-3 m/s")]
            SvagVind1Till3Ms = 1,
            /// <summary>
            ///  måttlig vind, 4-7 m/s
            /// </summary>
            [EnumMember(Value = "måttlig vind, 4-7 m/s")]
            MttligVind4Till7Ms = 2,
            /// <summary>
            ///  frisk vind, 8-13 m/s
            /// </summary>
            [EnumMember(Value = "frisk vind, 8-13 m/s")]
            FriskVind8Till13Ms = 3,
            /// <summary>
            ///  hård vind, 14-19 m/s
            /// </summary>
            [EnumMember(Value = "hård vind, 14-19 m/s")]
            HårdVind14Till19Ms = 4,
            /// <summary>
            ///  mycket hård vind, 20-24 m/s
            /// </summary>
            [EnumMember(Value = "mycket hård vind, 20-24 m/s")]
            MycketHrdVind20Till24Ms = 5,
            /// <summary>
            ///  storm, 25-32 m/s
            /// </summary>
            [EnumMember(Value = "storm, 25-32 m/s")]
            Storm25Till32Ms = 6,
            /// <summary>
            ///  orkan, ≥33 m/s
            /// </summary>
            [EnumMember(Value = "orkan, ≥33 m/s")]
            Orkan33Ms = 7,
            /// <summary>
            ///  0 Beaufort
            /// </summary>
            [EnumMember(Value = "0 Beaufort")]
            _0Beaufort = 8,
            /// <summary>
            ///  1 Beaufort
            /// </summary>
            [EnumMember(Value = "1 Beaufort")]
            _1Beaufort = 9,
            /// <summary>
            ///  2 Beaufort
            /// </summary>
            [EnumMember(Value = "2 Beaufort")]
            _2Beaufort = 10,
            /// <summary>
            ///  3 Beaufort
            /// </summary>
            [EnumMember(Value = "3 Beaufort")]
            _3Beaufort = 11,
            /// <summary>
            ///  4 Beaufort
            /// </summary>
            [EnumMember(Value = "4 Beaufort")]
            _4Beaufort = 12,
            /// <summary>
            ///  5 Beaufort
            /// </summary>
            [EnumMember(Value = "5 Beaufort")]
            _5Beaufort = 13,
            /// <summary>
            ///  6 Beaufort
            /// </summary>
            [EnumMember(Value = "6 Beaufort")]
            _6Beaufort = 14,
            /// <summary>
            ///  7 Beaufort
            /// </summary>
            [EnumMember(Value = "7 Beaufort")]
            _7Beaufort = 15,
            /// <summary>
            ///  8 Beaufort
            /// </summary>
            [EnumMember(Value = "8 Beaufort")]
            _8Beaufort = 16,
            /// <summary>
            ///  9 Beaufort
            /// </summary>
            [EnumMember(Value = "9 Beaufort")]
            _9Beaufort = 17,
            /// <summary>
            ///  10 Beaufort
            /// </summary>
            [EnumMember(Value = "10 Beaufort")]
            _10Beaufort = 18
        }

        /// <summary>
        /// States the strength of the wind during the survey event.
        /// </summary>
        [DataMember(Name = "windStrength")]
        public WindStrengthEnum? WindStrength { get; set; }

        /// <summary>
        /// States the precipitation conditions during the survey event.
        /// </summary>
        public enum PrecipitationEnum
        {
            /// <summary>
            ///  kraftigt regn
            /// </summary>
            [EnumMember(Value = "kraftigt regn")]
            KraftigtRegn = 0,
            /// <summary>
            ///  kraftigt snöfall
            /// </summary>
            [EnumMember(Value = "kraftigt snöfall")]
            KraftigtSnöfall = 1,
            /// <summary>
            ///  lätt regn
            /// </summary>
            [EnumMember(Value = "lätt regn")]
            LättRegn = 2,
            /// <summary>
            ///  lätt snöfall
            /// </summary>
            [EnumMember(Value = "lätt snöfall")]
            LättSnöfall = 3,
            /// <summary>
            ///  måttligt regn
            /// </summary>
            [EnumMember(Value = "måttligt regn")]
            MåttligtRegn = 4,
            /// <summary>
            ///  måttligt snöfall
            /// </summary>
            [EnumMember(Value = "måttligt snöfall")]
            MåttligtSnöfall = 5,
            /// <summary>
            ///  regnskurar
            /// </summary>
            [EnumMember(Value = "regnskurar")]
            Regnskurar = 6,
            /// <summary>
            ///  uppehåll
            /// </summary>
            [EnumMember(Value = "uppehåll")]
            Uppehåll = 7
        }

        /// <summary>
        /// States the precipitation conditions during the survey event.
        /// </summary>
        [DataMember(Name = "precipitation")]
        public PrecipitationEnum? Precipitation { get; set; }

        /// <summary>
        /// States the visibility conditions during the survey event.
        /// </summary>
        public enum VisibilityEnum
        {
            /// <summary>
            /// dimma, <1 km
            /// </summary>
            [EnumMember(Value = "dimma, <1 km")]
            Dimma1Km = 0,
            /// <summary>
            /// dis, 1-4 km
            /// </summary>
            [EnumMember(Value = "dis, 1-4 km")]
            Dis1Till4Km = 1,
            /// <summary>
            /// god, 10-20 km
            /// </summary>
            [EnumMember(Value = "god, 10-20 km")]
            God10Till20Km = 2,
            /// <summary>
            /// mycket god, >20 km
            /// </summary>
            [EnumMember(Value = "mycket god, >20 km")]
            MycketGod20Km = 3,
            /// <summary>
            /// måttlig, 4-10 km
            /// </summary>
            [EnumMember(Value = "måttlig, 4-10 km")]
            Måttlig4Till10Km = 4
        }

        /// <summary>
        /// States the visibility conditions during the survey event.
        /// </summary>
        [DataMember(Name = "visibility")]
        public VisibilityEnum? Visibility { get; set; }

        /// <summary>
        /// States the cloud condtions during the survey event.
        /// </summary>
        public enum CloudinessEnum
        {
            /// <summary>
            /// halvklart, 3-5/8
            /// </summary>
            [EnumMember(Value = "halvklart, 3-5/8")]
            Halvklart3Till5Av8 = 0,
            /// <summary>
            /// klart, 0/8
            /// </summary>
            [EnumMember(Value = "klart, 0/8")]
            Klart0Av8 = 1,
            /// <summary>
            /// molnigt, 6-7/8
            /// </summary>
            [EnumMember(Value = "molnigt, 6-7/8")]
            Molnigt6Till7Av8 = 2,
            /// <summary>
            /// mulet, 8/8
            /// </summary>
            [EnumMember(Value = "mulet, 8/8")]
            Mulet8Av8 = 3,
            /// <summary>
            /// nästan klart, 1-2/8
            /// </summary>
            [EnumMember(Value = "nästan klart, 1-2/8")]
            NästanKlart1Till2Av8 = 4,
            /// <summary>
            /// växlande, 0-8/8
            /// </summary>
            [EnumMember(Value = "växlande, 0-8/8")]
            Växlande0Till8Av8 = 5
        }

        /// <summary>
        /// States the cloud condtions during the survey event.
        /// </summary>
        [DataMember(Name = "cloudiness")]
        public CloudinessEnum? Cloudiness { get; set; }

        /// <summary>
        /// Weather variable reported as a measurement and a unit.
        /// </summary>
        [DataContract]
        public class WeatherMeasuring
        {
            /// <summary>
            /// Value for measured weather variable.
            /// </summary>
            [Required]
            [DataMember(Name = "weatherMeasure")]
            public decimal? WeatherMeasure { get; set; }

            /// <summary>
            /// Unit for a reported measurement (given in the attribute \"vädermått\").
            /// </summary>
            public enum UnitEnum
            {
                /// <summary>
                /// %
                /// </summary>
                [EnumMember(Value = "%")]
                Percent = 0,
                /// <summary>
                /// cm²
                /// </summary>
                [EnumMember(Value = "cm²")]
                Cm2 = 1,
                /// <summary>
                /// cm³
                /// </summary>
                [EnumMember(Value = "cm³")]
                Cm3 = 2,
                /// <summary>
                /// dm²
                /// </summary>
                [EnumMember(Value = "dm²")]
                Dm2 = 3,
                /// <summary>
                /// kompassgrader
                /// </summary>
                [EnumMember(Value = "kompassgrader")]
                Kompassgrader = 4,
                /// <summary>
                /// m/s
                /// </summary>
                [EnumMember(Value = "m/s")]
                Ms = 5,
                /// <summary>
                /// m²
                /// </summary>
                [EnumMember(Value = "m²")]
                M2 = 6,
                /// <summary>
                /// styck
                /// </summary>
                [EnumMember(Value = "styck")]
                Styck = 7,
                /// <summary>
                /// °C
                /// </summary>
                [EnumMember(Value = "°C")]
                GraderCelsius = 8
            }

            /// <summary>
            /// Unit for a reported measurement (given in the attribute "vädermått").
            /// </summary>
            [Required]
            [DataMember(Name = "unit")]
            public UnitEnum? Unit { get; set; }
        }

    }
}