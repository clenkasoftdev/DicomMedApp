namespace Clenkasoft.DicomMedAppApi.Models;

public class Instance
{
    public int Id { get; set; }
    
    // DICOM Tags
    public string SopInstanceUid { get; set; } = string.Empty;
    public string? SopClassUid { get; set; }
    public int? InstanceNumber { get; set; }
    
    // File Information
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? TransferSyntaxUid { get; set; }
    
    // Image Information (if applicable)
    public int? Rows { get; set; }
    public int? Columns { get; set; }
    public int? BitsAllocated { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Foreign Key
    public int SeriesId { get; set; }
    
    // Navigation Properties
    public Series Series { get; set; } = null!;
}
