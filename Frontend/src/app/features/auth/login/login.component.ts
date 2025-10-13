import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  private fb = inject(FormBuilder); 
  private authService = inject(AuthService);
  private router = inject(Router);
  private titleService = inject(Title);

  loginForm: FormGroup;
  loading = signal(false);
  errorMessage = signal('');

  constructor() {
    this.titleService.setTitle('Giriş Yap - Meeting App');
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.authService.login(this.loginForm.value).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.router.navigate(['/meetings']);
        }
      },
      error: (error) => {
        this.errorMessage.set(error.message || 'Giriş başarısız');
        this.loading.set(false);
      },
      complete: () => {
        this.loading.set(false);
      }
    });
  }

  getError(field: string): string {
    const control = this.loginForm.get(field);
    if (control?.hasError('required')) return `${field === 'email' ? 'Email' : 'Şifre'} zorunludur`;
    if (control?.hasError('email')) return 'Geçerli bir email giriniz';
    if (control?.hasError('minlength')) return 'Şifre en az 6 karakter olmalıdır';
    return '';
  }
}