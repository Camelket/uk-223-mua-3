import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Observable } from "rxjs";
import { AuthService } from "./auth.service";
import { Ledger, LedgerRequest, SimpleLedger } from "../models/model";
import { environment } from "../environments/environment";

@Injectable({
  providedIn: "root",
})
export class LedgerService {
  constructor(private http: HttpClient, private authService: AuthService) {}

  //TODO: correct methods to use ledger arg

  getMyLedgers(): Observable<Ledger[]> {
    const token = this.authService.getToken();
    return this.http.get<Ledger[]>(`${environment.apiUrl}/ledgers`, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }

  getAllSimpleLedgers(): Observable<SimpleLedger[]> {
    const token = this.authService.getToken();
    return this.http.get<SimpleLedger[]>(`${environment.apiUrl}/ledgers/names`, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }

  getAllLedgers(): Observable<Ledger[]> {
    const token = this.authService.getToken();
    return this.http.get<Ledger[]>(`${environment.apiUrl}/ledgers/all`, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }

  getLedger(id: number): Observable<Ledger> {
    const token = this.authService.getToken();
    return this.http.get<Ledger>(`${environment.apiUrl}/ledgers/${id}`, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }

  newLedger(ledgerReq: LedgerRequest): Observable<Ledger> {
    const token = this.authService.getToken();
    return this.http.post<Ledger>(`${environment.apiUrl}/ledgers`, ledgerReq, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }

  newLedgerForUser(userId: number, ledgerReq: LedgerRequest): Observable<Ledger> {
    const token = this.authService.getToken();
    return this.http.post<Ledger>(`${environment.apiUrl}/ledgers/${userId}`, ledgerReq, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }
}
