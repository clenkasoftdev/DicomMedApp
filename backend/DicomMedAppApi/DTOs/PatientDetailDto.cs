namespace Clenkasoft.DicomMedAppApi.DTOs
{
    public record PatientDetailDto(
        int Id,
        string PatientId,
        string PatientName,
        DateTime? BirthDate,
        string? Gender,
        List<StudyDto> Studies
    );
}