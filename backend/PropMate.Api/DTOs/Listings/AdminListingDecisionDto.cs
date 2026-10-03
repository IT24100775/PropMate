using System.ComponentModel.DataAnnotations;

namespace PropMate.Api.DTOs.Listings;

public class AdminListingDecisionDto
{
    [Required]
    [StringLength(500, MinimumLength = 3)]
    public string Reason { get; set; } = string.Empty;
}