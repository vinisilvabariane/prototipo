import { BaseService } from "@/lib/http/BaseService";
import { qs } from "@/lib/http/query";
import type {
  Answers,
  FieldTemplate,
  HelpdeskRole,
  Location,
  SaveSectorBody,
  SaveTemplateBody,
  SectorDetail,
  SectorList,
  TicketDetail,
  TicketPage,
  TicketQuery,
  TicketSummary,
} from "../types";

class HelpdeskService extends BaseService {
  constructor() {
    super("/helpdesk");
  }

  sectors(includeInactive = false) {
    return this.get<SectorList>(`/sectors${qs({ includeInactive: includeInactive ? "true" : undefined })}`);
  }

  sector(id: number) {
    return this.get<SectorDetail>(`/sectors/${id}`);
  }

  createSector(body: SaveSectorBody) {
    return this.post<SectorDetail>("/sectors", body);
  }

  updateSector(id: number, body: SaveSectorBody) {
    return this.put(`/sectors/${id}`, body);
  }

  templates() {
    return this.get<FieldTemplate[]>("/field-templates");
  }

  createTemplate(body: SaveTemplateBody) {
    return this.post<FieldTemplate>("/field-templates", body);
  }

  updateTemplate(id: number, body: SaveTemplateBody) {
    return this.put(`/field-templates/${id}`, body);
  }

  deleteTemplate(id: number) {
    return this.del(`/field-templates/${id}`);
  }

  roles() {
    return this.get<HelpdeskRole[]>("/roles");
  }

  locations() {
    return this.get<Location[]>("/locations");
  }

  tickets(query: TicketQuery = {}) {
    return this.get<TicketPage>(`/tickets${qs(query)}`);
  }

  ticket(id: number) {
    return this.get<TicketDetail>(`/tickets/${id}`);
  }

  openTicket(sectorId: number, answers: Answers) {
    return this.post<TicketSummary>("/tickets", { sectorId, answers });
  }

  start(id: number) {
    return this.post(`/tickets/${id}/start`);
  }

  resolve(id: number, answers: Answers, note: string) {
    return this.post(`/tickets/${id}/resolve`, { answers, note });
  }

  reject(id: number, reason: string) {
    return this.post(`/tickets/${id}/reject`, { reason });
  }
}

export const helpdeskService = new HelpdeskService();
