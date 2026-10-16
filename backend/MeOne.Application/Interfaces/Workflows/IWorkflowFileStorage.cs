namespace MeOne.Application.Interfaces.Workflows;

public interface IWorkflowFileStorage
{
    Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken);

    Task<Stream?> OpenReadAsync(string storedName, CancellationToken cancellationToken);

    Task DeleteAsync(string storedName, CancellationToken cancellationToken);
}
