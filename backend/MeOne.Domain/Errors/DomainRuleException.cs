namespace MeOne.Domain.Errors;

public sealed class DomainRuleException(string message) : InvalidOperationException(message);
