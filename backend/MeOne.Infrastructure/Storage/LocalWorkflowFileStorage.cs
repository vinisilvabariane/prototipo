using MeOne.Application.Interfaces.Workflows;
using Microsoft.Extensions.Options;

namespace MeOne.Infrastructure.Storage;

public sealed class LocalWorkflowFileStorage(IOptions<WorkflowStorageOptions> options)
    : LocalFileStorage(options.Value.Path), IWorkflowFileStorage;
