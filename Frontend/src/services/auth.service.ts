import { Injectable } from "@angular/core";
import { jwtDecode, JwtPayload } from "jwt-decode";

@Injectable({
  providedIn: "root",
})
export class AuthService {
  private tokenKey = "authToken";
  private isAuthenticated: boolean | null = null;

  isLoggedIn() {
    if (this.isAuthenticated == null) {
      this.isAuthenticated = this.getToken() != null;
    }

    if (!this.isAuthenticated) {
      return false;
    }

    if (this.tokenIsActive()) {
      return true;
    }

    this.clearToken();
    return false;
  }

  setToken(token: string) {
    localStorage.setItem(this.tokenKey, token);
    this.isAuthenticated = true;
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  clearToken() {
    localStorage.removeItem(this.tokenKey);
    this.isAuthenticated = false;
  }

  tokenIsActive(): boolean {
    const token = this.getToken();
    const { exp } = jwtDecode<JwtPayload>(token ?? "");
    if (!exp) {
      return false;
    }
    if (Date.now() >= exp * 1000) {
      return false;
    }
    return true;
  }
}
