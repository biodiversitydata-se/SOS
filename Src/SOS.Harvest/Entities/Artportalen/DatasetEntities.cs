namespace SOS.Harvest.Entities.Artportalen
{
    public class DatasetEntities
    {
        public IEnumerable<DS_DatasetEntity> Datasets { get; set; }
        public IEnumerable<DS_AccessRights> AccessRights { get; set; }
        public IEnumerable<DS_Organisation> Organisations { get; set; }
        public IEnumerable<DS_Methodology> Methodologies { get; set; }
        public IEnumerable<DS_ProgrammeArea> ProgrammeAreas { get; set; }
        public IEnumerable<DS_Project> Projects { get; set; }
        public IEnumerable<DS_ProjectType> ProjectTypes { get; set; }
        public IEnumerable<DS_Purpose> Purposes { get; set; }
        public IEnumerable<DS_DatasetCreator> DatasetCreatorRelations { get; set; }
        public IEnumerable<DS_DatasetMethodology> DatasetMethodologyRelations { get; set; }
        public IEnumerable<DS_DatasetProject> DatasetProjectRelations { get; set; }

        public class DS_DatasetEntity
        {
            public int Id { get; set; }
            public string Identifier { get; set; }
            public string? Metadatalanguage { get; set; }
            public string? Language { get; set; }
            public int AccessRightsId { get; set; }
            public int PurposeId { get; set; }
            public int AssignerId { get; set; }
            //public int CreatorId { get; set; }
            public int OwnerinstitutionId { get; set; }
            public int PublisherId { get; set; }
            public string DataStewardship { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string? Description { get; set; }
            public string? Title { get; set; }
            public string? Spatial { get; set; }
            public int ProgrammeAreaId { get; set; }
            public string? DescriptionAccessRights { get; set; }
        }

        public class DS_AccessRights
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        public class DS_Methodology
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public string Link { get; set; }
            public string Name { get; set; }
        }

        public class DS_Organisation
        {
            public int Id { get; set; }
            public string Identifier { get; set; }
            public string Code { get; set; }
        }

        public class DS_ProgrammeArea
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        public class DS_Project
        {
            public int Id { get; set; }
            public string ProjectId { get; set; }
            public string ProjectCode { get; set; }
            public int ProjectTypeId { get; set; }
            public int ApProjectId { get; set; }
        }

        public class DS_ProjectType
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        public class DS_Purpose
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        public class DS_DatasetCreator
        {
            public int DatasetId { get; set; }
            public int OrganisationId { get; set; }
        }

        public class DS_DatasetMethodology
        {
            public int DatasetId { get; set; }
            public int MethodologyId { get; set; }
        }

        public class DS_DatasetProject
        {
            public int DatasetId { get; set; }
            public int ProjectId { get; set; }
        }
    }
}