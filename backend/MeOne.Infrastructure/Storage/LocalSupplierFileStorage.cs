using MeOne.Application.Interfaces.Suppliers;
using Microsoft.Extensions.Options;

namespace MeOne.Infrastructure.Storage;

public sealed class LocalSupplierFileStorage(IOptions<SupplierStorageOptions> options)
    : LocalFileStorage(options.Value.Path), ISupplierFileStorage;
