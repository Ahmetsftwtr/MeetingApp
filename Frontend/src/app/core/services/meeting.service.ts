import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/base/api-response.model';
import { UpdateMeetingRequestDto } from '../models/meeting/Request/updateMeetingRequest.dto';
import { MeetingDto } from '../models/meeting/Response/meetingResponse.dto';
import { CreateMeetingRequestDto } from '../models/meeting/Request/createMeetingRequest.dto';
import { MeetingListResponseDto } from '../models/meeting/Response/meetingListResponse.dto';

@Injectable({
  providedIn: 'root'
})
export class MeetingService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/meetings`;

  create(request: CreateMeetingRequestDto): Observable<ApiResponse<MeetingDto>> {
    return this.http.post<ApiResponse<MeetingDto>>(`${this.apiUrl}/create`, request);
  }

  update(id: string, request: UpdateMeetingRequestDto): Observable<ApiResponse<MeetingDto>> {
    return this.http.put<ApiResponse<MeetingDto>>(`${this.apiUrl}/${id}`, request);
  }

  getById(id: string): Observable<ApiResponse<MeetingDto>> {
    return this.http.get<ApiResponse<MeetingDto>>(`${this.apiUrl}/${id}`);
  }

  getAll(): Observable<ApiResponse<MeetingListResponseDto>> {
    return this.http.get<ApiResponse<MeetingListResponseDto>>(`${this.apiUrl}/all`);
  }

  getUpcoming(): Observable<ApiResponse<MeetingListResponseDto>> {
    return this.http.get<ApiResponse<MeetingListResponseDto>>(`${this.apiUrl}/upcoming`);
  }

  getPast(): Observable<ApiResponse<MeetingListResponseDto>> {
    return this.http.get<ApiResponse<MeetingListResponseDto>>(`${this.apiUrl}/past`);
  }

  getCancelled(): Observable<ApiResponse<MeetingListResponseDto>> {
    return this.http.get<ApiResponse<MeetingListResponseDto>>(`${this.apiUrl}/cancelled`);
  }

  cancel(id: string): Observable<ApiResponse<MeetingDto>> {
    return this.http.patch<ApiResponse<MeetingDto>>(`${this.apiUrl}/${id}/cancel`, {});
  }
}