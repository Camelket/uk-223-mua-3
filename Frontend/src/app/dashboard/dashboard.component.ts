import { Component, OnInit } from "@angular/core";
import { LedgerService } from "../../services/ledger.service";
import { Booking, Deposit, Ledger } from "../../models/model";
import { LedgerOverviewComponent } from "../ledger-overview/ledger-overview.component";
import { TransactionHistoryComponent } from "../transaction-history/transaction-history.component";
import { BookingService } from "../../services/booking.service";
import { TransactionFormComponent } from "../transaction-form/transaction-form.component";
import { DepositFormComponent } from "../deposit-form/deposit-form.component";
import { DepositHistoryComponent } from "../deposit-history/deposit-history.component";
import { DepositService } from "../../services/deposit.service";

@Component({
  selector: "app-dashboard",
  imports: [
    LedgerOverviewComponent,
    TransactionHistoryComponent,
    TransactionFormComponent,
    DepositFormComponent,
    DepositHistoryComponent
  ],
  providers: [LedgerService, BookingService],
  templateUrl: "./dashboard.component.html",
  styleUrl: "./dashboard.component.css",
})
export class DashboardComponent implements OnInit {
  myLedgers: Ledger[] = [];
  myBookings: Booking[] = [];
  myDeposits: Deposit[] = [];

  constructor(
    private ledgerService: LedgerService,
    private bookingService: BookingService,
    private depositService: DepositService
  ) {}

  ngOnInit(): void {
    this.loadLedgers();
    this.loadBookings();
    this.loadDeposits();
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

  loadDeposits(): void {
    this.depositService.getMyDeposits().subscribe(
      (data) => (this.myDeposits = data),
      (error) => console.error("Error fetching deposits", error)
    );
  }

  reload() {
    this.loadLedgers();
    this.loadBookings();
    this.loadDeposits();
  }
}
