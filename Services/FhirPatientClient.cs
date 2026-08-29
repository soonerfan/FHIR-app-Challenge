using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FHIR_app_Challenge.Models;

namespace FHIR_app_Challenge.Services;

public sealed class FhirPatientClient
{
    private readonly HttpClient _httpClient;
    private readonly FhirOptions _options;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public FhirPatientClient(HttpClient httpClient, FhirOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<HttpResponseMessage> SearchAsync(string? name, CancellationToken cancellationToken)
    {
        var path = "Patient";
        if (!string.IsNullOrWhiteSpace(name))
        {
            path += $"?name={Uri.EscapeDataString(name.Trim())}";
        }

        return await SendAsync(HttpMethod.Get, path, null, cancellationToken);
    }

    public Task<HttpResponseMessage> CreateAsync(PatientInputModel patient, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Post, "Patient", BuildResource(patient), cancellationToken);

    public Task<HttpResponseMessage> UpdateAsync(string id, PatientInputModel patient, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Put, $"Patient/{Uri.EscapeDataString(id)}", BuildResource(patient, id), cancellationToken);

    public Task<HttpResponseMessage> GetPatientAsync(string id, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Get, $"Patient/{Uri.EscapeDataString(id)}", null, cancellationToken);

    public Task<HttpResponseMessage> GetObservationsAsync(string id, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Get, $"Observation?subject=Patient/{Uri.EscapeDataString(id)}&code=8867-4,8310-5,9279-1,59408-5,8302-2,29463-7,39156-5,55284-4", null, cancellationToken);

    public Task<HttpResponseMessage> GetConditionsAsync(string id, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Get, $"Condition?patient={Uri.EscapeDataString(id)}", null, cancellationToken);

    public Task<HttpResponseMessage> GetMedicationsAsync(string id, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Get, $"MedicationRequest?patient={Uri.EscapeDataString(id)}", null, cancellationToken);

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl) || string.IsNullOrWhiteSpace(_options.BearerToken))
        {
            throw new InvalidOperationException("FHIR:BaseUrl and FHIR:BearerToken must be configured.");
        }

        using var request = new HttpRequestMessage(method, new Uri(new Uri(_options.BaseUrl.TrimEnd('/') + "/"), path));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.BearerToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/fhir+json"));
        if (resource is not null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(resource, _jsonOptions), Encoding.UTF8, "application/fhir+json");
        }

        return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    private static object BuildResource(PatientInputModel input, string? id = null)
    {
        var resource = new Dictionary<string, object?>
        {
            ["resourceType"] = "Patient",
            ["name"] = new[] { new { given = new[] { input.GivenName.Trim() }, family = input.FamilyName.Trim() } },
            ["gender"] = input.Gender,
            ["birthDate"] = input.BirthDate?.ToString("yyyy-MM-dd")
        };

        if (!string.IsNullOrWhiteSpace(id))
        {
            resource["id"] = id;
        }

        return resource;
    }
}