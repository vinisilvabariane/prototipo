namespace MeOne.Application.Interfaces.WorkInstructions;

public interface IWorkInstructionFileStorage
{
    Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken);

    Task<Stream?> OpenReadAsync(string storedName, CancellationToken cancellationToken);

    Task DeleteAsync(string storedName, CancellationToken cancellationToken);
}
