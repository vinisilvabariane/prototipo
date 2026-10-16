import { supplierService } from "@/features/suppliers/services/SupplierService";

function parseId(value: string) {
  const id = Number(value);
  return Number.isInteger(id) && id > 0 ? id : null;
}

export async function GET(request: Request, ctx: RouteContext<"/suppliers/[id]/documents/[documentId]">) {
  const params = await ctx.params;
  const id = parseId(params.id);
  const documentId = parseId(params.documentId);

  if (id === null || documentId === null) return new Response("Documento não encontrado.", { status: 404 });

  const result = await supplierService.document(id, documentId);
  if (!result.ok) return new Response(result.error.messages.join("\n"), { status: result.error.status ?? 502 });

  const upstream = result.value;
  const headers = new Headers({ "cache-control": "private, no-store" });

  for (const name of ["content-type", "content-length"]) {
    const value = upstream.headers.get(name);
    if (value) headers.set(name, value);
  }

  const disposition = upstream.headers.get("content-disposition");
  if (disposition) {
    const download = new URL(request.url).searchParams.get("download") === "1";
    headers.set("content-disposition", download ? disposition.replace(/^inline/i, "attachment") : disposition);
  }

  return new Response(upstream.body, { status: 200, headers });
}
