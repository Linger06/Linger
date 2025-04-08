using System.ComponentModel.DataAnnotations;

namespace Linger.API.Models;

public class UserUpdateRequest
{
    [Required]
    public required string Id { get; set; }
    
    [Required]
    public required string Name { get; set; }
    
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}
