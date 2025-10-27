using Clenkasoft.DicomMedAppApi.Contracts;
using Clenkasoft.DicomMedAppApi.DTOs;

namespace Clenkasoft.DicomMedAppApi.Services
{
    public class AzureStorageBlobService : IDicomFileService
    {
        public Task<byte[]?> GetDicomFileAsync(int instanceId)
        {
            throw new NotImplementedException();
        }

        public Task<DicomUploadResponseDto> ImportDicomFileAsync(Stream fileStream, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
