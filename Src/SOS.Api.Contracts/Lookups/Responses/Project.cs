namespace SOS.Api.Contracts.Lookups.Responses;
/// <summary>
/// Artportalen project information.
/// </summary>
public class Project
{

    /// <summary>
    /// Indicates that the project is only available to its owner and members.
    /// </summary>
    public bool IsHidden { get; set; }

    /// <summary>
    /// Indicates if species observations that are reported in
    /// <br/>a project are publicly available or not.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Information about the type of project,
    /// <br/>for example 'Environmental monitoring'.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Information about the type of project in Swedish,
    /// <br/>for example 'Miljöövervakning'.
    /// </summary>
    public string CategorySwedish { get; set; }

    /// <summary>The controlling organisation identifier, when present.</summary>
    public int? ControlingOrganisationId { get; set; }

    /// <summary>The controlling user identifier, when present.</summary>
    public int? ControlingUserId { get; set; }

    /// <summary>
    /// Description of a project.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Date when the project ends (UTC).
    /// </summary>
    public System.DateTimeOffset? EndDate { get; set; }

    /// <summary>
    /// An identifier for the project.
    /// </summary>
    public int Id { get; set; }

    /// <summary>Identifiers of users who are members of the project.</summary>
    public ICollection<int> MemberIds { get; set; }

    /// <summary>
    /// Name of the project.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Name of person or organization that owns the project.
    /// </summary>
    public string Owner { get; set; }

    /// <summary>
    /// Project parameters
    /// </summary>
    public ICollection<ProjectParameter> ProjectParameters { get; set; }

    /// <summary>
    /// Web address that leads to more information about the
    /// <br/>project. The information should be accessible
    /// <br/>from the most commonly used web browsers.
    /// </summary>
    public string ProjectURL { get; set; }

    /// <summary>
    /// Date when the project starts (UTC).
    /// </summary>
    public System.DateTimeOffset? StartDate { get; set; }

    /// <summary>
    /// Survey method used in a project to
    /// <br/>retrieve species observations.
    /// </summary>
    public string SurveyMethod { get; set; }

    /// <summary>
    /// Survey method URL.
    /// </summary>
    public string SurveyMethodUrl { get; set; }

    /// <summary>The project owner's identifier in the user service.</summary>
    public int? UserServiceUserId { get; set; }

}


