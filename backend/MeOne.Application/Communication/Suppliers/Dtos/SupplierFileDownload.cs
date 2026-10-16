namespace MeOne.Application.Communication.Suppliers.Dtos;

public sealed record SupplierFileDownload(Stream Content, string FileName, string ContentType);
