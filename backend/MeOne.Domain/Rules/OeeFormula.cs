using System.Globalization;
using MeOne.Domain.Errors;

namespace MeOne.Domain.Rules;

public sealed record OeeFormulaError(string Message, int Position);

public sealed class OeeExpression
{
    private readonly Node _root;

    internal OeeExpression(string source, Node root, IReadOnlySet<string> identifiers)
    {
        Source = source;
        _root = root;
        Identifiers = identifiers;
    }

    public string Source { get; }

    public IReadOnlySet<string> Identifiers { get; }

    public double Evaluate(IReadOnlyDictionary<string, double> variables)
    {
        var value = _root.Evaluate(variables);

        return double.IsFinite(value) ? value : 0;
    }

    internal abstract class Node
    {
        public abstract double Evaluate(IReadOnlyDictionary<string, double> variables);
    }

    internal sealed class Number(double value) : Node
    {
        public override double Evaluate(IReadOnlyDictionary<string, double> variables) => value;
    }

    internal sealed class Variable(string name) : Node
    {
        public override double Evaluate(IReadOnlyDictionary<string, double> variables) =>
            variables.TryGetValue(name, out var value) ? value : 0;
    }

    internal sealed class Negate(Node operand) : Node
    {
        public override double Evaluate(IReadOnlyDictionary<string, double> variables) => -operand.Evaluate(variables);
    }

    internal sealed class Binary(char op, Node left, Node right) : Node
    {
        public override double Evaluate(IReadOnlyDictionary<string, double> variables)
        {
            var a = left.Evaluate(variables);
            var b = right.Evaluate(variables);

            return op switch
            {
                '+' => a + b,
                '-' => a - b,
                '*' => a * b,
                '/' => b == 0 ? 0 : a / b,
                _ => 0,
            };
        }
    }

    internal sealed class Compare(string op, Node left, Node right) : Node
    {
        public override double Evaluate(IReadOnlyDictionary<string, double> variables)
        {
            var a = left.Evaluate(variables);
            var b = right.Evaluate(variables);

            var result = op switch
            {
                "<" => a < b,
                ">" => a > b,
                "<=" => a <= b,
                ">=" => a >= b,
                "==" => Math.Abs(a - b) < 1e-9,
                "!=" => Math.Abs(a - b) >= 1e-9,
                _ => false,
            };

            return result ? 1 : 0;
        }
    }

    internal sealed class Call(string name, Node[] arguments) : Node
    {
        public override double Evaluate(IReadOnlyDictionary<string, double> variables)
        {
            var values = arguments.Select(argument => argument.Evaluate(variables)).ToArray();

            return name switch
            {
                "min" => values.Min(),
                "max" => values.Max(),
                "abs" => Math.Abs(values[0]),
                "clamp" => Math.Min(Math.Max(values[0], values[1]), values[2]),
                "if" => values[0] != 0 ? values[1] : values[2],
                "round" => values.Length == 2 ? Math.Round(values[0], (int)Math.Clamp(values[1], 0, 10), MidpointRounding.AwayFromZero) : Math.Round(values[0], MidpointRounding.AwayFromZero),
                _ => 0,
            };
        }
    }
}

public static class OeeFormula
{
    private const int MaxDepth = 40;

    private static readonly Dictionary<string, (int Min, int Max)> Functions = new()
    {
        ["min"] = (1, 16),
        ["max"] = (1, 16),
        ["abs"] = (1, 1),
        ["clamp"] = (3, 3),
        ["if"] = (3, 3),
        ["round"] = (1, 2),
    };

    public static IReadOnlyCollection<string> FunctionNames => Functions.Keys;

    public static (OeeExpression? Expression, OeeFormulaError? Error) Parse(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return (null, new(OeeErrors.FormulaRequired, 0));

        if (source.Length > OeeErrors.MaxFormulaLength)
            return (null, new(OeeErrors.FormulaTooLong, OeeErrors.MaxFormulaLength));

        var parser = new Parser(source);

        try
        {
            var root = parser.ParseExpression(0);
            parser.ExpectEnd();

            return (new OeeExpression(source, root, parser.Identifiers), null);
        }
        catch (FormulaSyntaxException exception)
        {
            return (null, new(exception.Message, exception.Position));
        }
    }

    public static OeeFormulaError? Validate(string? source, IReadOnlySet<string> allowedIdentifiers)
    {
        var (expression, error) = Parse(source);

        if (error is not null)
            return error;

        foreach (var identifier in expression!.Identifiers)
        {
            if (!allowedIdentifiers.Contains(identifier))
                return new($"\"{identifier}\" não existe neste tipo de fórmula.", IndexOfIdentifier(source!, identifier));
        }

        return null;
    }

    private static int IndexOfIdentifier(string source, string identifier)
    {
        var index = source.IndexOf(identifier, StringComparison.Ordinal);

        return index < 0 ? 0 : index;
    }

    private sealed class FormulaSyntaxException(string message, int position) : Exception(message)
    {
        public int Position { get; } = position;
    }

    private sealed class Parser(string source)
    {
        private int _index;

        public HashSet<string> Identifiers { get; } = [];

