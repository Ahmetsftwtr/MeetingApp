import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/base/api-response.model';
import { UpdateMeetingRequestDto } from '../models/meeting/Request/updateMeetingRequest.dto';
import { MeetingDto } from '../models/meeting/Response/meetingResponse.dto';
import { CreateMeetingRequestDto } from '../models/meeting/Request/createMeetingRequest.dto';
import { MeetingFilterDto } from '../models/meeting/Request/meetingFilter.dto';
import { PagedResultDto } from '../models/base/paged-result.model';

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

  getFiltered(filter: MeetingFilterDto): Observable<ApiResponse<PagedResultDto<MeetingDto>>> {
    let params = new HttpParams();
    
    if (filter.status !== undefined) params = params.set('status', filter.status.toString());
    if (filter.startDateFrom) params = params.set('startDateFrom', filter.startDateFrom);
    if (filter.startDateTo) params = params.set('startDateTo', filter.startDateTo);
    if (filter.endDateFrom) params = params.set('endDateFrom', filter.endDateFrom);
    if (filter.endDateTo) params = params.set('endDateTo', filter.endDateTo);
    if (filter.isCancelled !== undefined) params = params.set('isCancelled', filter.isCancelled.toString());
    if (filter.searchTerm) params = params.set('searchTerm', filter.searchTerm);
    if (filter.pageNumber !== undefined) params = params.set('pageNumber', filter.pageNumber.toString());
    if (filter.pageSize !== undefined) params = params.set('pageSize', filter.pageSize.toString());
    if (filter.orderBy) params = params.set('orderBy', filter.orderBy);
    if (filter.isDescending !== undefined) params = params.set('isDescending', filter.isDescending.toString());

    return this.http.get<ApiResponse<PagedResultDto<MeetingDto>>>(this.apiUrl, { params });
  }

  cancel(id: string): Observable<ApiResponse<MeetingDto>> {
    return this.http.patch<ApiResponse<MeetingDto>>(`${this.apiUrl}/${id}/cancel`, {});
  }
}