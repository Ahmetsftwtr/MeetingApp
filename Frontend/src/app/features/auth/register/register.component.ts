import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NgxIntlTelInputModule } from 'ngx-intl-tel-input';
import { SearchCountryField, CountryISO } from 'ngx-intl-tel-input';
import { Title } from '@angular/platform-browser';
import { IconComponent } from '../../../shared/components/icon/icon.component';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, NgxIntlTelInputModule, IconComponent],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private titleService = inject(Title);
  registerForm: FormGroup;
  loading = signal(false);
  errorMessage = signal('');
  selectedFile = signal<File | null>(null);
  previewUrl = signal<string | null>(null);

  SearchCountryField = SearchCountryField;
  CountryISO = CountryISO;
  preferredCountries: CountryISO[] = [CountryISO.Turkey, CountryISO.UnitedStates, CountryISO.UnitedKingdom];

  constructor() {
    this.titleService.setTitle('Kayıt Ol - Meeting App');
    this.registerForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onFileSelect(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile.set(file);
      
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.previewUrl.set(e.target.result);
      };
      reader.readAsDataURL(file);
    }
  }

  removePhoto() {
    this.selectedFile.set(null);
    this.previewUrl.set(null);
  }

  onSubmit() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    const phoneValue = this.registerForm.value.phone;
    const phoneNumber = phoneValue?.e164Number || phoneValue;

    const formData = {
      ...this.registerForm.value,
      phone: phoneNumber,
      profileImage: this.selectedFile()
    };

    this.authService.register(formData).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.router.navigate(['/meetings']);
        }
      },
      error: (error) => {
        this.errorMessage.set(error.message || 'Kayıt başarısız');
        this.loading.set(false);
      },
      complete: () => {
        this.loading.set(false);
      }
    });
  }

  getError(field: string): string {
    const control = this.registerForm.get(field);
    if (control?.hasError('required')) {
      const labels: any = {
        firstName: 'Ad',
        lastName: 'Soyad',
        email: 'Email',
        phone: 'Telefon',
        password: 'Şifre'
      };
      return `${labels[field]} zorunludur`;
    }
    if (control?.hasError('email')) return 'Geçerli bir email giriniz';
    if (control?.hasError('minlength')) return 'Şifre en az 6 karakter olmalıdır';
    return '';
  }
}