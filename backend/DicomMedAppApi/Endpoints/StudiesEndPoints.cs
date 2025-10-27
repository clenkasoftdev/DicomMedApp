using Clenkasoft.DicomMedAppApi.Contracts;
using Clenkasoft.DicomMedAppApi.Services;

namespace Clenkasoft.DicomMedAppApi.Endpoints
{
    public static class StudiesEndPoints
    {
        public static void Map(this WebApplication app)
        {
            app.MapGet("/api/studies/{studyId}/series", GetStudies).WithName("GetStudies");
        }

        private static async Task<IResult> GetStudies(
            int studyId,
           IDicomService dicomService,
           ILogger<DicomService> logger)
        {
            logger.LogInformation($"Fetching studies for series with id {studyId}");

            var patients = await dicomService.GetSeriesByStudyAsync(studyId);
            return Results.Ok(patients);
        }
    }
}
