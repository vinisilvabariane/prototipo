using MeOne.Application.Interfaces.WorkInstructions;
using Microsoft.Extensions.Options;

namespace MeOne.Infrastructure.Storage;

public sealed class LocalWorkInstructionFileStorage(IOptions<WorkInstructionStorageOptions> options)
    : LocalFileStorage(options.Value.Path), IWorkInstructionFileStorage;