        public OeeExpression.Node ParseExpression(int depth)
        {
            if (depth > MaxDepth)
                throw new FormulaSyntaxException("A fórmula tem parênteses demais aninhados.", _index);

            var left = ParseAdditive(depth);
            SkipSpaces();

            foreach (var op in new[] { "<=", ">=", "==", "!=", "<", ">" })
            {
                if (!Match(op))
                    continue;

                _index += op.Length;
                var right = ParseAdditive(depth);
                return new OeeExpression.Compare(op, left, right);
            }

            return left;
        }

        public void ExpectEnd()
        {
            SkipSpaces();

            if (_index < source.Length)
                throw new FormulaSyntaxException($"Caractere inesperado \"{source[_index]}\".", _index);
        }

        private OeeExpression.Node ParseAdditive(int depth)
        {
            var left = ParseTerm(depth);

            while (true)
            {
                SkipSpaces();

                if (_index < source.Length && source[_index] is '+' or '-')
                {
                    var op = source[_index++];
                    left = new OeeExpression.Binary(op, left, ParseTerm(depth));
                    continue;
                }

                return left;
            }
        }

        private OeeExpression.Node ParseTerm(int depth)
        {
            var left = ParseUnary(depth);

            while (true)
            {
                SkipSpaces();

                if (_index < source.Length && source[_index] is '*' or '/')
                {
                    var op = source[_index++];
                    left = new OeeExpression.Binary(op, left, ParseUnary(depth));
                    continue;
                }

                return left;
            }
        }

        private OeeExpression.Node ParseUnary(int depth)
        {
            SkipSpaces();

            if (_index < source.Length && source[_index] == '-')
            {
                _index++;
                return new OeeExpression.Negate(ParseUnary(depth));
            }

            return ParsePrimary(depth);
        }

        private OeeExpression.Node ParsePrimary(int depth)
        {
            SkipSpaces();

            if (_index >= source.Length)
                throw new FormulaSyntaxException("A fórmula termina antes do esperado: falta um valor.", _index);

            var current = source[_index];

            if (current == '(')
            {
                _index++;
                var inner = ParseExpression(depth + 1);
                SkipSpaces();

                if (_index >= source.Length || source[_index] != ')')
                    throw new FormulaSyntaxException("Falta fechar o parêntese.", _index);

                _index++;
                return inner;
            }

            if (char.IsDigit(current) || current == '.')
                return ParseNumber();

            if (char.IsLetter(current) || current == '_')
                return ParseIdentifierOrCall(depth);

            throw new FormulaSyntaxException($"Caractere inesperado \"{current}\".", _index);
        }

        private OeeExpression.Node ParseNumber()
        {
            var start = _index;

            while (_index < source.Length && (char.IsDigit(source[_index]) || source[_index] == '.'))
                _index++;

            var text = source[start.._index];

            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                ? new OeeExpression.Number(value)
                : throw new FormulaSyntaxException($"Número inválido \"{text}\". Use ponto como separador decimal.", start);
        }

        private OeeExpression.Node ParseIdentifierOrCall(int depth)
        {
            var start = _index;

            while (_index < source.Length && (char.IsLetterOrDigit(source[_index]) || source[_index] == '_'))
                _index++;

            var name = source[start.._index].ToLowerInvariant();
            SkipSpaces();

            if (_index >= source.Length || source[_index] != '(')
            {
                if (Functions.ContainsKey(name))
                    throw new FormulaSyntaxException($"\"{name}\" é uma função: use {name}(...).", start);

                Identifiers.Add(name);
                return new OeeExpression.Variable(name);
            }

            if (!Functions.TryGetValue(name, out var arity))
                throw new FormulaSyntaxException($"Função desconhecida \"{name}\". Disponíveis: {string.Join(", ", Functions.Keys)}.", start);

            _index++;
            var arguments = new List<OeeExpression.Node>();
            SkipSpaces();

            if (_index < source.Length && source[_index] == ')')
            {
                _index++;
            }
            else
            {
                while (true)
                {
                    arguments.Add(ParseExpression(depth + 1));
                    SkipSpaces();

                    if (_index >= source.Length)
                        throw new FormulaSyntaxException($"Falta fechar o parêntese da função {name}.", _index);

                    if (source[_index] is ',' or ';')
                    {
                        _index++;
                        continue;
                    }

                    if (source[_index] == ')')
                    {
                        _index++;
                        break;
                    }

                    throw new FormulaSyntaxException($"Esperava vírgula ou parêntese em {name}(...), achei \"{source[_index]}\".", _index);
                }
            }

            if (arguments.Count < arity.Min || arguments.Count > arity.Max)
            {
                var expected = arity.Min == arity.Max ? $"{arity.Min}" : $"de {arity.Min} a {arity.Max}";
                throw new FormulaSyntaxException($"{name}(...) recebe {expected} valor(es), mas recebeu {arguments.Count}.", start);
            }

            return new OeeExpression.Call(name, [.. arguments]);
        }

        private bool Match(string text) => string.CompareOrdinal(source, _index, text, 0, text.Length) == 0;

        private void SkipSpaces()
        {
            while (_index < source.Length && char.IsWhiteSpace(source[_index]))
                _index++;
        }
    }
}
