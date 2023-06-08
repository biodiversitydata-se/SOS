using System.Runtime.Serialization;

namespace SOS.Observations.Api.Dtos.DataStewardship.Enums
{
    /// <summary>
    /// Age category or development stage of the observed organism.
    /// </summary>
    public enum DsLifeStage
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
}
