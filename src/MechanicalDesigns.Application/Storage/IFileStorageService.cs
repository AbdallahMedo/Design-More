namespace MechanicalDesigns.Application.Storage;
public interface IFileStorageService{Task<string> SaveImageAsync(Stream content,string fileName,string contentType,CancellationToken ct);Task DeleteAsync(string url,CancellationToken ct);}
