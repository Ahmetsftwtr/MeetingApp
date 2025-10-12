import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MeetingService } from '../../../core/services/meeting.service';
import { DocumentService } from '../../../core/services/document.service';
import { MeetingDto, MeetingDocumentDto } from '../../../core/models/meeting/Response/meetingResponse.dto';
import { Title } from '@angular/platform-browser';
import { IconComponent } from '../../../shared/icon/icon.component';

@Component({
  selector: 'app-meeting-detail',
  imports: [CommonModule, RouterLink, IconComponent],
  templateUrl: './meeting-detail.component.html',
  styleUrl: './meeting-detail.component.scss'
})
export class MeetingDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private meetingService = inject(MeetingService);
  private documentService = inject(DocumentService);
  private titleService = inject(Title);

  meeting = signal<MeetingDto | null>(null);
  loading = signal(true);
  errorMessage = signal('');
  uploading = signal(false);
  selectedFile = signal<File | null>(null);
  uploadError = signal('');
  showUploadError = signal(false);

  constructor() {
    this.titleService.setTitle('Toplantı Detayı - Meeting App');
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadMeeting(id);
    }
  }

  loadMeeting(id: string) {
    this.loading.set(true);
    this.errorMessage.set('');

    this.meetingService.getById(id).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.meeting.set(response.data);
        }
        this.loading.set(false);
      },
      error: (error) => {
        this.errorMessage.set(error.message || 'Toplantı yüklenemedi');
        this.loading.set(false);
      }
    });
  }

  onFileSelect(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile.set(file);
      this.uploadDocument();
    }
  }

  uploadDocument() {
    const file = this.selectedFile();
    const meeting = this.meeting();
    
    if (!file || !meeting) return;

    this.uploading.set(true);
    this.uploadError.set('');
    this.showUploadError.set(false);
    
    this.documentService.upload(meeting.id, file).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          const updatedMeeting = { ...meeting };
          updatedMeeting.documents = [...updatedMeeting.documents, response.data];
          this.meeting.set(updatedMeeting);
        }
        this.uploading.set(false);
        this.selectedFile.set(null);
      },
      error: (error) => {
        this.uploading.set(false);
        this.selectedFile.set(null);
        this.uploadError.set(error.message || 'Dosya yüklenirken bir hata oluştu');
        this.showUploadError.set(true);
        
        setTimeout(() => {
          this.showUploadError.set(false);
        }, 5000);
      }
    });
  }

  downloadDocument(doc: MeetingDocumentDto) {
    const meeting = this.meeting();
    if (meeting) {
      this.documentService.downloadDocument(meeting.id, doc.id, doc.originalFileName);
    }
  }

  deleteDocument(docId: string) {
    const meeting = this.meeting();
    if (!meeting || !confirm('Bu dökümanı silmek istediğinizden emin misiniz?')) return;

    this.documentService.delete(meeting.id, docId).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          const updatedMeeting = { ...meeting };
          updatedMeeting.documents = updatedMeeting.documents.filter(d => d.id !== docId);
          this.meeting.set(updatedMeeting);
        }
      }
    });
  }

  cancelMeeting() {
    const meeting = this.meeting();
    if (!meeting || !confirm('Bu toplantıyı iptal etmek istediğinizden emin misiniz?')) return;

    this.meetingService.cancel(meeting.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.loadMeeting(meeting.id);
        }
      }
    });
  }

  deleteMeeting() {
    const meeting = this.meeting();
    if (!meeting || !confirm('Bu toplantıyı silmek istediğinizden emin misiniz?')) return;

    this.meetingService.delete(meeting.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.router.navigate(['/meetings']);
        }
      }
    });
  }

  editMeeting() {
    const meeting = this.meeting();
    if (meeting) {
      this.router.navigate(['/meetings', meeting.id, 'edit']);
    }
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

  formatFileSize(size: string): string {
    const bytes = parseInt(size);
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(2) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
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
