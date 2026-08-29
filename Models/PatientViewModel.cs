using System.ComponentModel.DataAnnotations;

namespace FHIR_app_Challenge.Models;

public sealed class PatientInputModel
{
    [Required]
    [StringLength(100)]
    public string GivenName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FamilyName { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(male|female|other|unknown)$")]
    public string Gender { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateOnly? BirthDate { get; set; }
}

public sealed record PatientSummary(
    string Id,
    string GivenName,
    string FamilyName,
    string Gender,
    string BirthDate);