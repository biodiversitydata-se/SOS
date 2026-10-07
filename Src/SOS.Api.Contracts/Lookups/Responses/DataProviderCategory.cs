namespace SOS.Api.Contracts.Lookups.Responses;
/// <summary>Categories describing a data provider's role, data sources or organism groups.</summary>
public enum DataProviderCategory
{

    /// <summary>Data-host providers.</summary>
    DataHostesship = 0,

    /// <summary>National inventory providers.</summary>
    NationalInventory = 1,

    /// <summary>Regional inventory providers.</summary>
    RegionalInventory = 2,

    /// <summary>Museum collections.</summary>
    MuseumCollections = 3,

    /// <summary>Herbarium collections.</summary>
    HerbariaCollections = 4,

    /// <summary>Citizen-science platforms.</summary>
    CitizenSciencePlatform = 5,

    /// <summary>Atlas surveys.</summary>
    Atlas = 6,

    /// <summary>Terrestrial observations.</summary>
    Terrestrial = 7,

    /// <summary>Freshwater observations.</summary>
    Freshwater = 8,

    /// <summary>Marine observations.</summary>
    Marine = 9,

    /// <summary>Vertebrate observations.</summary>
    Vertebrates = 10,

    /// <summary>Invertebrate observations.</summary>
    Invertebrates = 11,

    /// <summary>Arthropod observations.</summary>
    Arthropods = 12,

    /// <summary>Microorganism observations.</summary>
    Microorganisms = 13,

    /// <summary>Plant, bryophyte and lichen observations.</summary>
    Plants_Bryophytes_Lichens = 14,

    /// <summary>Fungal observations.</summary>
    Fungi = 15,

    /// <summary>Algal observations.</summary>
    Algae = 16,

    /// <summary>Data-stewardship providers.</summary>
    DataStewardship = 17,

}


