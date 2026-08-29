namespace FHIR_app_Challenge.Models;

public sealed class FhirOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string BearerToken { get; set; } = string.Empty;
}