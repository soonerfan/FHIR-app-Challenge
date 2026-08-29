using Microsoft.AspNetCore.Mvc;

namespace FHIR_app_Challenge.Controllers;

public sealed class PatientController : Controller
{
    [HttpGet("patient/{id}")]
    public IActionResult Details(string id) => View(new PatientDetailsViewModel(id));
}

public sealed record PatientDetailsViewModel(string Id);