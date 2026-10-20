"use server";

import { revalidatePath } from "next/cache";
import { mutate } from "@/lib/actions/mutate";
import type { Result } from "@/lib/result";
import { helpdeskService } from "../services/HelpdeskService";
import type { Answers, TicketSummary } from "../types";

const refresh = (id: number) => ["/helpdesk/board", "/helpdesk/tickets", `/helpdesk/tickets/${id}`];

export async function openTicket(sectorId: number, answers: Answers): Promise<Result<TicketSummary>> {
  const result = await helpdeskService.openTicket(sectorId, answers);
  if (result.ok) {
    revalidatePath("/helpdesk/board");
    revalidatePath("/helpdesk/tickets");
  }
  return result;
}

export async function startTicket(id: number): Promise<Result> {
  return mutate(helpdeskService.start(id), ...refresh(id));
}

export async function resolveTicket(id: number, answers: Answers, note: string): Promise<Result> {
  return mutate(helpdeskService.resolve(id, answers, note), ...refresh(id), "/oee");
}

export async function rejectTicket(id: number, reason: string): Promise<Result> {
  return mutate(helpdeskService.reject(id, reason), ...refresh(id));
}
