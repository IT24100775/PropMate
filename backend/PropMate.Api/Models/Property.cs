namespace PropMate.Api.Models;

public class Property
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsAvailable { get; set; }
}
