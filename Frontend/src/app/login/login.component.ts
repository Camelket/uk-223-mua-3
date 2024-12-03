import { Component } from '@angular/core';
import { HttpClient, HttpHandler } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule],
  providers:  [HttpClient]
})
export class LoginComponent {
  username: string = '';
  password: string = '';
  errorMessage: string = '';

  constructor(private http: HttpClient, private authService: AuthService, private router: Router) {}

  onSubmit() {
    const loginData = { username: this.username, password: this.password };

    this.http.post(`${environment.apiUrl}/auth/login`, loginData).subscribe({
      next: (response: any) => {
        console.log('Login successful', response);
        // Handle success (e.g., navigate to another page)
        this.authService.setToken(response.token);
        
        // Redirect to the ledgers page
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        console.error('Login failed', err);
        this.errorMessage = 'Invalid username or password.';
      },
    });
  }
}
