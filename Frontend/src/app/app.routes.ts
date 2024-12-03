import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { LedgerComponent } from './ledger/ledger.component';
import { AuthGuard } from './auth.guard';

export const routes: Routes = [
    {
      path: 'dashboard',
      component: LedgerComponent,
      canActivate: [AuthGuard],
    },
    {
      path: 'login',
      component: LoginComponent,
    },
    {
        path: '',
        redirectTo: '/login',
        pathMatch: 'full',
    }
  ];