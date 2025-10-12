import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { MeetingService } from '../../../core/services/meeting.service';
import { UpdateMeetingRequestDto } from '../../../core/models/meeting/Request/updateMeetingRequest.dto';

@Component({
  selector: 'app-meeting-edit',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './meeting-edit.component.html',
  styleUrl: './meeting-edit.component.scss'
})
export class MeetingEditComponent implements OnInit {
  private fb = inject(FormBuilder);
  private meetingService = inject(MeetingService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  loading = signal(false);
  errorMessage = signal('');
  meetingId: string = '';

  meetingForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(3)]],
    description: [''],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required]
  }, { validators: this.dateRangeValidator });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.meetingId = id;
      this.loadMeeting();
    } else {
      this.errorMessage.set('Geçersiz toplantı ID');
    }
  }

  dateRangeValidator(group: FormGroup): { [key: string]: boolean } | null {
    const startDate = group.get('startDate')?.value;
    const endDate = group.get('endDate')?.value;
    
    if (startDate && endDate && new Date(startDate) >= new Date(endDate)) {
      return { dateRange: true };
    }
    
    return null;
  }

  loadMeeting(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.meetingService.getById(this.meetingId).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          const meeting = response.data;
          
          const startDate = new Date(meeting.startDate);
          const endDate = new Date(meeting.endDate);
          
          const formatDateTime = (date: Date): string => {
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            const hours = String(date.getHours()).padStart(2, '0');
            const minutes = String(date.getMinutes()).padStart(2, '0');
            return `${year}-${month}-${day}T${hours}:${minutes}`;
          };

          this.meetingForm.patchValue({
            title: meeting.title,
            description: meeting.description || '',
            startDate: formatDateTime(startDate),
            endDate: formatDateTime(endDate)
          });

          this.loading.set(false);
        } else {
          this.errorMessage.set(response.message || 'Toplantı yüklenemedi');
          this.loading.set(false);
        }
      },
      error: (error) => {
        this.errorMessage.set(error.message || 'Bir hata oluştu');
        this.loading.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.meetingForm.invalid) {
      this.meetingForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    const formValue = this.meetingForm.value;
    const request: UpdateMeetingRequestDto = {
      title: formValue.title,
      description: formValue.description || undefined,
      startDate: new Date(formValue.startDate).toISOString(),
      endDate: new Date(formValue.endDate).toISOString()
    };

    this.meetingService.update(this.meetingId, request).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.router.navigate(['/meetings', this.meetingId]);
        } else {
          this.errorMessage.set(response.message || 'Toplantı güncellenemedi');
          this.loading.set(false);
        }
      },
      error: (error) => {
        this.errorMessage.set(error.message || 'Bir hata oluştu');
        this.loading.set(false);
      }
    });
  }

  get title() {
    return this.meetingForm.get('title');
  }

  get startDate() {
    return this.meetingForm.get('startDate');
  }

  get endDate() {
    return this.meetingForm.get('endDate');
  }
}
