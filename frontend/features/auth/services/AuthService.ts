import { BaseService } from "@/lib/http/BaseService";

type AuthResponse = { token: string; refreshToken: string; refreshExpires: string };

class AuthService extends BaseService {
  constructor() {
    super("/auth");
  }

  login(registration: number, password: string) {
    return this.post<AuthResponse>("", { registration, password }, { anonymous: true });
  }
}

export const authService = new AuthService();
