using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Tests.Domain;

public sealed class OeeFormulaTests
{
    private static readonly IReadOnlyDictionary<string, double> NoVariables = new Dictionary<string, double>();

    private static double Eval(string source, IReadOnlyDictionary<string, double>? variables = null)
    {
        var (expression, error) = OeeFormula.Parse(source);
        Assert.Null(error);
        return expression!.Evaluate(variables ?? NoVariables);
    }

    [Theory]
    [InlineData("1 + 2 * 3", 7)]
    [InlineData("(1 + 2) * 3", 9)]
    [InlineData("-2 + 5", 3)]
    [InlineData("-2 * 3", -6)]
    [InlineData("10 / 4", 2.5)]
    [InlineData("10 / 0", 0)]
    [InlineData("0 / 0", 0)]
    [InlineData("2 - 3 - 4", -5)]
    [InlineData("8 / 2 / 2", 2)]
    [InlineData("1.5 * 2", 3)]
    [InlineData("min(3, 1, 2)", 1)]
    [InlineData("max(3; 1; 2)", 3)]
    [InlineData("abs(-4)", 4)]
    [InlineData("clamp(15, 0, 10)", 10)]
    [InlineData("clamp(-5, 0, 10)", 0)]
    [InlineData("if(2 > 1, 10, 20)", 10)]
    [InlineData("if(2 < 1, 10, 20)", 20)]
    [InlineData("if(3 == 3, 1, 0)", 1)]
    [InlineData("if(3 != 3, 1, 0)", 0)]
    [InlineData("if(3 >= 3, 1, 0) + if(3 <= 2, 1, 0)", 1)]
    [InlineData("round(2.5)", 3)]
    [InlineData("MIN(4, 2)", 2)]
    public void Evaluates(string source, double expected) => Assert.Equal(expected, Eval(source), 9);

    [Fact]
    public void Variables_AreCaseInsensitiveAndMissingOnesAreZero()
    {
        var variables = new Dictionary<string, double> { ["real_production"] = 80, ["theoretical_production"] = 100 };

        Assert.Equal(0.8, Eval("Real_Production / theoretical_production", variables), 9);
        Assert.Equal(5, Eval("5 + unknown_value", variables), 9);
    }

    [Fact]
    public void Identifiers_AreCollected()
    {
        var (expression, _) = OeeFormula.Parse("if(a > 0, b / c, d) + a");

        Assert.Equal(["a", "b", "c", "d"], expression!.Identifiers.Order());
    }

    [Theory]
    [InlineData("", "Escreva a fórmula.")]
    [InlineData("   ", "Escreva a fórmula.")]
    [InlineData("1 +", "falta um valor")]
    [InlineData("(1 + 2", "Falta fechar o parêntese.")]
    [InlineData("1 + 2)", "Caractere inesperado")]
    [InlineData("1 $ 2", "Caractere inesperado")]
    [InlineData("foo(1)", "Função desconhecida")]
    [InlineData("min()", "recebe")]
    [InlineData("abs(1, 2)", "recebe 1 valor")]
    [InlineData("clamp(1, 2)", "recebe 3 valor")]
    [InlineData("min", "é uma função")]
    [InlineData("1.2.3 + 1", "Número inválido")]
    [InlineData("min(1 2)", "Esperava vírgula")]
    public void RejectsMalformedFormulas(string source, string messagePart)
    {
        var (expression, error) = OeeFormula.Parse(source);

        Assert.Null(expression);
        Assert.NotNull(error);
        Assert.Contains(messagePart, error!.Message);
    }

    [Fact]
    public void ErrorPosition_PointsToTheOffendingCharacter()
    {
        var (_, error) = OeeFormula.Parse("1 + 2 $ 3");

        Assert.Equal(6, error!.Position);
    }

    [Fact]
    public void RejectsFormulasThatAreTooLongOrTooDeep()
    {
        var (_, tooLong) = OeeFormula.Parse(new string('1', OeeErrors.MaxFormulaLength + 1));
        var (_, tooDeep) = OeeFormula.Parse(new string('(', 60) + "1" + new string(')', 60));

        Assert.Equal(OeeErrors.FormulaTooLong, tooLong!.Message);
        Assert.Contains("parênteses demais", tooDeep!.Message);
    }

    [Fact]
    public void Validate_RejectsVariablesOutsideTheScope()
    {
        var post = OeeCatalog.AllowedIn(OeeScope.PostPillar);

        Assert.Null(OeeFormula.Validate("real_production / theoretical_production", post));
        Assert.Contains("line_plan", OeeFormula.Validate("line_plan / 2", post)!.Message);
        Assert.Contains("availability", OeeFormula.Validate("availability", post)!.Message);
        Assert.Null(OeeFormula.Validate("availability * performance", OeeCatalog.AllowedIn(OeeScope.PostOee)));
    }

    [Fact]
    public void NonFiniteResults_BecomeZero()
    {
        Assert.Equal(0, Eval("x * x * x", new Dictionary<string, double> { ["x"] = 1e200 }));
    }

    [Fact]
    public void DefaultFormulas_AreValidInTheirOwnScope()
    {
        Assert.Null(OeeFormulas.Default.Check());
    }

    [Fact]
    public void Check_NamesTheSlotThatFailed()
    {
        var broken = OeeFormulas.Default with { LinePerformance = "availability * 2" };

        var message = broken.Check();

        Assert.NotNull(message);
        Assert.Contains("desempenho da linha", message);
    }

    [Fact]
    public void Catalog_ExposesEveryAggregateOfEveryPillar()
    {
        var line = OeeCatalog.AllowedIn(OeeScope.LinePillar);

        foreach (var pillar in OeeCatalog.AggregatedPillars)
        {
            foreach (var mode in OeeCatalog.AggregationModes)
                Assert.Contains(OeeCatalog.AggregateKey(pillar, mode), line);
        }

        Assert.DoesNotContain("real_production", line);
    }
}
