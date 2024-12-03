import { Component, OnInit } from '@angular/core';
import { LedgerService } from '../../services/ledger.service';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { BookingService } from '../../services/booking.service';
import {ReactiveFormsModule} from '@angular/forms';
import { Ledger } from '../../models/model';


@Component({
  selector: 'app-ledger',
  templateUrl: './ledger.component.html',
  styleUrls: ['./ledger.component.css'],
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  standalone: true,
  providers:  [ LedgerService, HttpClient ]
})
export class LedgerComponent implements OnInit {
  transferForm: FormGroup;

  ledgers: Ledger[] = [];
  transferMessage: string = '';

  constructor(private ledgerService: LedgerService, private bookingService: BookingService, private formBuilder: FormBuilder) {
    this.transferForm =  this.formBuilder.group({
      sourceId: [null, [Validators.required, Validators.min(1)]],
      targetId: [null, [Validators.required, Validators.min(1)]],
      amount: [null, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.loadLedgers();
  }

  loadLedgers(): void {
    this.ledgerService.getMyLedgers().subscribe(
      (data) => (this.ledgers = data),
      (error) => console.error('Error fetching ledgers', error)
    );
  }

  makeTransfer(): void {
    if (
      this.transferForm.valid
    ) {
      this.bookingService
        .transferFunds(this.transferForm.value)
        .subscribe({
          next: () => {
            this.transferMessage = 'Transfer successful!';
            this.loadLedgers(); // Refresh ledger balances
          },
          error: (error) => {
            this.transferMessage = `Transfer failed: ${error.error.message}`;
            console.error('Transfer error', error);
          }
        });
    } else {
      this.transferMessage = 'Please fill in all fields with valid data.';
    }
  }
}
