namespace TISklep.Service.Model;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public ICollection<BoardGame> BoardGames { get; set; } = new List<BoardGame>();
}