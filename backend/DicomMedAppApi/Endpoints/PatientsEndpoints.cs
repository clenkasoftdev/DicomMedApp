

using Clenkasoft.DicomMedAppApi.Contracts;
using Clenkasoft.DicomMedAppApi.Services;

namespace Clenkasoft.DicomMedAppApi.Endpoints
{
    public static class PatientsEndPoints
    {
        public static void Map(this WebApplication app)
        {
            app.MapGet("/api/patients", GetAllPatients).WithName("patients");
            app.MapGet("/api/patients/{id}", GetPatient).WithName("patient");
            app.MapGet("/api/patients/{id}/details", GetPatientDetails).WithName("patientdetails");
            app.MapGet("/api/patients/{id}/studies", GetPatientStudies).WithName("patientstudies");
        }

        private static async Task<IResult> GetAllPatients(
           IDicomService dicomService,
           ILogger<DicomService> logger)
        {
            logger.LogInformation("Fetching all patients");

            var patients = await dicomService.GetAllPatientsAsync();
            return Results.Ok(patients);
        }

        private static async Task<IResult> GetPatient(
           int id,
           IDicomService dicomService,
           ILogger<DicomService> logger)
        {
            logger.LogInformation($"Fetching patients with id {id} ");

            var patient = await dicomService.GetPatientAsync(id);
            return Results.Ok(patient);
        }

        private static async Task<IResult> GetPatientDetails(
           int id,
           IDicomService dicomService,
           ILogger<DicomService> logger)
        {
            logger.LogInformation($"Fetching all patient with id  {id} details");

            var patients = await dicomService.GetPatientWithStudiesAsync(id);
            return Results.Ok(patients);
        }

        private static async Task<IResult> GetPatientStudies(
           int id,
           IDicomService dicomService,
           ILogger<DicomService> logger)
        {
            logger.LogInformation($"Fetching all patient with id  {id}'s studies");

            var patients = await dicomService.GetStudiesByPatientAsync(id);
            return Results.Ok(patients);
        }
    }
}
