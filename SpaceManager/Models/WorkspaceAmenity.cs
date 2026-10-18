namespace SpaceManager.Models;

public class WorkspaceAmenity
{
    public int WorkspaceId { get; set; }
    public Workspace Workspace { get; set; }
    public int AmenityId { get; set; }
    public Amenity Amenity { get; set; }
    public bool IsOperational { get; set; } 
}
