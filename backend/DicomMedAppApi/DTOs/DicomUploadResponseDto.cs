namespace Clenkasoft.DicomMedAppApi.DTOs
{
    public record DicomUploadResponseDto(
         bool Success,
         string Message,
         int? PatientId,
         int? StudyId,
         int? SeriesId,
         int? InstanceId
    );
}
