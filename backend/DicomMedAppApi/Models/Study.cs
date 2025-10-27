namespace Clenkasoft.DicomMedAppApi.Models;

public class Study
{
    public int Id { get; set; }
    
    // DICOM Tags
    public string StudyInstanceUid { get; set; } = string.Empty;
    public DateTime? StudyDate { get; set; }
    public TimeSpan? StudyTime { get; set; }
    public string? StudyDescription { get; set; }
    public string? AccessionNumber { get; set; }
    public string? ReferringPhysicianName { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Foreign Key
    public int PatientId { get; set; }
    
    // Navigation Properties
    public Patient Patient { get; set; } = null!;
    public ICollection<Series> Series { get; set; } = new List<Series>();
}
