namespace Clenkasoft.DicomMedAppApi.DTOs
{
    public record StudyDto(
         int Id,
         string StudyInstanceUid,
         DateTime? StudyDate,
         string? StudyDescription,
         string? AccessionNumber,
         string PatientName,
         int SeriesCount
     );

}
