namespace SOS.Process.Models.Processed
{
    /// <summary>
    /// Not defined in Darwin Core.
    /// Conservation related information about the taxon that
    /// the species observation is attached to.
    /// </summary>
    public class DarwinCoreProject
    {
        /// <summary>
        /// Not defined in Darwin Core.
        /// Indicates if species observations that are reported in
        /// a project are publicly available or not.
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Information about the type of project,
        /// for example 'Environmental monitoring'.
        /// </summary>
        public string ProjectCategory { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Description of a project.
        /// </summary>
        public string ProjectDescription { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Date when the project ends.
        /// </summary>
        public string ProjectEndDate { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// An identifier for the project.
        /// In the absence of a persistent global unique identifier,
        /// construct one from a combination of identifiers in
        /// the project that will most closely make the ProjectID
        /// globally unique.
        /// The format LSID (Life Science Identifiers) is used as GUID
        /// (Globally unique identifier).
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string ProjectID { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Name of the project.
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Name of person or organization that owns the project.
        /// </summary>
        public string ProjectOwner { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Date when the project starts.
        /// </summary>
        public string ProjectStartDate { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Web address that leads to more information about the
        /// project. The information should be accessible
        /// from the most commonly used web browsers.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string ProjectURL { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Survey method used in a project to
        /// retrieve species observations.
        /// </summary>
        public string SurveyMethod { get; set; }
    }
}
