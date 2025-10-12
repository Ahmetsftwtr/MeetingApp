import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MeetingService } from '../../../core/services/meeting.service';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-meeting-create',
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './meeting-create.component.html',
  styleUrl: './meeting-create.component.scss'
})
export class MeetingCreateComponent {
  private fb = inject(FormBuilder);
  private meetingService = inject(MeetingService);
  private router = inject(Router);
  private titleService = inject(Title);

  meetingForm: FormGroup;
  loading = signal(false);
  errorMessage = signal('');

  constructor() {
    this.titleService.setTitle('Toplantı Oluştur - Meeting App');
    this.meetingForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3)]],
      description: [''],
      startDate: ['', [Validators.required]],
      endDate: ['', [Validators.required]]
    });
  }

  onSubmit() {
    if (this.meetingForm.invalid) {
      this.meetingForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.meetingService.create(this.meetingForm.value).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.router.navigate(['/meetings', response.data.id]);
        }
      },
      error: (error) => {
        this.errorMessage.set(error.message || 'Toplantı oluşturulamadı');
        this.loading.set(false);
      }
    });
  }

  getError(field: string): string {
    const control = this.meetingForm.get(field);
    if (control?.hasError('required')) {
      const labels: any = {
        title: 'Başlık',
        startDate: 'Başlangıç tarihi',
        endDate: 'Bitiş tarihi'
      };
      return `${labels[field]} zorunludur`;
    }
    if (control?.hasError('minlength')) return 'Başlık en az 3 karakter olmalıdır';
    return '';
  }
}
