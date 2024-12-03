import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Observable } from "rxjs";
import { AuthService } from "./auth.service";
import { Ledger, LedgerRequest } from "../models/model";

@Injectable({
  providedIn: "root",
})
export class LedgerService {
  private apiUrl = "https://localhost:44386/api/v1";

  constructor(private http: HttpClient, private authService: AuthService) {}

  //TODO: correct methods to use ledger arg

  getLedgers(): Observable<Ledger[]> {
    const token = this.authService.getToken();
    return this.http.get<Ledger[]>(`${this.apiUrl}/ledgers`, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }

  getLedger(id: number): Observable<Ledger> {
    const token = this.authService.getToken();
    return this.http.get<Ledger>(`${this.apiUrl}/ledgers/${id}`, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }

  newLedger(ledger: LedgerRequest): Observable<Ledger> {
    const token = this.authService.getToken();
    return this.http.post<Ledger>(`${this.apiUrl}/ledgers`, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }

  newLedgerForUser(userId: number, ledger: LedgerRequest): Observable<Ledger> {
    const token = this.authService.getToken();
    return this.http.post<Ledger>(`${this.apiUrl}/ledgers/${userId}`, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }
}
