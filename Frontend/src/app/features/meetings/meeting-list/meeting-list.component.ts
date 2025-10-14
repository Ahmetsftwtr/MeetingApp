import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MeetingService } from '../../../core/services/meeting.service';
import { MeetingDto } from '../../../core/models/meeting/Response/meetingResponse.dto';
import { Title } from '@angular/platform-browser';
import { IconComponent } from '../../../shared/icon/icon.component';

@Component({
  selector: 'app-meeting-list',
  imports: [CommonModule, RouterModule, IconComponent],
  templateUrl: './meeting-list.component.html',
  styleUrl: './meeting-list.component.scss'
})
export class MeetingListComponent implements OnInit {
  private meetingService = inject(MeetingService);
  private router = inject(Router);
  private titleService = inject(Title);

  meetings = signal<MeetingDto[]>([]);
  loading = signal(true);
  errorMessage = signal('');
  activeTab = signal<'all' | 'upcoming' | 'past' | 'cancelled'>('all');

  constructor() {
    this.titleService.setTitle('Toplantılar - Meeting App');
  }

  ngOnInit() {
    this.loadMeetings();
  }

  loadMeetings() {
    this.loading.set(true);
    this.errorMessage.set('');

    const serviceCall = {
      all: () => this.meetingService.getAll(),
      upcoming: () => this.meetingService.getUpcoming(),
      past: () => this.meetingService.getPast(),
      cancelled: () => this.meetingService.getCancelled()
    }[this.activeTab()];

    serviceCall().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.meetings.set(response.data.meetings || response.data);
        }
        this.loading.set(false);
      },
      error: (error) => {
        this.errorMessage.set(error.message || 'Toplantılar yüklenemedi');
        this.loading.set(false);
      }
    });
  }

  changeTab(tab: 'all' | 'upcoming' | 'past' | 'cancelled') {
    this.activeTab.set(tab);
    this.loadMeetings();
  }

  viewDetail(id: string) {
    this.router.navigate(['/meetings', id]);
  }

  createMeeting() {
    this.router.navigate(['/meetings/create']);
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('tr-TR', {
      day: '2-digit',
      month: 'long',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getStatusClass(meeting: MeetingDto): string {
    if (meeting.isCancelled) return 'cancelled';
    const now = new Date();
    const start = new Date(meeting.startDate);
    const end = new Date(meeting.endDate);
    if (now < start) return 'upcoming';
    if (now > end) return 'past';
    return 'ongoing';
  }

  getStatusText(meeting: MeetingDto): string {
    if (meeting.isCancelled) return 'İptal Edildi';
    const now = new Date();
    const start = new Date(meeting.startDate);
    const end = new Date(meeting.endDate);
    if (now < start) return 'Yaklaşan';
    if (now > end) return 'Tamamlandı';
    return 'Devam Ediyor';
  }

  getStatusIcon(meeting: MeetingDto): 'close' | 'calendar' | 'check' | 'clock' {
    if (meeting.isCancelled) return 'close';
    const now = new Date();
    const start = new Date(meeting.startDate);
    const end = new Date(meeting.endDate);
    if (now < start) return 'calendar';
    if (now > end) return 'check';
    return 'clock';
  }
}
