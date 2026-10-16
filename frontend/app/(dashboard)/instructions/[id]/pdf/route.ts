import { instructionService } from "@/features/instructions/services/InstructionService";

const parse = (value: string) => {
  const id = Number(value);
  return Number.isInteger(id) && id > 0 ? id : null;
};

export async function GET(request: Request, ctx: RouteContext<"/instructions/[id]/pdf">) {
  const id = parse((await ctx.params).id);
  if (id === null) return new Response("Instrução não encontrada.", { status: 404 });

  const result = await instructionService.pdf(id);
  if (!result.ok) return new Response(result.error.messages.join("\n"), { status: result.error.status ?? 502 });

  const upstream = result.value;
  const headers = new Headers({ "cache-control": "private, no-store" });
  headers.set("content-type", upstream.headers.get("content-type") ?? "application/pdf");

  const disposition = upstream.headers.get("content-disposition");
  if (disposition) {
    const download = new URL(request.url).searchParams.get("download") === "1";
    headers.set("content-disposition", download ? disposition.replace(/^inline/i, "attachment") : disposition);
  }

  return new Response(upstream.body, { status: 200, headers });
}
