import { CommonModule } from "@angular/common";
import { Component, Input, OnInit } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Ledger } from "../../models/model";

@Component({
  selector: "app-ledger-overview",
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: "./ledger-overview.component.html",
  styleUrl: "./ledger-overview.component.css",
})
export class LedgerOverviewComponent {
  @Input({ required: true })
  myLedgers: Ledger[] = [];
}
