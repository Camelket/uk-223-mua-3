import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Observable, take } from "rxjs";
import { AuthService } from "./auth.service";
import { Booking, BookingRequest } from "../models/model";
import { environment } from "../environments/environment";

@Injectable({
  providedIn: "root",
})
export class BookingService {

  constructor(private http: HttpClient, private authService: AuthService) {}

  getBookingsForLedger(id: number): Observable<Booking> {
    const token = this.authService.getToken();
    return this.http.get<Booking>(`${environment.apiUrl}/ledgers/${id}/bookings`, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }

  getMyBookings(): Observable<Booking[]> {
    const token = this.authService.getToken();
    return this.http.get<Booking[]>(`${environment.apiUrl}/bookings`, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }

  getAllBookings(): Observable<Booking[]> {
    const token = this.authService.getToken();
    return this.http.get<Booking[]>(`${environment.apiUrl}/bookings/all`, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }

  getLedger(id: number): Observable<Booking> {
    const token = this.authService.getToken();
    return this.http.get<Booking>(`${environment.apiUrl}/bookings/${id}`, {
      headers: {
        Authorization: token ?? "",
      },
    });
  }

  transferFunds(bookingRequest: BookingRequest): Observable<Booking> {
    const token = this.authService.getToken();
    return this.http.post<Booking>(`${environment.apiUrl}/booking`, {
      headers: {
        Authorization: token ?? "",
      },
      bookingRequest
    })
  }
}
