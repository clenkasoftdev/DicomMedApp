using FellowOakDicom;

namespace Clenkasoft.DicomMedAppApi.Parsers
{
    public class DicomFileParser
    {
        public async Task<ParsedDicomMetadata> ParseAsync(Stream fileStream)
        {
            if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));

            // Ensure stream position at start for parsing
            if (fileStream.CanSeek) fileStream.Position = 0;

            var dicomFile = await DicomFile.OpenAsync(fileStream);
            var dataset = dicomFile.Dataset;

            // Extract and sanitize tags (matching the original LocalDicomFileService extraction)
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

            return new ParsedDicomMetadata
            {
                PatientId = patientId,
                PatientName = patientName,
                PatientBirthDate = patientBirthDate,
                PatientSex = patientSex,

                StudyInstanceUid = studyInstanceUid,
                StudyDate = studyDate,
                StudyTime = studyTime,
                StudyDescription = studyDescription,
                AccessionNumber = accessionNumber,
                ReferringPhysician = referringPhysician,

                SeriesInstanceUid = seriesInstanceUid,
                Modality = modality,
                SeriesNumber = seriesNumber,
                SeriesDescription = seriesDescription,
                BodyPartExamined = bodyPartExamined,
                ProtocolName = protocolName,

                SopInstanceUid = sopInstanceUid,
                SopClassUid = sopClassUid,
                InstanceNumber = instanceNumber,
                TransferSyntaxUid = transferSyntaxUid,

                Rows = rows,
                Columns = columns,
                BitsAllocated = bitsAllocated
            };
        }

        // Small helper to remove null bytes and trim strings
        private static string SanitizeString(string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Replace("\0", "").Trim();
        }

        // Ensure path segments don't contain invalid chars or path traversal
        public static string SanitizePathSegment(string? input)
        {
            var safe = SanitizeString(input);
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                safe = safe.Replace(c, '_');
            }
            // additionally prevent path traversal
            safe = safe.Replace("..", "_");
            if (string.IsNullOrWhiteSpace(safe)) safe = "unknown";
            return safe;
        }
    }
}
