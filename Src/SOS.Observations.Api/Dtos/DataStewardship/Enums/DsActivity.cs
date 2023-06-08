using System.Runtime.Serialization;

namespace SOS.Observations.Api.Dtos.DataStewardship.Enums
{
    /// <summary>
    /// Activity or behavior of the observed organism.
    /// </summary>
    public enum DsActivity
    {
        /// <summary>
        /// använt bo
        /// </summary>
        [EnumMember(Value = "använt bo")]
        AnväntBo = 0,
        /// <summary>
        /// avledningsbeteende
        /// </summary>
        [EnumMember(Value = "avledningsbeteende")]
        AvledningsbeteendeEnum = 1,
        /// <summary>
        /// besöker bebott bo
        /// </summary>
        [EnumMember(Value = "besöker bebott bo")]
        BesökerBebottBo = 2,
        /// <summary>
        /// bo, hörda ungar
        /// </summary>
        [EnumMember(Value = "bo, hörda ungar")]
        BoHördaUngar = 3,
        /// <summary>
        /// bo, ägg/ungar
        /// </summary>
        [EnumMember(Value = "bo, ägg/ungar")]
        BoÄggUngar = 4,
        /// <summary>
        /// bobesök?
        /// </summary>
        [EnumMember(Value = "bobesök?")]
        Bobesök = 5,
        /// <summary>
        /// bobygge
        /// </summary>
        [EnumMember(Value = "bobygge")]
        Bobygge = 6,
        /// <summary>
        ///  DrktigHona for dräktig hona
        /// </summary>
        [EnumMember(Value = "dräktig hona")]
        DräktigHona = 7,
        /// <summary>
        ///  Dd for död
        /// </summary>
        [EnumMember(Value = "död")]
        Död = 8,
        /// <summary>
        ///  Fragment for fragment
        /// </summary>
        [EnumMember(Value = "fragment")]
        Fragment = 9,
        /// <summary>
        ///  Friflygande for friflygande
        /// </summary>
        [EnumMember(Value = "friflygande")]
        Friflygande = 10,
        /// <summary>
        ///  Frispringandekrypande for frispringande/krypande
        /// </summary>
        [EnumMember(Value = "frispringande/krypande")]
        FrispringandeKrypande = 11,
        /// <summary>
        ///  FrskaGnagspr for färska gnagspår
        /// </summary>
        [EnumMember(Value = "färska gnagspår")]
        FärskaGnagspår = 12,
        /// <summary>
        ///  FdaTUngar for föda åt ungar
        /// </summary>
        [EnumMember(Value = "föda åt ungar")]
        FödaÅtUngar = 13,
        /// <summary>
        ///  Fdoskande for födosökande
        /// </summary>
        [EnumMember(Value = "födosökande")]
        Födosökande = 14,
        /// <summary>
        ///  Frbiflygande for förbiflygande
        /// </summary>
        [EnumMember(Value = "förbiflygande")]
        Förbiflygande = 15,
        /// <summary>
        ///  IVattensimmande for i vatten/simmande
        /// </summary>
        [EnumMember(Value = "i vatten/simmande")]
        IVattenSimmande = 16,
        /// <summary>
        ///  IngetKriteriumAngivet for inget kriterium angivet
        /// </summary>
        [EnumMember(Value = "inget kriterium angivet")]
        IngetKriteriumAngivet = 17,
        /// <summary>
        ///  LocklteVrigaLten for lockläte, övriga läten
        /// </summary>
        [EnumMember(Value = "lockläte, övriga läten")]
        LockläteÖvrigaLäten = 18,
        /// <summary>
        ///  MisslyckadHckning for misslyckad häckning
        /// </summary>
        [EnumMember(Value = "misslyckad häckning")]
        MisslyckadHäckning = 19,
        /// <summary>
        ///  ObsIHcktidLmpligBiotop for obs i häcktid, lämplig biotop
        /// </summary>
        [EnumMember(Value = "obs i häcktid, lämplig biotop")]
        ObsIHäcktidLämpligBiotop = 20,
        /// <summary>
        ///  ParILmpligHckbiotop for par i lämplig häckbiotop
        /// </summary>
        [EnumMember(Value = "par i lämplig häckbiotop")]
        ParILämpligHäckbiotop = 21,
        /// <summary>
        ///  Parning for parning
        /// </summary>
        [EnumMember(Value = "parning")]
        Parning = 22,
        /// <summary>
        ///  PermanentRevir for permanent revir
        /// </summary>
        [EnumMember(Value = "permanent revir")]
        PermanentRevir = 23,
        /// <summary>
        ///  PullinyligenFlyggaUngar for pulli/nyligen flygga ungar
        /// </summary>
        [EnumMember(Value = "pulli/nyligen flygga ungar")]
        PulliNyligenFlyggaUngar = 24,
        /// <summary>
        ///  PVervintringsplats for på övervintringsplats
        /// </summary>
        [EnumMember(Value = "på övervintringsplats")]
        PåÖvervintringsplats = 25,
        /// <summary>
        ///  Rastande for rastande
        /// </summary>
        [EnumMember(Value = "rastande")]
        Rastande = 26,
        /// <summary>
        ///  Revirhvdande for revirhävdande
        /// </summary>
        [EnumMember(Value = "revirhävdande")]
        Revirhävdande = 27,
        /// <summary>
        ///  Ruvande for ruvande
        /// </summary>
        [EnumMember(Value = "ruvande")]
        Ruvande = 28,
        /// <summary>
        ///  SannoliktEjHckande for sannolikt ej häckande
        /// </summary>
        [EnumMember(Value = "sannolikt ej häckande")]
        SannoliktEjHäckande = 29,
        /// <summary>
        ///  Spelsng for spel/sång
        /// </summary>
        [EnumMember(Value = "spel/sång")]
        SpelSång = 30,
        /// <summary>
        ///  SpelsngEjHckning for spel/sång, ej häckning
        /// </summary>
        [EnumMember(Value = "spel/sång, ej häckning")]
        SpelSångEjHäckning = 31,
        /// <summary>
        ///  Stationr for stationär
        /// </summary>
        [EnumMember(Value = "stationär")]
        Stationär = 32,
        /// <summary>
        ///  Strckande for sträckande
        /// </summary>
        [EnumMember(Value = "sträckande")]
        Sträckande = 33,
        /// <summary>
        ///  UpprrdVarnande for upprörd, varnande
        /// </summary>
        [EnumMember(Value = "upprörd, varnande")]
        UpprördVarnande = 34,
        /// <summary>
        ///  Vilande for vilande
        /// </summary>
        [EnumMember(Value = "vilande")]
        Vilande = 35,
        /// <summary>
        ///  Gglggande for äggläggande
        /// </summary>
        [EnumMember(Value = "äggläggande")]
        Äggläggande = 36,
        /// <summary>
        ///  Ggskal for äggskal
        /// </summary>
        [EnumMember(Value = "äggskal")]
        Äggskal = 37,
        /// <summary>
        ///  LdreGnagspr for äldre gnagspår
        /// </summary>
        [EnumMember(Value = "äldre gnagspår")]
        ÄldreGnagspår = 38,
        /// <summary>
        /// bobygge
        /// </summary>
        [EnumMember(Value = "exkrementsäck")]
        Exkrementsäck = 39,
    }
}

