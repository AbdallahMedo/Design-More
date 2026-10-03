using System.ComponentModel.DataAnnotations;

namespace MechanicalDesigns.Application.Contact;

public sealed class ContactRequest
{
    [Required, StringLength(150)]
    public string FullName { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "Enter an 11-digit Egyptian mobile number starting with 010, 011, 012, or 015.")]
    public string PhoneNumber { get; init; } = string.Empty;

    [Required, StringLength(200), RegularExpression(@"^[^\r\n]+$", ErrorMessage = "Subject must be a single line.")]
    public string Subject { get; init; } = string.Empty;

    [Required, StringLength(5000)]
    public string Message { get; init; } = string.Empty;
}
