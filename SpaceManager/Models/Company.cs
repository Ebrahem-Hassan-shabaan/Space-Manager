namespace SpaceManager.Models;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TaxRegistrationNumber { get; set; } 
    public string BillingAddress { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string ContactEmail { get; set; }
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();    

}
