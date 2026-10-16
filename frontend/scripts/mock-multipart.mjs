export function parseMultipart(buffer, contentType) {
  const boundary = /boundary=(?:"([^"]+)"|([^;]+))/i.exec(contentType ?? "");
  const mark = boundary?.[1] ?? boundary?.[2];
  const fields = [];
  if (!mark) return fields;

  const delimiter = Buffer.from(`--${mark}`);
  let start = buffer.indexOf(delimiter);

  while (start !== -1) {
    const next = buffer.indexOf(delimiter, start + delimiter.length);
    if (next === -1) break;

    const part = buffer.subarray(start + delimiter.length + 2, next - 2);
    const headerEnd = part.indexOf("\r\n\r\n");
    if (headerEnd !== -1) {
      const headers = part.subarray(0, headerEnd).toString();
      fields.push({
        name: /name="([^"]*)"/.exec(headers)?.[1],
        filename: /filename="([^"]*)"/.exec(headers)?.[1],
        type: /content-type:\s*(.+)/i.exec(headers)?.[1]?.trim(),
        content: part.subarray(headerEnd + 4),
      });
    }
    start = next;
  }

  return fields;
}
