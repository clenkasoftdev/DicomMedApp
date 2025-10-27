using Clenkasoft.DicomMedAppApi.Models;

namespace Clenkasoft.DicomMedAppApi.Contracts
{
    public interface IDicomRepository
    {
        Task<Patient?> GetPatientAsync(int patientId);
        Task<List<Patient>> GetAllPatientsAsync();
        Task<List<Study>> GetStudiesByPatientAsync(int patientId);

        Task<List<Series>> GetSeriesByStudyAsync(int studyId);
        Task<List<Instance>> GetInstancesBySeriesAsync(int seriesId);
        Task<Instance> GetInstanceByIdAsync(int instanceId);

        #region Finding existing entities by unique identifiers
        Task<Patient?> GetPatientByPatientIdAsync(string patientId);
        Task<Study> GetStudyByInstanceUidAsync(string studyInstanceUid);
        Task<Instance> GetInstanceBySobUIdAsync(string instanceSubUid);
        Task<Series> GetSeriesByInstanceUIdAsync(string seriesInstanceUid);
        #endregion

        Task AddPatientAsync(Patient patient);
        Task AddSeriesAsync(Series series);
        Task AddStudyAsync(Study study);
        Task AddInstanceAsync(Instance instance);
    }
}
