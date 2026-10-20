"use server";

import { revalidatePath } from "next/cache";
import { mutate } from "@/lib/actions/mutate";
import type { Result } from "@/lib/result";
import { helpdeskService } from "../services/HelpdeskService";
import type { FieldTemplate, SaveSectorBody, SaveTemplateBody, SectorDetail } from "../types";

const refreshSectors = ["/helpdesk", "/helpdesk/sectors"];

export async function createSector(body: SaveSectorBody): Promise<Result<SectorDetail>> {
  const result = await helpdeskService.createSector(body);
  if (result.ok) for (const path of refreshSectors) revalidatePath(path);
  return result;
}

export async function updateSector(id: number, body: SaveSectorBody): Promise<Result> {
  return mutate(helpdeskService.updateSector(id, body), ...refreshSectors, `/helpdesk/sectors/${id}`, `/helpdesk/new/${id}`);
}

export async function createTemplate(body: SaveTemplateBody): Promise<Result<FieldTemplate>> {
  return helpdeskService.createTemplate(body);
}

export async function updateTemplate(id: number, body: SaveTemplateBody): Promise<Result> {
  return mutate(helpdeskService.updateTemplate(id, body));
}

export async function deleteTemplate(id: number): Promise<Result> {
  return mutate(helpdeskService.deleteTemplate(id));
}
