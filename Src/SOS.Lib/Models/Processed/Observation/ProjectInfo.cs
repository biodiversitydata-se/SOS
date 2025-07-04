﻿using SOS.Lib.Models.Interfaces;
using System;
using System.Collections.Generic;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     Project info.
    /// </summary>
    public class ProjectInfo : IEntity<int>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ProjectInfo()
        {

        }

        /// <summary>
        /// Only available to owner and members
        /// </summary>
        public bool IsHidden { get; set; }

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
        /// Id of controling organization if any
        /// </summary>
        public int? ControlingOrganisationId { get; set; }

        /// <summary>
        /// Id of controling user if any
        /// </summary>
        public int? ControlingUserId { get; set; }

        /// <summary>
        ///     Description of a project.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Date when the project ends (UTC).
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        ///     An identifier for the project.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Id's of project members
        /// </summary>
        public IEnumerable<int> MemberIds { get; set; } = [];

        /// <summary>
        ///     Name of the project.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Name of person or organization that owns the project.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Project parameters
        /// </summary>
        public IEnumerable<ProjectParameter> ProjectParameters { get; set; }

        /// <summary>
        ///     Web address that leads to more information about the
        ///     project. The information should be accessible
        ///     from the most commonly used web browsers.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string ProjectURL { get; set; }

        /// <summary>
        ///     Date when the project starts (UTC).
        /// </summary>
        public DateTime? StartDate { get; set; }

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
        /// Owner id in user admin
        /// </summary>
        public int? UserServiceUserId { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(Owner)}: {Owner}";
        }
    }
}