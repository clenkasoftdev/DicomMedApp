using Clenkasoft.DicomMedAppApi.DTOs;
using Clenkasoft.DicomMedAppApi.Models;

namespace Clenkasoft.DicomMedAppApi.Contracts
{
    public interface IDicomService
    {
        Task<DicomUploadResponseDto> ImportDicomFileAsync(Stream fileStream, string fileName);
        Task<PatientDto?> GetPatientAsync(int patientId);
        
        Task<List<PatientDto>> GetAllPatientsAsync();
        Task<PatientDetailDto?> GetPatientWithStudiesAsync(int patientId);

        Task<InstanceDto> GetInstanceByIdAsync(int instanceId);
        Task<List<SeriesDto>> GetSeriesByStudyAsync(int studyId);
        Task<List<InstanceDto>> GetInstancesBySeriesAsync(int seriesId);
        Task<List<StudyDto>> GetStudiesByPatientAsync(int patientId);

        #region Finding existing entities by unique identifiers
        Task<Patient?> GetPatientByPatientIdAsync(string patientId);
        Task<Study?> GetStudyByInstanceUidAsync(string studyInstanceUid);
        Task<Instance?> GetInstanceBySobUIdAsync(string instanceSubUid);
        Task<Series?> GetSeriesByInstanceUIdAsync(string seriesInstanceUid);
        #endregion



        Task<byte[]?> GetDicomFileAsync(int instanceId);

        Task AddPatientAsync(Patient patient);
        Task AddSeriesAsync(Series series);
        Task AddStudyAsync(Study study);
        Task AddInstanceAsync(Instance instance);
    }
}
