namespace Clenkasoft.DicomMedAppApi.Models;

public class Series
{
    public int Id { get; set; }
    
    // DICOM Tags
    public string SeriesInstanceUid { get; set; } = string.Empty;
    public string? Modality { get; set; } // CT, MR, US, CR, DX, etc.
    public int? SeriesNumber { get; set; }
    public string? SeriesDescription { get; set; }
    public string? BodyPartExamined { get; set; }
    public string? ProtocolName { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Foreign Key
    public int StudyId { get; set; }
    
    // Navigation Properties
    public Study Study { get; set; } = null!;
    public ICollection<Instance> Instances { get; set; } = new List<Instance>();
}
