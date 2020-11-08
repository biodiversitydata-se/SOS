namespace SOS.Lib.Enums.VocabularyValues
{
    /// <summary>
    ///     Enumeration of ValidationStatus.
    /// </summary>
    public enum ValidationStatusId
    {
        /// <summary>
        ///     Verifired.
        ///     (Validerad)
        /// </summary>
        Verified = 0,

        /// <summary>
        ///     Unvalidated.
        ///     (Ovaliderad)
        /// </summary>
        Unvalidated = 10,

        /// <summary>
        ///     Validation requested.
        ///     (Validering efterfrågas)
        /// </summary>
        ValidationRequested = 12,

        /// <summary>
        ///     Dialogue at reporter, hidden sighting.
        ///     (Dialog hos rapportör, dolt fynd)
        /// </summary>
        DialogueAtReporterHiddenSighting = 13,

        /// <summary>
        ///     Documentation requested.
        ///     (Dokumentation efterfrågas)
        /// </summary>
        DocumentationRequested = 14,

        /// <summary>
        ///     Dialogue with reporter.
        ///     (Dialog hos rapportör)
        /// </summary>
        DialogueWithReporter = 15,

        /// <summary>
        ///     Dialogue with validator.
        ///     (Dialog hos validator)
        /// </summary>
        DialogueWithValidator = 16,

        /// <summary>
        ///     Need not be validated.
        ///     (Behöver inte valideras)
        /// </summary>
        NeedNotBeValidated = 20,

        /// <summary>
        ///     Description required (for the regional records committee).
        ///     (Rapport ska skrivas (för Rrk))
        /// </summary>
        DescriptionRequiredForTheRegionalRecordsCommittee = 30,

        /// <summary>
        ///     Report written (for Regional Validation Committee).
        ///     (Rapport skriven (för Rrk))
        /// </summary>
        ReportWrittenForRegionalValidationCommittee = 35,

        /// <summary>
        ///     Description required (for the National Rarities Committee).
        ///     (Rapport ska skrivas (för RK))
        /// </summary>
        DescriptionRequiredForTheNationalRaritiesCommittee = 40,

        /// <summary>
        ///     Report treated regional (for National Rarities Committee).
        ///     (Rapport behandlad av Rrk (för RK))
        /// </summary>
        ReportTreatedRegionalForNationalRaritiesCommittee = 45,

        /// <summary>
        ///     Rejected.
        ///     (Underkänd)
        /// </summary>
        Rejected = 50,

        /// <summary>
        ///     Approved based on reporters documentation.
        ///     (Godkänd baserat på observatörens uppgifter)
        /// </summary>
        ApprovedBasedOnReportersDocumentation = 60,

        /// <summary>
        ///     Approved. Specimen checked by validator.
        ///     (Godkänd. Belägg granskat av validerare.)
        /// </summary>
        ApprovedSpecimenCheckedByValidator = 61,

        /// <summary>
        ///     Approved based on image, sound or video recording.
        ///     (Godkänd. Foto (eller ljud) granskat av validerare)
        /// </summary>
        ApprovedBasedOnImageSoundOrVideoRecording = 62,

        /// <summary>
        ///     Approved based on reporters rarity form.
        ///     (Godkänd baserat på raritetsblankett)
        /// </summary>
        ApprovedBasedOnReportersRarityForm = 63,

        /// <summary>
        ///     Approved based on determinators verification.
        ///     (Godkänd baserat på determinatörs verifiering)
        /// </summary>
        ApprovedBasedOnDeterminatorsVerification = 64,

        /// <summary>
        ///     Approved based on reporters old rarity form.
        ///     (Godkänd baserat på äldre raritetsblankett)
        /// </summary>
        ApprovedBasedOnReportersOldRarityForm = 65,

        /// <summary>
        ///     Not able to validate.
        ///     (Ej möjlig att validera)
        /// </summary>
        NotAbleToValidate = 70
    }
}