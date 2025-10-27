namespace Clenkasoft.DicomMedAppApi.DTOs
{
    public record SeriesDto(
        int Id,
        string SeriesInstanceUid,
        string? Modality,
        int? SeriesNumber,
        string? SeriesDescription,
        int InstanceCount
    );
}
