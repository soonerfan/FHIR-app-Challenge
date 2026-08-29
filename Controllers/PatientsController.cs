using System.Text.Json;
using FHIR_app_Challenge.Models;
using FHIR_app_Challenge.Services;
using Microsoft.AspNetCore.Mvc;

namespace FHIR_app_Challenge.Controllers;

[ApiController]
[Route("api/patients")]
public sealed class PatientsController : ControllerBase
{
    private readonly FhirPatientClient _client;

    public PatientsController(FhirPatientClient client) => _client = client;

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? name, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _client.SearchAsync(name, cancellationToken);
            return await ForwardAsync(response, cancellationToken);
        }
        catch (Exception exception) when (exception is InvalidOperationException or HttpRequestException)
        {
            return Problem(exception.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PatientInputModel patient, CancellationToken cancellationToken) =>
        await WriteAsync(() => _client.CreateAsync(patient, cancellationToken), cancellationToken);

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] PatientInputModel patient, CancellationToken cancellationToken) =>
        await WriteAsync(() => _client.UpdateAsync(id, patient, cancellationToken), cancellationToken);

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken cancellationToken) =>
        await ReadAsync(() => _client.GetPatientAsync(id, cancellationToken), cancellationToken);

    [HttpGet("{id}/observations")]
    public async Task<IActionResult> Observations(string id, CancellationToken cancellationToken) =>
        await ReadAsync(() => _client.GetObservationsAsync(id, cancellationToken), cancellationToken);

    [HttpGet("{id}/conditions")]
    public async Task<IActionResult> Conditions(string id, CancellationToken cancellationToken) =>
        await ReadAsync(() => _client.GetConditionsAsync(id, cancellationToken), cancellationToken);

    [HttpGet("{id}/medications")]
    public async Task<IActionResult> Medications(string id, CancellationToken cancellationToken) =>
        await ReadAsync(() => _client.GetMedicationsAsync(id, cancellationToken), cancellationToken);

    private static async Task<IActionResult> WriteAsync(Func<Task<HttpResponseMessage>> request, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await request();
            return await ForwardAsync(response, cancellationToken);
        }
        catch (Exception exception) when (exception is InvalidOperationException or HttpRequestException)
        {
            return new ObjectResult(new { error = exception.Message }) { StatusCode = StatusCodes.Status502BadGateway };
        }
    }

    private static async Task<IActionResult> ReadAsync(Func<Task<HttpResponseMessage>> request, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await request();
            return await ForwardAsync(response, cancellationToken);
        }
        catch (Exception exception) when (exception is InvalidOperationException or HttpRequestException)
        {
            return new ObjectResult(new { error = exception.Message }) { StatusCode = StatusCodes.Status502BadGateway };
        }
    }

    private static async Task<IActionResult> ForwardAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return new ContentResult
        {
            Content = string.IsNullOrWhiteSpace(content) ? JsonSerializer.Serialize(new { }) : content,
            ContentType = "application/fhir+json",
            StatusCode = (int)response.StatusCode
        };
    }
}