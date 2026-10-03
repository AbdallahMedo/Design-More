namespace MechanicalDesigns.Domain.Entities;
public sealed class ContactMessage : Entity
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Message { get; set; } = "";
}
