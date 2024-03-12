﻿using SOS.Shared.Api.Dtos.Enum;

namespace SOS.Shared.Api.Dtos.Filter
{
    /// <summary>
    /// Generalization filter.
    /// </summary>
    public class GeneralizationFilterDto
    {
        /// <summary>
        /// Sensitive observations generalizations filter.
        /// </summary>
        public SensitiveGeneralizationFilterDto? SensitiveGeneralizationFilter { get; set; }

        /// <summary>
        /// Public observations generalizations filter.
        /// </summary>
        public PublicGeneralizationFilterDto? PublicGeneralizationFilter { get; set; }

        /// <summary>
        /// Try get real coordinate if the user has access.
        /// </summary>
        public bool? TryGetRealCoordinate { get; set; }
    }
}