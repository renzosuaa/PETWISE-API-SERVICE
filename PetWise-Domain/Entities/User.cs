namespace PetWise.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Nickname { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool HasCompletedSetup { get; set; }
}