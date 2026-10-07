namespace SpaceManager.Models;

public class WorkspaceType
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Workspace> Workspaces { get; set; } = new List<Workspace>();
}
