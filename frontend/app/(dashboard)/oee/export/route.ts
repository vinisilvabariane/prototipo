import { comparisonCsv, fileSafe, runCsv } from "@/features/oee/export";
import { oeeService } from "@/features/oee/services/OeeService";

const HEADERS = (name: string) => ({
  "Content-Type": "text/csv; charset=utf-8",
  "Content-Disposition": `attachment; filename="${name}.csv"`,
  "Cache-Control": "no-store",
});

const failureResponse = (message: string, status: number) => new Response(message, { status, headers: { "Content-Type": "text/plain; charset=utf-8" } });

export async function GET(request: Request) {
  const params = new URL(request.url).searchParams;
  const runId = Number(params.get("runId") ?? 0);

  if (runId > 0) {
    const result = await oeeService.run(runId);
    if (!result.ok) return failureResponse("Não foi possível exportar esta produção.", result.error.code === "notFound" ? 404 : 502);

    const run = result.value;
    return new Response(runCsv(run), { headers: HEADERS(`oee-${fileSafe(run.lineName)}-${fileSafe(run.productCode)}-${run.runId}`) });
  }

  const from = params.get("from") ?? "";
  const to = params.get("to") ?? "";
  const result = await oeeService.runs({ from, to, lineId: params.get("lineId") ?? "", status: params.get("status") ?? "" });
  if (!result.ok) return failureResponse("Não foi possível exportar o comparativo.", result.error.code === "badRequest" ? 400 : 502);

  return new Response(comparisonCsv(result.value), { headers: HEADERS(`oee-comparativo-${fileSafe(from) || "periodo"}-${fileSafe(to) || "atual"}`) });
}
