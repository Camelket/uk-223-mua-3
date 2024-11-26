import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private tokenKey = 'authToken';
  private isAuthenticated : boolean | null = null; // Set this based on your authentication logic

  isLoggedIn() {
    if(this.isAuthenticated == null) {
      this.isAuthenticated = this.getToken() != null;
    }

    return this.isAuthenticated == true;
  }

  setToken(token: string) {
    localStorage.setItem(this.tokenKey, token); // Or sessionStorage
    this.isAuthenticated = true;
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey); // Or sessionStorage
  }

  clearToken() {
    localStorage.removeItem(this.tokenKey);
    this.isAuthenticated = false;
  }
}
