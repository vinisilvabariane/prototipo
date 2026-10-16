import { BaseService } from "@/lib/http/BaseService";
import { qs } from "@/lib/http/query";
import type {
  CopyAllocationsResult,
  CopyLayoutResult,
  LayoutDetail,
  LayoutList,
  LayoutQuery,
  LayoutSummary,
  LayoutTarget,
  ProductionLine,
  ProductionProduct,
  ProductionShift,
  RunCandidate,
  RunDetail,
  RunPage,
  RunQuery,
  RunSummary,
  SaveLayoutBody,
} from "../types";

type SetupResource = "lines" | "shifts" | "products";

class ProductionService extends BaseService {
  constructor() {
    super("/production");
  }

  lines(includeInactive = false) {
    return this.get<ProductionLine[]>(`/lines${qs({ includeInactive: includeInactive ? "true" : undefined })}`);
  }

  shifts(includeInactive = false) {
    return this.get<ProductionShift[]>(`/shifts${qs({ includeInactive: includeInactive ? "true" : undefined })}`);
  }

  products(includeInactive = false) {
    return this.get<ProductionProduct[]>(`/products${qs({ includeInactive: includeInactive ? "true" : undefined })}`);
  }

  createSetup(resource: SetupResource, body: unknown) {
    return this.post<unknown>(`/${resource}`, body);
  }

  updateSetup(resource: SetupResource, id: number, body: unknown) {
    return this.put(`/${resource}/${id}`, body);
  }

  layouts(query: LayoutQuery = {}) {
    return this.get<LayoutList>(`/layouts${qs(query)}`);
  }

  layout(id: number) {
    return this.get<LayoutDetail>(`/layouts/${id}`);
  }

  createLayout(target: LayoutTarget) {
    return this.post<LayoutSummary>("/layouts", target);
  }

  saveLayout(id: number, body: SaveLayoutBody) {
    return this.put(`/layouts/${id}`, body);
  }

  deleteLayout(id: number) {
    return this.del(`/layouts/${id}`);
  }

  copyLayout(id: number, targets: LayoutTarget[]) {
    return this.post<CopyLayoutResult>(`/layouts/${id}/copy`, { targets });
  }

  runs(query: RunQuery = {}) {
    return this.get<RunPage>(`/runs${qs(query)}`);
  }

  run(id: number) {
    return this.get<RunDetail>(`/runs/${id}`);
  }

  createRun(layoutId: number) {
    return this.post<RunSummary>("/runs", { layoutId });
  }

  candidates(runId: number, search: string) {
    return this.get<RunCandidate[]>(`/runs/${runId}/candidates${qs({ search })}`);
  }

  allocate(runId: number, postId: number, userId: number) {
    return this.post(`/runs/${runId}/posts/${postId}/allocation`, { userId });
  }

  release(runId: number, postId: number) {
    return this.del(`/runs/${runId}/posts/${postId}/allocation`);
  }

  releaseAll(runId: number) {
    return this.del(`/runs/${runId}/allocations`);
  }

  copyPrevious(runId: number) {
    return this.post<CopyAllocationsResult>(`/runs/${runId}/allocations/copy-previous`);
  }

  start(runId: number) {
    return this.post(`/runs/${runId}/start`);
  }

  close(runId: number) {
    return this.post(`/runs/${runId}/close`);
  }

  cancel(runId: number) {
    return this.del(`/runs/${runId}`);
  }

  recordOutput(runId: number, postId: number, quantity: number, hourStart: string | null) {
    return this.post(`/runs/${runId}/outputs`, { postId, quantity, hourStart });
  }
}

export const productionService = new ProductionService();
