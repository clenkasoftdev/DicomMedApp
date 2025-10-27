using Clenkasoft.DicomMedAppApi.Contracts;
using Clenkasoft.DicomMedAppApi.Infrastructure.Data;
using Clenkasoft.DicomMedAppApi.Models;
using Microsoft.EntityFrameworkCore;


namespace Clenkasoft.DicomMedAppApi.Repositories
{
    public class DicomRepository : IDicomRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DicomRepository> _logger;

        public DicomRepository(ApplicationDbContext context, IConfiguration configuration, ILogger<DicomRepository> _logger)
        {
            _context = context;
            this._logger = _logger;

        }

        public Task<Patient?> GetPatientAsync(int patientId)
        {
            return _context.Patients.Include(p => p.Studies).ThenInclude(s => s.Series).FirstOrDefaultAsync(p => p.Id == patientId);
        }

       
        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            return await _context.Patients.Select(p => new Patient
            {
                Id = p.Id,
                PatientId = p.PatientId,
                PatientName = p.PatientName,
                BirthDate = p.BirthDate,
                Gender = p.Gender,
                Studies = p.Studies,
            }).ToListAsync();

        }

       
        public async Task<List<Instance>> GetInstancesBySeriesAsync(int seriesId)
        {
            // Plan:
            // - Order by entity fields before projection (EF Core can translate this).
            // - Add ThenBy(i => i.Id) for deterministic ordering when InstanceNumber is null or duplicated.
            // - Use AsNoTracking() for read-only performance.
            var result = await _context.Instances
                .AsNoTracking()
                .Where(i => i.SeriesId == seriesId)
                .OrderBy(i => i.InstanceNumber)
                .ThenBy(i => i.Id)  
                .Select(i => new Instance
                {
                    Id = i.Id,
                    SopInstanceUid = i.SopInstanceUid,
                    SopClassUid = i.SopClassUid,
                    InstanceNumber = i.InstanceNumber,
                    FilePath = i.FilePath,
                    FileSize = i.FileSize,
                    Rows = i.Rows,
                    Columns = i.Columns,
                })
                .ToListAsync();

            return result;
        }

      

        public async Task<List<Series>> GetSeriesByStudyAsync(int studyId)
        {
           return await _context.Series
          .Include(s => s.Instances)
          .Where(s => s.StudyId == studyId)
          .Select(s => new Series
          {
            Id = s.Id,
            SeriesInstanceUid = s.SeriesInstanceUid,
            Modality = s.Modality,
            SeriesNumber = s.SeriesNumber,
            SeriesDescription = s.SeriesDescription,
            Instances = s.Instances,
          })
          .ToListAsync();
        }

        public async Task<List<Study>> GetStudiesByPatientAsync(int patientId)
        {
            return await _context.Studies
           .Include(s => s.Patient)
           .Include(s => s.Series)
           .Where(s => s.PatientId == patientId)
           .Select(s => new Study
           {
               Id = s.Id,
                StudyInstanceUid = s.StudyInstanceUid,
                StudyDate = s.StudyDate,
                StudyTime = s.StudyTime,
                StudyDescription = s.StudyDescription,
                AccessionNumber = s.AccessionNumber,
                Patient = s.Patient,
                ReferringPhysicianName = s.ReferringPhysicianName,
                Series = s.Series,
           }
            )
           .ToListAsync();
        }
        public async Task<Instance> GetInstanceByIdAsync(int instanceId)
        {
            return await _context.Instances.FirstAsync(i => i.Id == instanceId);
        }

        public Task<Patient?> GetPatientByPatientIdAsync(string patientId)
        {
            return _context.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId);
        }

        public async Task<Study> GetStudyByInstanceUidAsync(string studyInstanceUid)
        {
            return await _context.Studies.FirstAsync(s => s.StudyInstanceUid == studyInstanceUid);
        }
        public async Task<Instance> GetInstanceBySobUIdAsync(string instanceSubUid)
        {
            return await _context.Instances.FirstAsync(i => i.SopInstanceUid == instanceSubUid);
        }

        public async Task<Series> GetSeriesByInstanceUIdAsync(string seriesInstanceUid)
        {
            return await _context.Series.FirstAsync(i => i.SeriesInstanceUid == seriesInstanceUid);
        }

        public async Task AddPatientAsync(Patient patient)
        {
            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();
        }
        public async Task AddSeriesAsync(Series series)
        {
            await _context.Series.AddAsync(series);
            await _context.SaveChangesAsync();
        }

        public async Task AddStudyAsync(Study study)
        {

            await _context.Studies.AddAsync(study);
            await _context.SaveChangesAsync();
        }

        public async Task AddInstanceAsync(Instance instance)
        {
            await _context.Instances.AddAsync(instance);
            await _context.SaveChangesAsync();
        }
    }
}
