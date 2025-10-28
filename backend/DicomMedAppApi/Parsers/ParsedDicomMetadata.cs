namespace Clenkasoft.DicomMedAppApi.Parsers
{
    public class ParsedDicomMetadata
    {
        // Patient
        public string PatientId { get; init; } = string.Empty;
        public string PatientName { get; init; } = string.Empty;
        public DateTime? PatientBirthDate { get; init; }
        public string? PatientSex { get; init; }

        // Study
        public string StudyInstanceUid { get; init; } = string.Empty;
        public DateTime? StudyDate { get; init; }
        public TimeSpan? StudyTime { get; init; }
        public string? StudyDescription { get; init; }
        public string? AccessionNumber { get; init; }
        public string? ReferringPhysician { get; init; }

        // Series
        public string SeriesInstanceUid { get; init; } = string.Empty;
        public string? Modality { get; init; }
        public int? SeriesNumber { get; init; }
        public string? SeriesDescription { get; init; }
        public string? BodyPartExamined { get; init; }
        public string? ProtocolName { get; init; }

        // Instance
        public string SopInstanceUid { get; init; } = string.Empty;
        public string? SopClassUid { get; init; }
        public int? InstanceNumber { get; init; }
        public string? TransferSyntaxUid { get; init; }

        // Image specific
        public int? Rows { get; init; }
        public int? Columns { get; init; }
        public int? BitsAllocated { get; init; }
    }
}
