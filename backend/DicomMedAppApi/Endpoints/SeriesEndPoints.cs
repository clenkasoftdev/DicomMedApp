using Clenkasoft.DicomMedAppApi.Contracts;
using Clenkasoft.DicomMedAppApi.Services;

namespace Clenkasoft.DicomMedAppApi.Endpoints
{
    public static class SeriesEndPoints
    {
        public static void Map(this WebApplication app)
        {
            app.MapGet("/api/series/{seriesId}/instances", GetSeriesInstances).WithName("GetSeries");
        }

        private static async Task<IResult> GetSeriesInstances(
            int seriesId,
           IDicomService dicomService,
           ILogger<DicomService> logger)
        {
            logger.LogInformation($"Fetching instances for series with id {seriesId}");

            var patients = await dicomService.GetInstancesBySeriesAsync(seriesId);
            return Results.Ok(patients);
        }
    }
}
