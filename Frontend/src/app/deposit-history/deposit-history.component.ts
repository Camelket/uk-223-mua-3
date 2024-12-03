import { Component, Input } from "@angular/core";
import { Deposit } from "../../models/model";
import dayjs from "dayjs";

@Component({
  selector: "app-deposit-history",
  imports: [],
  templateUrl: "./deposit-history.component.html",
  styleUrl: "./deposit-history.component.css",
})
export class DepositHistoryComponent {
  @Input({ required: true })
  myDeposits: Deposit[] = [];

  mySortedDeposits = () =>
    this.myDeposits.sort((a, b) => dayjs(b.date).unix() - dayjs(a.date).unix());

  formatDate(input: string): string {
    return dayjs(input).format("DD.MM.YYYY - HH:mm");
  }
}
