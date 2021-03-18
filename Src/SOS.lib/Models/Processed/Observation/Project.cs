using System;
using System.Collections.Generic;
using Nest;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     Artportalen project information.
    /// </summary>
    public class Project
    {
        /// <summary>
        ///     Indicates if species observations that are reported in
        ///     a project are publicly available or not.
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        ///     Information about the type of project,
        ///     for example 'Environmental monitoring'.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        ///     Information about the type of project in Swedish,
        ///     for example 'Miljöövervakning'.
        /// </summary>
        public string CategorySwedish { get; set; }

        /// <summary>
        ///     Description of a project.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Date when the project ends.
        /// </summary>
        [Date]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// An identifier for the project.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Name of the project.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Name of person or organization that owns the project.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        ///     Date when the project starts.
        /// </summary>
        [Date]
        public DateTime? StartDate { get; set; }

        /// <summary>
        ///     Web address that leads to more information about the
        ///     project. The information should be accessible
        ///     from the most commonly used web browsers.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string ProjectURL { get; set; }

        /// <summary>
        ///     Survey method used in a project to
        ///     retrieve species observations.
        /// </summary>
        public string SurveyMethod { get; set; }

        /// <summary>
        ///     Survey method URL.
        /// </summary>
        public string SurveyMethodUrl { get; set; }

        /// <summary>
        ///     Project parameters.
        /// </summary>
        [Nested]
        public IEnumerable<ProjectParameter> ProjectParameters { get; set; }
    }
}