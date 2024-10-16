﻿namespace SOS.Shared.Api.Dtos.Enum
{
    public enum ProtectionFilterDto
    {
        /// <summary>
        /// Public observations
        /// </summary>
        Public = 0,

        /// <summary>
        /// Sensitive observations
        /// </summary>
        Sensitive,

        /// <summary>
        /// Both public and sensitive observations
        /// </summary>
        BothPublicAndSensitive
    }
}
