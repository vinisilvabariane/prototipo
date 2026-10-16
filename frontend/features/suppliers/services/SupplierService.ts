import { BaseService } from "@/lib/http/BaseService";
import { qs } from "@/lib/http/query";
import type {
  SupplierBody,
  SupplierDetail,
  SupplierDocument,
  SupplierItemRoute,
  SupplierPage,
  SupplierQuery,
  SupplierSummary,
  SupplierUser,
} from "../types";

class SupplierService extends BaseService {
  constructor() {
    super("/suppliers");
  }

  list(query: SupplierQuery = {}) {
    return this.get<SupplierPage>(qs(query));
  }

  users() {
    return this.get<SupplierUser[]>("/users");
  }

  getById(id: number) {
    return this.get<SupplierDetail>(`/${id}`);
  }

  create(body: SupplierBody) {
    return this.post<SupplierSummary>("", body);
  }

  update(id: number, body: SupplierBody) {
    return this.put(`/${id}`, body);
  }

  addItem(id: number, route: SupplierItemRoute, body: unknown) {
    return this.post(`/${id}/${route}`, body);
  }

  updateItem(id: number, route: SupplierItemRoute, itemId: number, body: unknown) {
    return this.put(`/${id}/${route}/${itemId}`, body);
  }

  removeItem(id: number, route: SupplierItemRoute, itemId: number) {
    return this.del(`/${id}/${route}/${itemId}`);
  }

  addDocument(id: number, form: FormData) {
    return this.postForm<SupplierDocument>(`/${id}/documents`, form);
  }

  removeDocument(id: number, documentId: number) {
    return this.del(`/${id}/documents/${documentId}`);
  }

  document(id: number, documentId: number) {
    return this.stream(`/${id}/documents/${documentId}/file`);
  }
}

export const supplierService = new SupplierService();
