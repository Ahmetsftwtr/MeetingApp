export enum MeetingStatus {
  All = 0,
  Upcoming = 1,
  Past = 2,
  Cancelled = 3,
  Active = 4
}

export interface MeetingFilterDto {
  status?: MeetingStatus;
  startDateFrom?: string;
  startDateTo?: string;
  endDateFrom?: string;
  endDateTo?: string;
  isCancelled?: boolean;
  searchTerm?: string;
  pageNumber?: number;
  pageSize?: number;
  orderBy?: string;
  isDescending?: boolean;
}
