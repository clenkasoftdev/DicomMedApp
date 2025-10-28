using Clenkasoft.DicomMedAppApi.Contracts;
using Clenkasoft.DicomMedAppApi.DTOs;
using Clenkasoft.DicomMedAppApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clenkasoft.DicomMedAppApi.Endpoints
{
    public static class FileEndPoints
    {
        public static void Map(this WebApplication app)
        {
            app.MapPost("/api/dicom/upload", UploadDicomFile)
         .WithName("UploadDicomFile");

            app.MapPost("/api/dicom/upload/batch", UploadMultipleDicomFiles)
                .WithName("UploadMultipleDicomFiles");

            app.MapGet("/api/dicom/download/{instanceId:int}", DownloadDicomFile).WithName("DownloadDicomFile");


        }

        private static async Task<IResult> UploadDicomFile(
    HttpRequest request,
    IDicomFileService dicomFileService,
    ILogger<LocalDicomFileService> logger)
        {
            if (!request.HasFormContentType)
                return Results.BadRequest(new { message = "Expected multipart/form-data" });

            var form = await request.ReadFormAsync();
            var file = form.Files.GetFile("file");

            if (file == null || file.Length == 0)
            {
                logger.LogWarning("No file provided for single upload");
                return Results.BadRequest(new { success = false, message = "No file provided" });
            }

            if (!file.FileName.EndsWith(".dcm", StringComparison.OrdinalIgnoreCase))
                return Results.BadRequest(new { message = "File must be a DICOM file (.dcm)" });

            try
            {
                logger.LogInformation("Uploading DICOM file: {FileName}, Size: {Size} bytes",
                    file.FileName, file.Length);

                await using var stream = file.OpenReadStream();
                var result = await dicomFileService.ImportDicomFileAsync(stream, file.FileName);

                if (!result.Success)
                    return Results.BadRequest(result);

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading file {FileName}", file?.FileName);
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> UploadMultipleDicomFiles(
     HttpRequest request,
     IDicomFileService dicomFileService,
     ILogger<LocalDicomFileService> logger)
        {
            if (!request.HasFormContentType)
                return Results.BadRequest(new { message = "Expected multipart/form-data" });

            var form = await request.ReadFormAsync();
            var files = form.Files; // Gets all files

            if (files == null || files.Count == 0)
            {
                logger.LogWarning("No files provided for batch upload");
                return Results.BadRequest(new { success = false, message = "No files provided" });
            }

            var results = new List<object>();

            foreach (var file in files)
            {
                if (!file.FileName.EndsWith(".dcm", StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(new { fileName = file.FileName, success = false, message = "Not a DICOM file" });
                    continue;
                }

                try
                {
                    logger.LogInformation("Uploading DICOM file: {FileName}, Size: {Size} bytes",
                        file.FileName, file.Length);

                    await using var stream = file.OpenReadStream();
                    var result = await dicomFileService.ImportDicomFileAsync(stream, file.FileName);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
                    results.Add(new { fileName = file.FileName, success = false, message = ex.Message });
                }
            }

            return Results.Ok(new { totalFiles = files.Count, results });
        }

        private static async Task<IResult> DownloadDicomFile(
           int instanceId,
           IDicomFileService dicomFileService,
           ILogger<LocalDicomFileService> logger)
        {
            try
            {
                var bytes = await dicomFileService.GetDicomFileAsync(instanceId);
                if (bytes == null || bytes.Length == 0)
                {
                    logger.LogWarning("Requested DICOM file not found for instanceId {InstanceId}", instanceId);
                    return Results.NotFound(new { success = false, message = "File not found" });
                }

                // Standard DICOM mime type (application/dicom) or octet-stream fallback
                return Results.File(bytes, "application/dicom", $"instance_{instanceId}.dcm");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error downloading DICOM file for instanceId {InstanceId}", instanceId);
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        }


    }
}
