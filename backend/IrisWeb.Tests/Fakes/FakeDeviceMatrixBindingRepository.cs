using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Tests.Fakes;

public sealed class FakeDeviceMatrixBindingRepository
    : InMemoryRepository<DeviceMatrixBinding, string>, IDeviceMatrixBindingRepository
{
}
