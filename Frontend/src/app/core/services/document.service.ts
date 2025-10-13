import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MeetingDocumentDto } from '../models/meeting/Response/meetingResponse.dto';
import { ApiResponse } from '../models/base/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  upload(meetingId: string, file: File): Observable<ApiResponse<MeetingDocumentDto>> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<ApiResponse<MeetingDocumentDto>>(
      `${this.apiUrl}/MeetingDocuments/upload?meetingId=${meetingId}`,
      formData
    );
  }

  getList(meetingId: string): Observable<ApiResponse<MeetingDocumentDto[]>> {
    return this.http.get<ApiResponse<MeetingDocumentDto[]>>(
      `${this.apiUrl}/meetings/${meetingId}/documents/list`
    );
  }

  download(meetingId: string, documentId: string): Observable<Blob> {
    return this.http.get(
      `${this.apiUrl}/MeetingDocuments/${documentId}/download`,
      { responseType: 'blob' }
    );
  }

  delete(meetingId: string, documentId: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(
      `${this.apiUrl}/MeetingDocuments/${documentId}`
    );
  }

  downloadDocument(meetingId: string, documentId: string, fileName: string): void {
    this.download(meetingId, documentId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => console.error('Download error:', err)
    });
  }
}