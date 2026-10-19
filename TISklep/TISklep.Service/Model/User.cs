namespace TISklep.Service.Model;

public class User
{
    public int Id { get; set; }
    public string KeycloakId { get; set; } = string.Empty; // Claim "sub" z tokenu JWT
    public string Username { get; set; } = string.Empty;   // Claim "preferred_username"
    public string Email { get; set; } = string.Empty;      // Claim "email"

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}