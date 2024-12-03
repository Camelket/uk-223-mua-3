import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Observable, take } from "rxjs";
import { AuthService } from "./auth.service";
import { Booking, Bookingrequest, BookingResponse } from "../models/model";

@Injectable({
  providedIn: "root",
})
export class BookingService {
  private apiUrl = "https://localhost:44386/api/v1";

  constructor(private http: HttpClient, private authService: AuthService) {}

  getBookingsForLedger(id: number): Observable<Booking> {
    const token = this.authService.getToken();
    return this.http.get<Booking>(`${this.apiUrl}/ledgers/${id}/bookings`, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }

  transferFunds(bookingRequest: Bookingrequest): Observable<BookingResponse> {
    const token = this.authService.getToken();
    return this.http.post<BookingResponse>(`${this.apiUrl}/booking`, {
      headers: {
        Authorization: token ?? "",
      },
      bookingRequest
    })
  }
}
