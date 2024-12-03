export interface Ledger {
  id: number;
  userId: number;
  name: string;
  balance: number;
}

export interface Booking {
  id: number;
  sourceId: number;
  targetId: number;
  transferedAmount: number;
  date: Date;
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

export interface Bookingrequest {
  sourceId: number;
  targetId: number;
  amount: number;
}

export interface BookingResponse {
  id: number,
  sourceId: number,
  targetId: number,
  transferedAmount: number,
  dateTime: Date
}
