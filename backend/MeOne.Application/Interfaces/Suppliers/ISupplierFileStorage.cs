namespace MeOne.Application.Interfaces.Suppliers;

public interface ISupplierFileStorage
{
    Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken);

    Task<Stream?> OpenReadAsync(string storedName, CancellationToken cancellationToken);

    Task DeleteAsync(string storedName, CancellationToken cancellationToken);
}
