import { workflowService } from "@/features/workflows/services/WorkflowService";

function parseId(value: string) {
  const id = Number(value);
  return Number.isInteger(id) && id > 0 ? id : null;
}

export async function GET(_request: Request, ctx: RouteContext<"/workflows/[id]/attachments/[attachmentId]">) {
  const params = await ctx.params;
  const id = parseId(params.id);
  const attachmentId = parseId(params.attachmentId);

  if (id === null || attachmentId === null) return new Response("Anexo não encontrado.", { status: 404 });

  const result = await workflowService.attachment(id, attachmentId);
  if (!result.ok) return new Response(result.error.messages.join("\n"), { status: result.error.status ?? 502 });

  const upstream = result.value;
  const headers = new Headers();
  for (const name of ["content-type", "content-disposition", "content-length"]) {
    const value = upstream.headers.get(name);
    if (value) headers.set(name, value);
  }
  headers.set("cache-control", "private, no-store");

  return new Response(upstream.body, { status: 200, headers });
}
