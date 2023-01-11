using System;
using System.Collections.Generic;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     Dataset object
    /// </summary>
    public class ArtportalenDatasetMetadata : IEntity<int>
    {
        public int Id { get; set; }
        public string Identifier { get; set; }
        public string Metadatalanguage { get; set; }
        public string Language { get; set; }
        public string DataStewardship { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Spatial { get; set; }
        public string DescriptionAccessRights { get; set; }
        public IEnumerable<Project> Projects { get; set; }
        public IEnumerable<Methodology> Methodologies { get; set; }
        public IEnumerable<Organisation> Creators { get; set; }
        public Organisation Assigner { get; set; }
        public Organisation OwnerInstitution { get; set; }
        public Organisation Publisher { get; set; }
        public AccessRights DatasetAccessRights { get; set; }
        public Purpose DatasetPurpose { get; set; }
        public ProgrammeArea DatasetProgrammeArea { get; set; }


        public class AccessRights
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        public class Methodology
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public string Link { get; set; }
            public string Name { get; set; }
        }

        public class Organisation
        {
            public int Id { get; set; }
            public string Identifier { get; set; }
            public string Code { get; set; }
        }

        public class ProgrammeArea
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        public class Project
        {
            public int Id { get; set; }
            public string ProjectId { get; set; }
            public string ProjectCode { get; set; }
            public ProjectType ProjectType { get; set; }
            public int ApProjectId { get; set; }
        }

        public class ProjectType
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        public class Purpose
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }
    }
}