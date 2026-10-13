export const text = (formData: FormData, key: string) => String(formData.get(key) ?? "").trim();

export const secret = (formData: FormData, key: string) => String(formData.get(key) ?? "");

export function integer(formData: FormData, key: string) {
  const value = Number(text(formData, key));
  return Number.isInteger(value) && value > 0 ? value : 0;
}
