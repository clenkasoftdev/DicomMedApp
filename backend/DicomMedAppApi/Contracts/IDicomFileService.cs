using Clenkasoft.DicomMedAppApi.DTOs;

namespace Clenkasoft.DicomMedAppApi.Contracts
{
    public interface IDicomFileService
    {
        Task<byte[]?> GetDicomFileAsync(int instanceId);
        Task<DicomUploadResponseDto> ImportDicomFileAsync(Stream fileStream, string fileName);
    }
}
