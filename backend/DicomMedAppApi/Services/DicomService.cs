using Clenkasoft.DicomMedAppApi.Contracts;
using Clenkasoft.DicomMedAppApi.DTOs;
using Clenkasoft.DicomMedAppApi.Models;

namespace Clenkasoft.DicomMedAppApi.Services
{
    public class DicomService : IDicomService
    {
        private readonly IDicomRepository _dicomRepository;
        private readonly ILogger<DicomService> _logger;
        public DicomService(IDicomRepository dicomRepository, ILogger<DicomService> logger)
        {
            _dicomRepository = dicomRepository;
            _logger = logger;
        }

        #region PatientOperations
        public async Task<PatientDto?> GetPatientAsync(int patientId)
        {
            var patient = await _dicomRepository.GetPatientAsync(patientId);
            if (patient == null)
                return null;
            
            return new PatientDto(
                patient.Id,
                patient.PatientId,
                patient.PatientName,
                patient.BirthDate,
                patient.Gender,
                patient.Studies.Count
            );
        }


        public async Task<List<PatientDto>> GetAllPatientsAsync()
        {
            var res = await _dicomRepository.GetAllPatientsAsync();

            return res.Select(p => new PatientDto
                (p.Id,
                    p.PatientId,
                    p.PatientName,
                    p.BirthDate,
                    p.Gender,
                    p.Studies.Count
                )
            ).ToList();
        }

        public async Task<PatientDetailDto?> GetPatientWithStudiesAsync(int patientId)
        {
            var patient = await _dicomRepository.GetPatientAsync(patientId);

            if (patient == null)
                return null;

            var studies = await _dicomRepository.GetStudiesByPatientAsync(patientId);


            return new PatientDetailDto(
                patient.Id,
                patient.PatientId,
                patient.PatientName,
                patient.BirthDate,
                patient.Gender,
                new List<StudyDto>(studies.Select(s => new StudyDto(
                    s.Id,
                    s.StudyInstanceUid,
                    s.StudyDate,
                    s.StudyDescription,
                    s.AccessionNumber,
                    s.Patient.PatientName,
                    s.Series.Count
                ))).ToList()
            );
        }

        public async Task<List<SeriesDto>> GetSeriesByStudyAsync(int studyId)
        {
            var seriesByStudy = await _dicomRepository.GetSeriesByStudyAsync(studyId);

            return seriesByStudy.Select(s => new SeriesDto(
                    s.Id,
                    s.SeriesInstanceUid,
                    s.Modality,
                    s.SeriesNumber,
                    s.SeriesDescription,
                    s.Instances.Count
                )).ToList();
        }

        public async Task<List<StudyDto>> GetStudiesByPatientAsync(int patientId)
        {
            var studiesByPatient =  await _dicomRepository.GetStudiesByPatientAsync(patientId);
            return studiesByPatient.Select(s => new StudyDto(
                    s.Id,
                    s.StudyInstanceUid,
                    s.StudyDate,
                    s.StudyDescription,
                    s.AccessionNumber,
                    s.Patient.PatientName,
                    s.Series.Count
                )).ToList();
        }

        public async Task<List<InstanceDto>> GetInstancesBySeriesAsync(int seriesId)
        {
            var instancesBySeries = await _dicomRepository.GetInstancesBySeriesAsync(seriesId);

            return instancesBySeries.Select(i => new InstanceDto(
                    i.Id,
                    i.SopInstanceUid,
                    i.InstanceNumber,
                    i.FilePath,
                    i.FileSize,
                    i.Rows,
                    i.Columns
                )).ToList();
        }

        public async Task<InstanceDto> GetInstanceByIdAsync(int instanceId)
        {
            var result = await _dicomRepository.GetInstanceByIdAsync(instanceId);
            if (result == null)
                throw new Exception($"Instance with id {instanceId} not found.");
            return new InstanceDto(
                result.Id,
                result.SopInstanceUid,
                result.InstanceNumber,
                result.FilePath,
                result.FileSize,
                result.Rows,
                result.Columns
            );
        }


        public async Task<Study?> GetStudyByInstanceUidAsync(string studyInstanceUid)
        {
            return await _dicomRepository.GetStudyByInstanceUidAsync(studyInstanceUid);
        }

        public async Task<Patient?> GetPatientByPatientIdAsync(string patientId)
        {
            return await _dicomRepository.GetPatientByPatientIdAsync(patientId);

        }

        public async Task<Instance?> GetInstanceBySobUIdAsync(string instanceSubUid)
        {
            return await _dicomRepository.GetInstanceBySobUIdAsync(instanceSubUid);
           
        }

        public async Task<Series?> GetSeriesByInstanceUIdAsync(string seriesInstanceUid)
        {
            return await  _dicomRepository.GetSeriesByInstanceUIdAsync(seriesInstanceUid);
           
        }

        public async Task AddPatientAsync(Patient patient)
        {
            if (patient is null)
                throw new ArgumentNullException(nameof(patient));

            await _dicomRepository.AddPatientAsync(patient);
        }
        public async Task AddSeriesAsync(Series series)
        {
            if (series is null)
                throw new ArgumentNullException(nameof(series));

            await _dicomRepository.AddSeriesAsync(series);
        }

        public async Task AddStudyAsync(Study study)
        {
            if (study is null)
                throw new ArgumentNullException(nameof(study));

            await _dicomRepository.AddStudyAsync(study);
        }

        public async Task AddInstanceAsync(Instance instance)
        {
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            await _dicomRepository.AddInstanceAsync(instance);
        }

        #endregion

        #region FileHandling

        public Task<DicomUploadResponseDto> ImportDicomFileAsync(Stream fileStream, string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]?> GetDicomFileAsync(int instanceId)
        {
            throw new NotImplementedException();
        }







        #endregion
    }
}
