namespace Clenkasoft.DicomMedAppApi.DTOs
{
    public record PatientDto(
         int Id,
         string PatientId,
         string PatientName,
         DateTime? BirthDate,
         string? Gender,
         int StudyCount
     );
}
