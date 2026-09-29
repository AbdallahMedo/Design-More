using MechanicalDesigns.Application.Storage;
namespace MechanicalDesigns.Infrastructure.Storage;
public sealed class LocalFileStorageService:IFileStorageService{
 private static string Root=>Path.Combine(AppContext.BaseDirectory,"wwwroot","uploads");
 public async Task<string>SaveImageAsync(Stream content,string fileName,string contentType,CancellationToken ct){var ext=Path.GetExtension(fileName).ToLowerInvariant();if(ext is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")throw new ArgumentException("Only JPG, PNG, and WebP images are allowed.");Directory.CreateDirectory(Root);var safe=$"{Guid.NewGuid():N}{ext}";await using var output=File.Create(Path.Combine(Root,safe));await content.CopyToAsync(output,ct);return $"/uploads/{safe}";}
 public Task DeleteAsync(string url,CancellationToken ct){if(url.StartsWith("/uploads/",StringComparison.OrdinalIgnoreCase)){var path=Path.Combine(Root,Path.GetFileName(url));if(File.Exists(path))File.Delete(path);}return Task.CompletedTask;}}
