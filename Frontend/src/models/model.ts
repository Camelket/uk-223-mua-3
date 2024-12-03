export interface Ledger {
  id: number;
  userId: number;
  name: string;
  balance: number;
}

export interface SimpleLedger {
  id: number;
  name: string;
}

export interface Booking {
  id: number;
  sourceId: number;
  sourceName: string;
  targetId: number;
  targetName: string;
  transferedAmount: number;
  date: string;
}

export interface Deposit {
  id: number;
  amount: number;
  ledgerId: number;
  ledgerName: string;
  date: string;
}

export interface Login {
  token: string;
}

export interface User {
  id: number;
  username: string;
  role: string;
  ledgers: Ledger[];
}

// requests
export interface LoginRequest {
  username: string;
  password: string;
}

export interface LedgerRequest {
  name: string;
}

export interface BookingRequest {
  sourceId: number;
  targetId: number;
  amount: number;
}

export interface DepositRequest {
  amount: number;
  ledgerId: number;
}
