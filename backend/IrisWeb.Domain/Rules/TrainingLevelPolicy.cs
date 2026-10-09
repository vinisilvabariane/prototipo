using System.Globalization;
using System.Text;

namespace IrisWeb.Domain.Rules;

public enum TrainingStatus
{
    Unknown,
    Trained,
    Untrained,
}

/// <summary>
/// Treinado e o que o Sync chama de Qualificado ou Multiplicador (rank 3 e 4). "Sem
/// treinamento", "Nao treinado", "Nao qualificado" e "Em treinamento" nao sao. Sem nivel
/// gravado nao da para afirmar nada: fica Unknown, e a tela diz "nivel nao registrado".
/// </summary>
public static class TrainingLevelPolicy
{
    public static TrainingStatus Classify(string? level)
    {
        var folded = Fold(level);
        if (folded.Length == 0) return TrainingStatus.Unknown;

        if (folded == "4" || folded.Contains("MULTIPLICADOR")) return TrainingStatus.Trained;

        var negated = folded.Contains("NAO QUALIFICADO") || folded.Contains("NAO TREINADO") ||
                      folded.Contains("SEM TREINAMENTO") || folded.Contains("EM TREINAMENTO");
        if (negated) return TrainingStatus.Untrained;

        if (folded == "3" || folded.Contains("QUALIFICADO")) return TrainingStatus.Trained;

        return TrainingStatus.Untrained;
    }

    private static string Fold(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var decomposed = value.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        }
        return builder.ToString();
    }
}
