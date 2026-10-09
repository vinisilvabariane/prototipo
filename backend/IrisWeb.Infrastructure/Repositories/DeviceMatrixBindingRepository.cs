using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Infrastructure.Data.Context;

namespace IrisWeb.Infrastructure.Repositories;

public sealed class DeviceMatrixBindingRepository(IrisWebDbContext context)
    : Repository<DeviceMatrixBinding, string>(context), IDeviceMatrixBindingRepository
{
}
