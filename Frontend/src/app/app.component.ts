import { Component, OnInit } from "@angular/core";
import { RouterOutlet } from "@angular/router";
import { LedgerService } from "../services/ledger.service";

@Component({
  selector: "app-root",
  standalone: true,
  imports: [RouterOutlet],
  providers: [LedgerService],
  templateUrl: "./app.component.html",
  styleUrl: "./app.component.sass",
})
export class AppComponent implements OnInit {
  title = "L-Bank";
  total: number | null = null;

  constructor(private ledgerService: LedgerService) {}

  ngOnInit(): void {
    this.loadTotalMoney();
  }

  loadTotalMoney() {
    this.ledgerService.getTotalMoney().subscribe(
      (data) => (this.total = data),
      (error) => console.error("Error fetching total", error)
    );
  }
}
