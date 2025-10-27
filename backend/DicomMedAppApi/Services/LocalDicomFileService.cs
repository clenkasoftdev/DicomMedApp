using Clenkasoft.DicomMedAppApi.Contracts;
using Clenkasoft.DicomMedAppApi.DTOs;
using Clenkasoft.DicomMedAppApi.Models;
using FellowOakDicom;


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
                // 1. Parse DICOM file using fo-dicom
                var dicomFile = await DicomFile.OpenAsync(fileStream);
                var dataset = dicomFile.Dataset;

                // 2. Extract DICOM metadata
                var patientId = SanitizeString(dataset.GetSingleValueOrDefault(DicomTag.PatientID, "UNKNOWN"));
                var patientName = SanitizeString(dataset.GetSingleValueOrDefault(DicomTag.PatientName, "UNKNOWN"));
                var patientBirthDate = dataset.GetSingleValueOrDefault<DateTime?>(DicomTag.PatientBirthDate, null);
                var patientSex = SanitizeString(dataset.GetSingleValueOrDefault<string?>(DicomTag.PatientSex, null));

                var studyInstanceUid = SanitizeString(dataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty));
                var studyDate = dataset.GetSingleValueOrDefault<DateTime?>(DicomTag.StudyDate, null);
                var studyTime = dataset.GetSingleValueOrDefault<TimeSpan?>(DicomTag.StudyTime, null);
                var studyDescription = SanitizeString(dataset.GetSingleValueOrDefault<string?>(DicomTag.StudyDescription, null));
                var accessionNumber = SanitizeString(dataset.GetSingleValueOrDefault<string?>(DicomTag.AccessionNumber, null));
                var referringPhysician = SanitizeString(dataset.GetSingleValueOrDefault<string?>(DicomTag.ReferringPhysicianName, null));

                var seriesInstanceUid = SanitizeString(dataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty));
                var modality = SanitizeString(dataset.GetSingleValueOrDefault<string?>(DicomTag.Modality, null));
                var seriesNumber = dataset.GetSingleValueOrDefault<int?>(DicomTag.SeriesNumber, null);
                var seriesDescription = SanitizeString(dataset.GetSingleValueOrDefault<string?>(DicomTag.SeriesDescription, null));
                var bodyPartExamined = SanitizeString(dataset.GetSingleValueOrDefault<string?>(DicomTag.BodyPartExamined, null));
                var protocolName = SanitizeString(dataset.GetSingleValueOrDefault<string?>(DicomTag.ProtocolName, null));

                var sopInstanceUid = SanitizeString(dataset.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, string.Empty));
                var sopClassUid = dataset.GetSingleValueOrDefault<string?>(DicomTag.SOPClassUID, null);
                var instanceNumber = dataset.GetSingleValueOrDefault<int?>(DicomTag.InstanceNumber, null);
                var transferSyntaxUid = dicomFile.FileMetaInfo.TransferSyntax?.UID.UID;

                // Image specific tags
                var rows = dataset.GetSingleValueOrDefault<int?>(DicomTag.Rows, null);
                var columns = dataset.GetSingleValueOrDefault<int?>(DicomTag.Columns, null);
                var bitsAllocated = dataset.GetSingleValueOrDefault<int?>(DicomTag.BitsAllocated, null);

                // 3. Find or create Patient
                var patient = await _dicomService.GetPatientByPatientIdAsync(patientId);
                if(patient == null)
                {
                    patient = new Patient
                    {
                        PatientId = patientId,
                        PatientName = patientName,
                        BirthDate = patientBirthDate,
                        Gender = patientSex
                    };

                    await _dicomService.AddPatientAsync(patient);
                }

                // 4. Find or create Study
                var study = await _dicomService.GetStudyByInstanceUidAsync(studyInstanceUid);
                if (study == null)
                {
                    study = new Study
                    {
                        StudyInstanceUid = studyInstanceUid,
                        StudyDate = studyDate,
                        StudyTime = studyTime,
                        StudyDescription = studyDescription,
                        AccessionNumber = accessionNumber,
                        ReferringPhysicianName = referringPhysician,
                        PatientId = patient.Id
                    };
                    await _dicomService.AddStudyAsync(study);
                }

                // 5. Find or create Series
                var series = await _dicomService.GetSeriesByInstanceUIdAsync(seriesInstanceUid);
                if (series == null)
                {
                    series = new Series
                    {
                        SeriesInstanceUid = seriesInstanceUid,
                        Modality = modality,
                        SeriesNumber = seriesNumber,
                        SeriesDescription = seriesDescription,
                        BodyPartExamined = bodyPartExamined,
                        ProtocolName = protocolName,
                        StudyId = study.Id

                    };
                    await _dicomService.AddSeriesAsync(series);
                }

                // 6. Check if Instance already exists
                var existingInstance = await _dicomService.GetInstanceBySobUIdAsync(sopInstanceUid);
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

                // 7. Save DICOM file to local storage
                var relativeFilePath = Path.Combine(
                    patientId,
                    studyInstanceUid,
                    seriesInstanceUid,
                    $"{sopInstanceUid}.dcm"
                );

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
                    SopInstanceUid = sopInstanceUid,
                    SopClassUid = sopClassUid,
                    InstanceNumber = instanceNumber,
                    FilePath = relativeFilePath,
                    FileSize = fileInfo.Length,
                    TransferSyntaxUid = transferSyntaxUid,
                    Rows = rows,
                    Columns = columns,
                    BitsAllocated = bitsAllocated,
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

        /// <summary>
        ///  DICOM files can have null bytes (\0) in their text fields, but 
        ///  PostgreSQL doesn't allow null bytes in text/varchar columns.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string SanitizeString(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remove null bytes and other problematic characters
            return input.Replace("\0", "").Trim();
        }
    }
}
