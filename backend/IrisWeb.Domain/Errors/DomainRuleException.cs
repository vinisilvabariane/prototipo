namespace IrisWeb.Domain.Errors;

public sealed class DomainRuleException(string message) : InvalidOperationException(message);
