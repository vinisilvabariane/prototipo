namespace MeOne.Domain.Errors;

public static class PagingErrors
{
    public const int MaxPageSize = 100;
    public const int MaxSearchLength = 255;

    public const string PageNegative = "A página começa em 0 e não pode ser negativa.";

    public static string PageSizeOutOfRange => $"O tamanho da página deve ser entre 1 e {MaxPageSize}.";

    public static string SearchTooLong => $"A busca não pode passar de {MaxSearchLength} caracteres.";
}
