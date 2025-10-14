import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MeetingService } from '../../../core/services/meeting.service';
import { MeetingDto } from '../../../core/models/meeting/Response/meetingResponse.dto';
import { Title } from '@angular/platform-browser';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { MeetingFilterDto, MeetingStatus } from '../../../core/models/meeting/Request/meetingFilter.dto';
import { MeetingsFilterComponent, FilterValues } from '../components/meetings-filter/meetings-filter.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-meeting-list',
  imports: [CommonModule, RouterModule, IconComponent, MeetingsFilterComponent, PaginationComponent],
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
  activeTab = signal<MeetingStatus>(MeetingStatus.All);
  currentPage = signal(1);
  pageSize = signal(10);
  totalCount = signal(0);
  totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize()));
  hasLoaded = signal(false);
  
  searchTerm = signal('');
  startDateFrom = signal('');
  startDateTo = signal('');
  endDateFrom = signal('');
  endDateTo = signal('');
  sortBy = signal('StartDate');
  sortDesc = signal(true);

  constructor() {
    this.titleService.setTitle('Toplantılar - Meeting App');
  }

  ngOnInit() {
    this.loadMeetings();
  }

  loadMeetings() {
    this.loading.set(true);
    this.errorMessage.set('');

    const filter: MeetingFilterDto = {
      status: this.activeTab(),
      pageNumber: this.currentPage(),
      pageSize: this.pageSize(),
      orderBy: this.sortBy(),
      isDescending: this.sortDesc(),
      searchTerm: this.searchTerm() || undefined,
      startDateFrom: this.startDateFrom() || undefined,
      startDateTo: this.startDateTo() || undefined,
      endDateFrom: this.endDateFrom() || undefined,
      endDateTo: this.endDateTo() || undefined
    };

    this.meetingService.getFiltered(filter).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.meetings.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
          this.currentPage.set(response.data.pageNumber);
        }
        this.loading.set(false);
        this.hasLoaded.set(true);
      },
      error: (error) => {
        this.errorMessage.set(error.message || 'Toplantılar yüklenemedi');
        this.loading.set(false);
        this.hasLoaded.set(true);
      }
    });
  }

  onFilterChange(filters: FilterValues) {
    this.searchTerm.set(filters.searchTerm);
    this.startDateFrom.set(filters.startDateFrom);
    this.startDateTo.set(filters.startDateTo);
    this.endDateFrom.set(filters.endDateFrom);
    this.endDateTo.set(filters.endDateTo);
    this.sortBy.set(filters.sortBy);
    this.sortDesc.set(filters.sortDesc);
    this.pageSize.set(filters.pageSize);
    this.currentPage.set(1);
    this.loadMeetings();
  }

  onClearFilters() {
    this.searchTerm.set('');
    this.startDateFrom.set('');
    this.startDateTo.set('');
    this.endDateFrom.set('');
    this.endDateTo.set('');
    this.sortBy.set('StartDate');
    this.sortDesc.set(true);
    this.pageSize.set(10);
    this.currentPage.set(1);
    this.loadMeetings();
  }

  changeTab(status: MeetingStatus) {
    this.activeTab.set(status);
    this.currentPage.set(1);
    this.loadMeetings();
  }

  changePage(page: number) {
    this.currentPage.set(page);
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
