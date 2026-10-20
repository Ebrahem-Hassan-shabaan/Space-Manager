namespace SpaceManager.Models;

public class Amenity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<WorkspaceAmenity> WorkspaceAmenities { get; set; } = new List<WorkspaceAmenity>();

}
