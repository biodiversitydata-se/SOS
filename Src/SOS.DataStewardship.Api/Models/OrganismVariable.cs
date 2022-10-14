using System;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models
{ 
    /// <summary>
    /// Observed attributes and other variables linked to a certain individual or uniform group of individuals, e.g. sex, age, behavior.
    /// </summary>
    [DataContract]
    public class OrganismVariable
    { 
        /// <summary>
        /// Sex of the observed organism.
        /// </summary>
        public enum SexEnum
        {
            /// <summary>
            /// hane
            /// </summary>
            [EnumMember(Value = "hane")]
            Hane = 0,
            /// <summary>
            /// hona
            /// </summary>
            [EnumMember(Value = "hona")]
            Hona = 1,
            /// <summary>
            /// i par
            /// </summary>
            [EnumMember(Value = "i par")]
            IPar = 2
        }

        /// <summary>
        /// Sex of the observed organism.
        /// </summary>
        [DataMember(Name="sex")]
        public SexEnum? Sex { get; set; }

        /// <summary>
        /// Age category or development stage of the observed organism.
        /// </summary>
        public enum LifeStageEnum
        {
            /// <summary>
            ///  _1K for 1K
            /// </summary>
            [EnumMember(Value = "1K")]
            _1K = 0,
            /// <summary>
            ///  _1K_1 for 1K+
            /// </summary>
            [EnumMember(Value = "1K+")]
            _1KPlus = 1,
            /// <summary>
            ///  _2K for 2K
            /// </summary>
            [EnumMember(Value = "2K")]
            _2K = 2,
            /// <summary>
            ///  _2K_3 for 2K+
            /// </summary>
            [EnumMember(Value = "2K+")]
            _2KPlus = 3,
            /// <summary>
            ///  _3K for 3K
            /// </summary>
            [EnumMember(Value = "3K")]
            _3K = 4,
            /// <summary>
            ///  _3K_5 for 3K+
            /// </summary>
            [EnumMember(Value = "3K+")]
            _3KPlus = 5,
            /// <summary>
            ///  _3K_6 for 3K-
            /// </summary>
            [EnumMember(Value = "3K-")]
            _3KMinus = 6,
            /// <summary>
            ///  _4K for 4K
            /// </summary>
            [EnumMember(Value = "4K")]
            _4K = 7,
            /// <summary>
            ///  _4K_8 for 4K+
            /// </summary>
            [EnumMember(Value = "4K+")]
            _4KPlus = 8,
            /// <summary>
            ///  _4K_9 for 4K-
            /// </summary>
            [EnumMember(Value = "4K-")]
            _4KMinus = 9,
            /// <summary>
            ///  Adult for adult
            /// </summary>
            [EnumMember(Value = "adult")]
            Adult = 10,
            /// <summary>
            ///  Blomning for blomning
            /// </summary>
            [EnumMember(Value = "blomning")]
            Blomning = 11,
            /// <summary>
            ///  EjBestmdLder for ej bestämd ålder
            /// </summary>
            [EnumMember(Value = "ej bestämd ålder")]
            EjBestämdÅlder = 12,
            /// <summary>
            ///  FulltUtveckladeBlad for fullt utvecklade blad
            /// </summary>
            [EnumMember(Value = "fullt utvecklade blad")]
            FulltUtveckladeBlad = 13,
            /// <summary>
            ///  GulnadeLvblad for gulnade löv/blad
            /// </summary>
            [EnumMember(Value = "gulnade löv/blad")]
            GulnadeLövBlad = 14,
            /// <summary>
            ///  Imagoadult for imago/adult
            /// </summary>
            [EnumMember(Value = "imago/adult")]
            ImagoAdult = 15,
            /// <summary>
            ///  Juvenil for juvenil
            /// </summary>
            [EnumMember(Value = "juvenil")]
            Juvenil = 16,
            /// <summary>
            ///  Knoppbristning for knoppbristning
            /// </summary>
            [EnumMember(Value = "knoppbristning")]
            Knoppbristning = 17,
            /// <summary>
            ///  Larv for larv
            /// </summary>
            [EnumMember(Value = "larv")]
            Larv = 18,
            /// <summary>
            ///  Larvnymf for larv/nymf
            /// </summary>
            [EnumMember(Value = "larv/nymf")]
            LarvNymf = 19,
            /// <summary>
            ///  MedGroddkorn for med groddkorn
            /// </summary>
            [EnumMember(Value = "med groddkorn")]
            MedGroddkorn = 20,
            /// <summary>
            ///  MedHonorgan for med honorgan
            /// </summary>
            [EnumMember(Value = "med honorgan")]
            MedHonorgan = 21,
            /// <summary>
            ///  MedKapsel for med kapsel
            /// </summary>
            [EnumMember(Value = "med kapsel")]
            MedKapsel = 22,
            /// <summary>
            ///  Pulli for pulli
            /// </summary>
            [EnumMember(Value = "pulli")]
            Pulli = 23,
            /// <summary>
            ///  Puppa for puppa
            /// </summary>
            [EnumMember(Value = "puppa")]
            Puppa = 24,
            /// <summary>
            ///  Subadult for subadult
            /// </summary>
            [EnumMember(Value = "subadult")]
            Subadult = 25,
            /// <summary>
            ///  UtanKapsel for utan kapsel
            /// </summary>
            [EnumMember(Value = "utan kapsel")]
            UtanKapsel = 26,
            /// <summary>
            ///  Vilstadium for vilstadium
            /// </summary>
            [EnumMember(Value = "vilstadium")]
            Vilstadium = 27,
            /// <summary>
            ///  Gg for ägg
            /// </summary>
            [EnumMember(Value = "ägg")]
            Ägg = 28,
            /// <summary>
            ///  Rsunge for årsunge
            /// </summary>
            [EnumMember(Value = "årsunge")]
            Årsunge = 29,
            /// <summary>
            ///  Rsyngel for årsyngel
            /// </summary>
            [EnumMember(Value = "årsyngel")]
            Årsyngel = 30,
            /// <summary>
            ///  Verblommad for överblommad
            /// </summary>
            [EnumMember(Value = "överblommad")]
            Överblommad = 31
        }

        /// <summary>
        /// Age category or development stage of the observed organism.
        /// </summary>
        [DataMember(Name="lifeStage")]
        public LifeStageEnum? LifeStage { get; set; }

        /// <summary>
        /// Activity or behavior of the observed organism.
        /// </summary>
        public enum ActivityEnum
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

        /// <summary>
        /// Activity or behavior of the observed organism.
        /// </summary>
        [DataMember(Name="activity")]
        public ActivityEnum? Activity { get; set; }
    }
}
