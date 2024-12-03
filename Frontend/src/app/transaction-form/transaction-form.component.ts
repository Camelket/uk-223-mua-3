import { CommonModule } from "@angular/common";
import { Component, EventEmitter, Input, Output } from "@angular/core";
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { Ledger, SimpleLedger } from "../../models/model";
import { LedgerService } from "../../services/ledger.service";
import { BookingService } from "../../services/booking.service";

interface ShowDropdowns {
  [key: string]: boolean;
}

interface LedgerSelection {
  sourceOptions: Ledger[];
  targetOptions: SimpleLedger[];
  sourceSelection: Ledger | null;
  targetSelection: SimpleLedger | null;
}

@Component({
  selector: "app-transaction-form",
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  standalone: true,
  templateUrl: "./transaction-form.component.html",
  styleUrl: "./transaction-form.component.css",
})
export class TransactionFormComponent {
  transferForm: FormGroup;

  @Input({ required: true })
  myOptions: Ledger[] = [];

  @Output()
  onTransaction = new EventEmitter<void>();

  options: SimpleLedger[] = [];

  selection: LedgerSelection = {
    sourceOptions: [],
    targetOptions: [],
    sourceSelection: null,
    targetSelection: null,
  };

  showDropdowns: ShowDropdowns = {};

  message: string | null = null;

  constructor(
    private formBuilder: FormBuilder,
    private ledgerService: LedgerService,
    private bookingService: BookingService
  ) {
    this.transferForm = this.formBuilder.group({
      source: ["", [Validators.required]],
      target: ["", [Validators.required]],
      amount: [null, [Validators.required, Validators.min(0.1)]],
    });
  }

  ngOnInit() {
    this.loadSimpleLedgers();
  }

  loadSimpleLedgers() {
    this.ledgerService.getAllSimpleLedgers().subscribe(
      (data) => {
        this.options = data;
        this.selection.sourceOptions = [...this.myOptions];
        this.selection.targetOptions = [...this.options];
      },
      (error) => console.error("Cannot load simple ledgers")
    );
  }

  onInputChange(field: "source" | "target") {
    const inputValue = this.transferForm.get(field)!.value.toLowerCase();
    if (field === "source") {
      this.selection.sourceOptions = this.myOptions.filter((o) =>
        o.name.toLowerCase().includes(inputValue)
      );
    }
    if (field === "target") {
      this.selection.targetOptions = this.options.filter((o) =>
        o.name.toLowerCase().includes(inputValue)
      );
    }
  }

  selectOption(field: "source" | "target", ledger: Ledger | SimpleLedger) {
    this.transferForm.get(field)!.setValue(ledger.name);

    if (field === "source") {
      this.selection.sourceSelection = ledger as Ledger;
    }
    if (field === "target") {
      this.selection.targetSelection = ledger as SimpleLedger;
    }

    this.showDropdowns[field] = false;
  }

  showSuggestions(field: "source" | "target") {
    this.showDropdowns[field] = true;
  }

  hideSuggestions(field: "source" | "target") {
    setTimeout(() => {
      this.showDropdowns[field] = false;
    }, 200);
  }

  makeTransfer() {
    if (
      this.transferForm.valid &&
      this.selection.sourceSelection &&
      this.selection.targetSelection
    ) {
      this.bookingService
        .transferFunds({
          amount: this.transferForm.value.amount,
          sourceId: this.selection.sourceSelection.id,
          targetId: this.selection.targetSelection.id,
        })
        .subscribe({
          next: () => {
            this.message = "Transfer successful!";
            setTimeout(() => {
              this.message = null;
            }, 2000);
            this.onTransaction.emit();
            this.transferForm.reset();
          },
          error: (error) => {
            this.message = `Transfer failed: ${error.error.detail}`;
            console.error("Transfer error", error);
          },
        });
    } else {
      this.message = "Please fill in all fields with valid data.";
    }
  }
}
