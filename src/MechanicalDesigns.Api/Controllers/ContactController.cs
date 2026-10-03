using MechanicalDesigns.Application.Contact;
using MechanicalDesigns.Application.Email;
using MechanicalDesigns.Domain.Entities;
using MechanicalDesigns.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MechanicalDesigns.Api.Controllers;
[ApiController]
[AllowAnonymous]
[Route("api/contact")]
public sealed class ContactController(IEmailService email, IConfiguration configuration, ApplicationDbContext db, ILogger<ContactController> logger) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(32_768)]
    public async Task<IActionResult> Send(ContactRequest request, CancellationToken cancellationToken)
    {
        var message = new ContactMessage {
            FullName = request.FullName.Trim(), Email = request.Email.Trim(), PhoneNumber = request.PhoneNumber,
            Subject = request.Subject.Trim(), Message = request.Message.Trim()
        };
        db.ContactMessages.Add(message);
        await db.SaveChangesAsync(cancellationToken);
        var emailSent = false;
        try
        {
            var recipient = configuration["Contact:RecipientEmail"];
            if (string.IsNullOrWhiteSpace(recipient)) throw new InvalidOperationException("Configure Contact:RecipientEmail.");
            var body = $"Full name: {message.FullName}\nEmail: {message.Email}\nEgyptian mobile: {message.PhoneNumber}\nSubject: {message.Subject}\n\n{message.Message}";
            await email.SendAsync(recipient, $"Website inquiry: {message.Subject}", body, cancellationToken);
            emailSent = true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Email notification failed for saved contact message {MessageId}", message.Id);
        }
        return Ok(new { success = true, emailSent, message = "Your message has been received successfully." });
    }
}
