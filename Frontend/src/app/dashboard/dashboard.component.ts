import { Component, OnInit } from "@angular/core";
import { LedgerService } from "../../services/ledger.service";
import { Booking, Ledger } from "../../models/model";
import { LedgerOverviewComponent } from "../ledger-overview/ledger-overview.component";
import { TransactionHistoryComponent } from "../transaction-history/transaction-history.component";
import { BookingService } from "../../services/booking.service";
import { TransactionFormComponent } from "../transaction-form/transaction-form.component";

@Component({
  selector: "app-dashboard",
  imports: [
    LedgerOverviewComponent,
    TransactionHistoryComponent,
    TransactionFormComponent,
  ],
  providers: [LedgerService, BookingService],
  templateUrl: "./dashboard.component.html",
  styleUrl: "./dashboard.component.css",
})
export class DashboardComponent implements OnInit {
  myLedgers: Ledger[] = [];
  myBookings: Booking[] = [];

  constructor(
    private ledgerService: LedgerService,
    private bookingService: BookingService
  ) {}

  ngOnInit(): void {
    this.loadLedgers();
    this.loadBookings();
  }

  loadLedgers(): void {
    this.ledgerService.getMyLedgers().subscribe(
      (data) => (this.myLedgers = data),
      (error) => console.error("Error fetching ledgers", error)
    );
  }

  loadBookings(): void {
    this.bookingService.getMyBookings().subscribe(
      (data) => (this.myBookings = data),
      (error) => console.error("Error fetching bookings", error)
    );
  }

  reload() {
    this.loadLedgers();
    this.loadBookings();
  }
}
