import { instructionService } from "@/features/instructions/services/InstructionService";

const parse = (value: string) => {
  const id = Number(value);
  return Number.isInteger(id) && id > 0 ? id : null;
};

export async function GET(_request: Request, ctx: RouteContext<"/instructions/[id]/steps/[stepId]/image">) {
  const params = await ctx.params;
  const id = parse(params.id);
  const stepId = parse(params.stepId);
  if (id === null || stepId === null) return new Response("Imagem não encontrada.", { status: 404 });

  const result = await instructionService.stepImage(id, stepId);
  if (!result.ok) return new Response(result.error.messages.join("\n"), { status: result.error.status ?? 502 });

  const upstream = result.value;
  const headers = new Headers({ "cache-control": "private, max-age=60" });
  headers.set("content-type", upstream.headers.get("content-type") ?? "application/octet-stream");

  return new Response(upstream.body, { status: 200, headers });
}
