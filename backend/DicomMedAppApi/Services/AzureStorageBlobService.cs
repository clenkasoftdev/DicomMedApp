using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Clenkasoft.DicomMedAppApi.Contracts;
using Clenkasoft.DicomMedAppApi.DTOs;
using Clenkasoft.DicomMedAppApi.Models;
using Clenkasoft.DicomMedAppApi.Parsers;

namespace Clenkasoft.DicomMedAppApi.Services
{
    public class AzureStorageBlobService : IDicomFileService
    {
        private readonly IDicomService _dicomService;
        private readonly ILogger<AzureStorageBlobService> _logger;
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public AzureStorageBlobService(IDicomService dicomService, ILogger<AzureStorageBlobService> logger, IConfiguration configuration)
        {
            _dicomService = dicomService;
            _logger = logger;
            _configuration = configuration;

            _containerName = configuration["AzureStorage:ContainerName"] ?? "dicomfiles";

            var useManagedIdentity = bool.TryParse(configuration["AzureStorage:UseManagedIdentity"], out var umi) && umi;
            if (useManagedIdentity)
            {
                var endpoint = configuration["AzureStorage:BlobServiceEndpoint"];
                if (string.IsNullOrWhiteSpace(endpoint))
                    throw new InvalidOperationException("AzureStorage:BlobServiceEndpoint must be set when UseManagedIdentity is true.");
                _blobServiceClient = new BlobServiceClient(new Uri(endpoint), new DefaultAzureCredential());
            }
            else
            {
                var conn = configuration["AzureStorage:ConnectionString"];
                if (string.IsNullOrWhiteSpace(conn))
                    throw new InvalidOperationException("AzureStorage:ConnectionString is not configured.");
                _blobServiceClient = new BlobServiceClient(conn);
            }

            // ensure container exists
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            container.CreateIfNotExists();
        }


        public async Task<byte[]?> GetDicomFileAsync(int instanceId)
        {
            try
            {
                var instanceDto = await _dicomService.GetInstanceByIdAsync(instanceId);
                if (instanceDto == null || string.IsNullOrWhiteSpace(instanceDto.FilePath))
                    return null;

                var container = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blob = container.GetBlobClient(instanceDto.FilePath);

                if (!await blob.ExistsAsync())
                {
                    _logger.LogWarning("Blob not found: {BlobName}", instanceDto.FilePath);
                    return null;
                }

                // Download content to memory
                var download = await blob.DownloadContentAsync();
                return download.Value.Content.ToArray();
            }
            catch (RequestFailedException rfe)
            {
                _logger.LogError(rfe, "Azure storage request failed while getting DICOM file for instance {InstanceId}", instanceId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting DICOM file for instance {InstanceId}", instanceId);
                return null;
            }
        }

        public async Task<DicomUploadResponseDto> ImportDicomFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                // 1. Get DICOM Parser
                DicomFileParser dicomFileParser = new DicomFileParser();

                // 2. Parse Dicom file and Extract metadata
                var dicomMetadata = await dicomFileParser.ParseAsync(fileStream);

                // 3. Find or create Patient
                var patient = await _dicomService.GetPatientByPatientIdAsync(dicomMetadata.PatientId);
                if (patient == null)
                {
                    patient = new Patient
                    {
                        PatientId = dicomMetadata.PatientId,
                        PatientName = dicomMetadata.PatientName,
                        BirthDate = dicomMetadata.PatientBirthDate,
                        Gender = dicomMetadata.PatientSex
                    };

                    await _dicomService.AddPatientAsync(patient);
                }

                // 4. Find or create Study
                var study = await _dicomService.GetStudyByInstanceUidAsync(dicomMetadata.StudyInstanceUid);
                if (study == null)
                {
                    study = new Study
                    {
                        StudyInstanceUid = dicomMetadata.StudyInstanceUid,
                        StudyDate = dicomMetadata.StudyDate,
                        StudyTime = dicomMetadata.StudyTime,
                        StudyDescription = dicomMetadata.StudyDescription,
                        AccessionNumber = dicomMetadata.AccessionNumber,
                        ReferringPhysicianName = dicomMetadata.ReferringPhysician,
                        PatientId = patient.Id
                    };
                    await _dicomService.AddStudyAsync(study);
                }

                // 5. Find or create Series
                var series = await _dicomService.GetSeriesByInstanceUIdAsync(dicomMetadata.SeriesInstanceUid);
                if (series == null)
                {
                    series = new Series
                    {
                        SeriesInstanceUid = dicomMetadata.SeriesInstanceUid,
                        Modality = dicomMetadata.Modality,
                        SeriesNumber = dicomMetadata.SeriesNumber,
                        SeriesDescription = dicomMetadata.SeriesDescription,
                        BodyPartExamined = dicomMetadata.BodyPartExamined,
                        ProtocolName = dicomMetadata.ProtocolName,
                        StudyId = study.Id
                    };
                    await _dicomService.AddSeriesAsync(series);
                }

                // 6. Check if Instance already exists
                var existingInstance = await _dicomService.GetInstanceBySobUIdAsync(dicomMetadata.SopInstanceUid);
                if (existingInstance != null)
                {
                    return new DicomUploadResponseDto(
                        false,
                        "Instance already exists in the database",
                        patient.Id,
                        study.Id,
                        series.Id,
                        existingInstance.Id
                    );
                }

                // 7. Upload DICOM file to Azure Blob Storage
                // Compose a logical blob name (folder-like)
                var blobName = Path.Combine(
                   DicomFileParser.SanitizePathSegment(dicomMetadata.PatientId),
                    DicomFileParser.SanitizePathSegment(dicomMetadata.StudyInstanceUid),
                    DicomFileParser.SanitizePathSegment(dicomMetadata.SeriesInstanceUid),
                    $"{DicomFileParser.SanitizePathSegment(dicomMetadata.SopInstanceUid)}.dcm"
                ).Replace("\\", "/"); // blobs use forward slashes

                var container = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = container.GetBlobClient(blobName);

                // Reset stream position for upload
                if (fileStream.CanSeek)
                    fileStream.Position = 0;

                await blobClient.UploadAsync(fileStream, overwrite: true);

                // Fetch properties to get size (if needed)
                var properties = await blobClient.GetPropertiesAsync();

                // 8. Create a new instance record
                var instance = new Instance
                {
                    SopInstanceUid = dicomMetadata.SopInstanceUid,
                    SopClassUid = dicomMetadata.SopClassUid,
                    InstanceNumber = dicomMetadata.InstanceNumber,
                    FilePath = blobName,
                    FileSize = properties.Value.ContentLength,
                    TransferSyntaxUid = dicomMetadata.TransferSyntaxUid,
                    Rows = dicomMetadata.Rows,
                    Columns = dicomMetadata.Columns,
                    BitsAllocated = dicomMetadata.BitsAllocated,
                    SeriesId = series.Id
                };

                await _dicomService.AddInstanceAsync(instance);

                return new DicomUploadResponseDto(
                    true,
                    "DICOM file imported and uploaded to Azure Blob Storage successfully",
                    patient.Id,
                    study.Id,
                    series.Id,
                    instance.Id
                );
            }
            catch (RequestFailedException rfe)
            {
                _logger.LogError(rfe, "Azure Storage request failed while importing DICOM file {FileName}", fileName);
                return new DicomUploadResponseDto(false, $"Azure storage error: {rfe.Message}", null, null, null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing DICOM file {FileName}", fileName);
                return new DicomUploadResponseDto(false, $"Error importing DICOM file: {ex.Message}", null, null, null, null);
            }
        }

    }
}
