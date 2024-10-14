using System;

namespace SOS.Lib.Models.Verbatim.Artportalen
{
    public class DiaryEntry
    {
        /// <summary>
        /// Id of cloudiness
        /// </summary>
        public int? CloudinessId { get; set; }

        /// <summary>
        /// Controling organisation id 
        /// </summary>
        public int? ControlingOrganisationId { get; set; }

        /// <summary>
        /// End time
        /// </summary>
        public TimeSpan? EndTime { get; set; }

        /// <summary>
		/// Issue date
		/// </summary>
		public DateTime IssueDate { get; set; }

        /// <summary>
        /// Organization id
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// Precipitation id
        /// </summary>
        public int? PrecipitationId { get; set; }

        /// <summary>
        /// Project id
        /// </summary>
		public int ProjectId { get; set; }

        /// <summary>
        /// Site id
        /// </summary>
		public int? SiteId { get; set; }

        /// <summary>
        /// Snow cover id
        /// </summary>
		public int? SnowcoverId { get; set; }

        /// <summary>
        /// Start time
        /// </summary>
		public TimeSpan? StartTime { get; set; }

        /// <summary>
        /// Temperature
        /// </summary>
		public int? Temperature { get; set; }

        /// <summary>
        /// User id
        /// </summary>
		public int UserId { get; set; }

        /// <summary>
        /// Visibility id
        /// </summary>
		public int? VisibilityId { get; set; }

        /// <summary>
        /// Wind id
        /// </summary>
		public int? WindId { get; set; }

        /// <summary>
        /// Wind strength id
        /// </summary>
		public int? WindStrengthId { get; set; }
    }
}
