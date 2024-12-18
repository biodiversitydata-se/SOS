﻿using System.Runtime.Serialization;

namespace SOS.Shared.Api.Dtos.DataStewardship.Enums
{
    /// <summary>
    /// Information about whether the whole, parts of, or nothing in the dataset is publically available.
    /// </summary>
    public enum DsAccessRights
    {
        /// <summary>
        /// publik
        /// </summary>
        [EnumMember(Value = "publik")]
        Publik = 0,
        /// <summary>
        /// begränsad
        /// </summary>
        [EnumMember(Value = "begränsad")]
        Begränsad = 1,
        /// <summary>
        /// ej offentlig
        /// </summary>
        [EnumMember(Value = "ej offentlig")]
        EjOffentlig = 2
    }
}
