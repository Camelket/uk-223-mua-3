import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Observable } from "rxjs";
import { AuthService } from "./auth.service";
import { Deposit, DepositRequest } from "../models/model";
import { environment } from "../environments/environment";

@Injectable({
  providedIn: "root",
})
export class DepositService {
  constructor(private http: HttpClient, private authService: AuthService) {}

  getDepositsForLedger(id: number): Observable<Deposit[]> {
    const token = this.authService.getToken();
    return this.http.get<Deposit[]>(
      `${environment.apiUrl}/ledgers/${id}/deposits`,
      {
        headers: {
          Authorization: token ?? "",
        },
      }
    );
  }

  getMyDeposits(): Observable<Deposit[]> {
    const token = this.authService.getToken();
    return this.http.get<Deposit[]>(`${environment.apiUrl}/deposits`, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }

  getAllDeposits(): Observable<Deposit[]> {
    const token = this.authService.getToken();
    return this.http.get<Deposit[]>(`${environment.apiUrl}/deposits/all`, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }

  newDeposit(depositReq: DepositRequest): Observable<Deposit> {
    const token = this.authService.getToken();
    return this.http.post<Deposit>(
      `${environment.apiUrl}/deposits`,
      depositReq,
      {
        headers: {
          Authorization: token ?? "",
        },
      }
    );
  }
}
