namespace MeOne.Domain.Rules;

public enum OeeScope
{
    PostPillar,
    PostOee,
    LinePillar,
    LineOee,
}

public sealed record OeeVariable(string Key, string Label, string Description, string Unit, IReadOnlyList<OeeScope> Scopes);

public static class OeeCatalog
{
    public static readonly string[] Pillars = ["availability", "performance", "quality"];

    public static readonly string[] AggregatedPillars = ["availability", "performance", "quality", "machine_oee"];

    public static readonly string[] AggregationModes = ["average", "sum", "product", "last_post"];

    private static readonly OeeScope[] PostData = [OeeScope.PostPillar, OeeScope.PostOee];

    private static readonly OeeScope[] LineData = [OeeScope.LinePillar, OeeScope.LineOee];

    public static IReadOnlyList<OeeVariable> Variables { get; } = Build();

    public static IReadOnlySet<string> AllowedIn(OeeScope scope) =>
        Variables.Where(variable => variable.Scopes.Contains(scope)).Select(variable => variable.Key).ToHashSet();

    public static string AggregateKey(string pillar, string mode) => $"{pillar}_{mode}";

    private static List<OeeVariable> Build()
    {
        var list = new List<OeeVariable>
        {
            new("elapsed_seconds", "Tempo decorrido", "Segundos entre o início da produção e agora (ou o encerramento).", "s", PostData),
            new("oee_time_rate_seconds", "Tempo de referência do turno", "Base de tempo do turno. Hoje é igual ao tempo decorrido.", "s", PostData),
            new("planned_stop_seconds", "Parada planejada", "Segundos de paradas planejadas. Ainda não há pausas cadastradas, então vale 0.", "s", PostData),
            new("occupied_seconds", "Tempo com gente no posto", "Segundos em que alguém estava alocado no posto.", "s", PostData),
            new("downtime_availability", "Parada que afeta a disponibilidade", "Segundos de parada marcados como perda de disponibilidade. Vem do help desk.", "s", PostData),
            new("downtime_performance", "Parada que afeta o desempenho", "Segundos de parada marcados como perda de desempenho (microparadas). Vem do help desk.", "s", PostData),
            new("total_downtime", "Parada total", "Soma de todas as paradas do posto. Vem do help desk.", "s", PostData),
            new("real_downtime", "Parada real", "Segundos de parada medidos pela máquina. Vem do help desk.", "s", PostData),
            new("ticket_downtime_proportional", "Parada de chamados", "Segundos de parada abertos por chamados. Vem do help desk.", "s", PostData),
            new("ticket_downtime_pieces", "Peças perdidas por chamados", "Meta por hora × parada de chamados, em peças.", "pç", PostData),
            new("real_production", "Produzido", "Soma do que foi lançado no posto no período.", "pç", PostData),
            new("theoretical_production", "Produção teórica", "Meta por hora × horas decorridas.", "pç", PostData),
            new("expected_production", "Produção esperada", "Meta por hora × horas com gente no posto.", "pç", PostData),
            new("expected_production_min", "Meta mínima", "Produção esperada × 0,9.", "pç", PostData),
            new("max_rate_hour", "Capacidade máxima", "Meta por hora × horas decorridas (igual à teórica por enquanto).", "pç", PostData),
            new("defects", "Defeitos", "Peças com defeito de qualidade no período. Vem do help desk; defeito sem posto conta no último posto medido.", "pç", PostData),
            new("repair_defects", "Defeitos de retrabalho", "Peças com defeito encontrado no reparo. Vem do help desk.", "pç", PostData),
            new("availability", "Disponibilidade", "Pilar de disponibilidade já calculado (razão de 0 a 1).", "razão", [OeeScope.PostOee, OeeScope.LineOee]),
            new("performance", "Desempenho", "Pilar de desempenho já calculado (razão de 0 a 1).", "razão", [OeeScope.PostOee, OeeScope.LineOee]),
            new("quality", "Qualidade", "Pilar de qualidade já calculado (razão de 0 a 1).", "razão", [OeeScope.PostOee, OeeScope.LineOee]),
            new("machine_oee", "OEE médio dos postos", "Média do OEE dos postos medidos (razão de 0 a 1).", "razão", [OeeScope.LineOee]),
            new("line_plan", "Plano da linha", "Produção teórica do último posto da linha.", "pç", LineData),
            new("line_produced", "Produzido na linha", "Soma do produzido em todos os postos medidos.", "pç", LineData),
            new("line_produced_station", "Produzido no último posto", "Produzido no último posto medido do fluxo.", "pç", LineData),
            new("line_elapsed", "Tempo decorrido da linha", "Segundos da produção.", "s", LineData),
            new("line_downtime_availability", "Paradas de disponibilidade da linha", "Soma nos postos medidos.", "s", LineData),
            new("line_downtime_performance", "Paradas de desempenho da linha", "Soma nos postos medidos.", "s", LineData),
            new("line_total_downtime", "Parada total da linha", "Soma nos postos medidos.", "s", LineData),
            new("line_defects", "Defeitos da linha", "Soma nos postos medidos.", "pç", LineData),
        };

        foreach (var pillar in AggregatedPillars)
        {
            foreach (var mode in AggregationModes)
            {
                var label = pillar switch
                {
                    "availability" => "disponibilidade",
                    "performance" => "desempenho",
                    "quality" => "qualidade",
                    _ => "OEE do posto",
                };

                var modeLabel = mode switch
                {
                    "average" => "média",
                    "sum" => "soma",
                    "product" => "produto",
                    _ => "último posto",
                };

                var scopes = pillar == "machine_oee" ? new[] { OeeScope.LineOee, OeeScope.LinePillar } : [OeeScope.LinePillar, OeeScope.LineOee];

                list.Add(new(
                    AggregateKey(pillar, mode),
                    $"{char.ToUpperInvariant(label[0])}{label[1..]} ({modeLabel})",
                    $"Agrega a {label} dos postos medidos pela {modeLabel}. Razão de 0 a 1.",
                    "razão",
                    scopes));
            }
        }

        return list;
    }
}
