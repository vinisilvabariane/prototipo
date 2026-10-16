namespace MeOne.Application.Communication.Suppliers.Dtos;

public sealed record SupplierFileUpload(string FileName, long Length, Stream Content);
