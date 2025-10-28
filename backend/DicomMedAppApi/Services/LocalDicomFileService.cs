using Clenkasoft.DicomMedAppApi.Contracts;
using Clenkasoft.DicomMedAppApi.DTOs;
using Clenkasoft.DicomMedAppApi.Models;
using Clenkasoft.DicomMedAppApi.Parsers;


namespace Clenkasoft.DicomMedAppApi.Services
{
    public class LocalDicomFileService : IDicomFileService
    {
        private readonly IDicomService _dicomService;
        private readonly ILogger<LocalDicomFileService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _storageBasePath = "DicomStorage"; // Base path for DICOM files
        public LocalDicomFileService(IDicomService dicomService, ILogger<LocalDicomFileService> logger, IConfiguration configuration)
        {
            _dicomService = dicomService;
            _logger = logger;
            _configuration = configuration;
            _storageBasePath = configuration["DicomStorage:BasePath"] ?? "/tmp/dicom-storage";
        }

        public async Task<byte[]?> GetDicomFileAsync(int instanceId)
        {
            var instance = await _dicomService.GetInstanceByIdAsync(instanceId);

            if (instance == null)
                return null;

            var fullPath = Path.Combine(_storageBasePath, instance.FilePath);

            if (!File.Exists(fullPath))
                return null;

            return await File.ReadAllBytesAsync(fullPath);
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
                var relativeFilePath = Path.Combine(
                   DicomFileParser.SanitizePathSegment(dicomMetadata.PatientId),
                    DicomFileParser.SanitizePathSegment(dicomMetadata.StudyInstanceUid),
                    DicomFileParser.SanitizePathSegment(dicomMetadata.SeriesInstanceUid),
                    $"{DicomFileParser.SanitizePathSegment(dicomMetadata.SopInstanceUid)}.dcm"
                ).Replace("\\", "/"); // blobs use forward slashes


                var fullFilePath = Path.Combine(_storageBasePath, relativeFilePath);

                var directoryPath = Path.GetDirectoryName(fullFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath!);
                }

                // reset stream position to beginning
                // Reset stream position and save
                fileStream.Position = 0;
                using (var fileStreamOut = File.Create(fullFilePath))
                {
                    await fileStream.CopyToAsync(fileStreamOut);
                }

                var fileInfo = new FileInfo(fullFilePath);

                // Create a new instance record
                var instance = new Instance
                {
                    SopInstanceUid = dicomMetadata.SopInstanceUid,
                    SopClassUid = dicomMetadata.SopClassUid,
                    InstanceNumber = dicomMetadata.InstanceNumber,
                    FilePath = relativeFilePath,
                    FileSize = fileInfo.Length,
                    TransferSyntaxUid = dicomMetadata.TransferSyntaxUid,
                    Rows = dicomMetadata.Rows,
                    Columns = dicomMetadata.Columns,
                    BitsAllocated = dicomMetadata.BitsAllocated,
                    SeriesId = series.Id
                };

                await _dicomService.AddInstanceAsync(instance);

                return new DicomUploadResponseDto(
                    true,
                    "DICOM file imported successfully",
                    patient.Id,
                    study.Id,
                    series.Id,
                    instance.Id
                );


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing DICOM file {FileName}", fileName);
                return new DicomUploadResponseDto(
                   false,
                   $"Error importing DICOM file: {ex.Message}",
                   null,
                   null,
                   null,
                   null
               );
            }
        }

    }
}
