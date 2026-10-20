using System.Globalization;
using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Errors;

namespace MeOne.Domain.Rules;

public sealed record HelpdeskAnswerResult(string? Problem, IReadOnlyDictionary<string, string> Answers)
{
    public bool Failed => Problem is not null;
}

public sealed record HelpdeskSpot(ulong? LineId, ulong? PostId, bool Stopped, DateTime? StoppedAt, int Defects);

public static class HelpdeskAnswers
{
    public const string PostSuffix = "_post";
    public const string TimeSuffix = "_time";
    public const string Stopped = "stopped";
    public const string Running = "running";

    private static readonly TimeSpan FutureTolerance = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MaxStopAge = TimeSpan.FromDays(7);

    public static HelpdeskAnswerResult Normalize(HelpdeskForm form, IReadOnlyDictionary<string, string?> raw, DateTime now)
    {
        var provided = raw
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
            .ToDictionary(pair => pair.Key, pair => pair.Value!.Trim());

        var answers = new Dictionary<string, string>();

        foreach (var field in HelpdeskFormRules.Visible(form, provided))
        {
            provided.TryGetValue(field.Id, out var value);

            if (string.IsNullOrEmpty(value))
            {
                if (field.Required)
                    return new(HelpdeskErrors.FieldRequired(field.Label), answers);

                continue;
            }

            var problem = field.Type switch
            {
                HelpdeskFieldType.Text => LengthProblem(value, HelpdeskErrors.MaxTextLength),
                HelpdeskFieldType.TextArea => LengthProblem(value, HelpdeskErrors.MaxTextAreaLength),
                HelpdeskFieldType.Number => NumberProblem(value),
                HelpdeskFieldType.Select or HelpdeskFieldType.CardRadio => field.Options.Any(option => option.Value == value) ? null : "escolha uma das opções.",
                HelpdeskFieldType.ProductionSpot => PositiveProblem(value, "selecione a linha."),
                HelpdeskFieldType.LineStop => value is Stopped or Running ? null : "informe se a linha está parada.",
                HelpdeskFieldType.Defects => DefectsProblem(value),
                _ => null,
            };

            if (problem is not null)
                return new(HelpdeskErrors.AnswerInvalid(field.Label, problem), answers);

            answers[field.Id] = value;

            if (field.Type == HelpdeskFieldType.ProductionSpot
                && provided.TryGetValue(field.Id + PostSuffix, out var post))
            {
                if (PositiveProblem(post, "selecione o posto.") is { } postProblem)
                    return new(HelpdeskErrors.AnswerInvalid(field.Label, postProblem), answers);

                answers[field.Id + PostSuffix] = post;
            }

            if (field.Type == HelpdeskFieldType.LineStop && value == Stopped)
            {
                if (!provided.TryGetValue(field.Id + TimeSuffix, out var time))
                    time = now.ToString("O", CultureInfo.InvariantCulture);

                if (!DateTime.TryParse(time, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var stoppedAt))
                    return new(HelpdeskErrors.AnswerInvalid(field.Label, "informe um horário válido."), answers);

                if (stoppedAt > now + FutureTolerance)
                    return new(HelpdeskErrors.AnswerInvalid(field.Label, "o horário da parada não pode estar no futuro."), answers);

                if (now - stoppedAt > MaxStopAge)
                    return new(HelpdeskErrors.AnswerInvalid(field.Label, "o horário da parada é antigo demais (máximo 7 dias)."), answers);

                answers[field.Id + TimeSuffix] = DateTime.SpecifyKind(stoppedAt, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture);
            }
        }

        return new(null, answers);
    }

    public static HelpdeskSpot ReadSpot(HelpdeskForm form, IReadOnlyDictionary<string, string> answers)
    {
        ulong? lineId = null;
        ulong? postId = null;
        var stopped = false;
        DateTime? stoppedAt = null;
        var defects = 0;

        if (form.OfType(HelpdeskFieldType.ProductionSpot) is { } spot && answers.TryGetValue(spot.Id, out var line)
            && ulong.TryParse(line, CultureInfo.InvariantCulture, out var parsedLine))
        {
            lineId = parsedLine;

            if (answers.TryGetValue(spot.Id + PostSuffix, out var post) && ulong.TryParse(post, CultureInfo.InvariantCulture, out var parsedPost))
                postId = parsedPost;
        }

        if (form.OfType(HelpdeskFieldType.LineStop) is { } stop && answers.TryGetValue(stop.Id, out var state) && state == Stopped)
        {
            stopped = true;

            if (answers.TryGetValue(stop.Id + TimeSuffix, out var time)
                && DateTime.TryParse(time, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var parsedTime))
                stoppedAt = DateTime.SpecifyKind(parsedTime, DateTimeKind.Utc);
        }

        if (form.OfType(HelpdeskFieldType.Defects) is { } defect && answers.TryGetValue(defect.Id, out var quantity)
            && int.TryParse(quantity, CultureInfo.InvariantCulture, out var parsedQuantity))
            defects = parsedQuantity;

        return new(lineId, postId, stopped, stoppedAt, defects);
    }

    private static string? LengthProblem(string value, int max) =>
        value.Length > max ? $"use no máximo {max} caracteres." : null;

    private static string? NumberProblem(string value) =>
        decimal.TryParse(value.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var number) && Math.Abs(number) <= 1_000_000_000m
            ? null
            : "informe um número válido.";

    private static string? PositiveProblem(string value, string message) =>
        ulong.TryParse(value, CultureInfo.InvariantCulture, out var number) && number > 0 ? null : message;

    private static string? DefectsProblem(string value) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var quantity) && quantity <= HelpdeskErrors.MaxDefects
            ? null
            : $"informe um número inteiro de 0 a {HelpdeskErrors.MaxDefects}.";
}
