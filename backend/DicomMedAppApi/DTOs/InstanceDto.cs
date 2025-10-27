namespace Clenkasoft.DicomMedAppApi.DTOs
{
    public record InstanceDto(
        int Id,
        string SopInstanceUid,
        int? InstanceNumber,
        string FilePath,
        long FileSize,
        int? Rows,
        int? Columns
    );

}
