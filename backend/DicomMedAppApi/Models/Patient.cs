namespace Clenkasoft.DicomMedAppApi.Models;

public class Patient
{
    public int Id { get; set; }
    
    // DICOM Tags
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation Properties
    public ICollection<Study> Studies { get; set; } = new List<Study>();
}
