import { CommonModule } from "@angular/common";
import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { Deposit, Ledger, SimpleLedger } from "../../models/model";
import { LedgerService } from "../../services/ledger.service";
import { BookingService } from "../../services/booking.service";
import { DepositService } from "../../services/deposit.service";

interface LedgerSelection {
  options: Ledger[];
  selected: Ledger | null;
}

@Component({
  selector: "app-deposit-form",
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  standalone: true,
  providers: [DepositService],
  templateUrl: "./deposit-form.component.html",
  styleUrl: "./deposit-form.component.css",
})
export class DepositFormComponent implements OnInit {
  transferForm: FormGroup;

  @Input({ required: true })
  myOptions: Ledger[] = [];

  @Output()
  onDeposit = new EventEmitter<void>();

  selection: LedgerSelection = {
    options: [],
    selected: null,
  };

  showDropdown = false;
  message: string | null = null;

  constructor(
    private formBuilder: FormBuilder,
    private depositService: DepositService
  ) {
    this.transferForm = this.formBuilder.group({
      source: ["", [Validators.required]],
      amount: [null, [Validators.required]],
    });
  }

  ngOnInit(): void {
    this.selection.options = [...this.myOptions];
  }

  onInputChange() {
    const inputValue = this.transferForm.get("source")!.value.toLowerCase();
    this.selection.options = this.myOptions.filter((o) =>
      o.name.toLowerCase().includes(inputValue)
    );
  }

  selectOption(ledger: Ledger) {
    this.transferForm.get("source")!.setValue(ledger.name);
    this.selection.selected = ledger;
    this.showDropdown = false;
  }

  showSuggestions() {
    this.showDropdown = true;
  }

  hideSuggestions() {
    setTimeout(() => {
      this.showDropdown = false;
    }, 200);
  }

  makeDeposit() {
    if (this.transferForm.valid && this.selection.selected) {
      this.depositService
        .newDeposit({
          amount: this.transferForm.value.amount,
          ledgerId: this.selection.selected.id,
        })
        .subscribe({
          next: () => {
            this.message = "Deposit successful!";
            setTimeout(() => {
              this.message = null;
            }, 2000);
            this.onDeposit.emit();
            this.transferForm.reset();
          },
          error: (error) => {
            this.message = `Deposit failed: ${error.error.detail}`;
            console.error("Deposit error", error);
          },
        });
    } else {
      this.message = "Please fill in all fields with valid data.";
    }
  }
}
