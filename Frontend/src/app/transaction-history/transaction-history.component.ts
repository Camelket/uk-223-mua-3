import { Component, Input } from "@angular/core";
import { Booking } from "../../models/model";
import dayjs from "dayjs";

@Component({
  selector: "app-transaction-history",
  imports: [],
  templateUrl: "./transaction-history.component.html",
  styleUrl: "./transaction-history.component.css",
})
export class TransactionHistoryComponent {
  @Input({ required: true })
  myBookings: Booking[] = [];

  mySortedBookings = () =>
    this.myBookings.sort((a, b) => dayjs(b.date).unix() - dayjs(a.date).unix());

  formatDate(input: string): string {
    return dayjs(input).format("DD.MM.YYYY - HH:mm");
  }
}
