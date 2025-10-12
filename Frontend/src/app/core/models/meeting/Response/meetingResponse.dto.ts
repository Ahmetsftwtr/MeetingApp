export interface MeetingDocumentDto {
    id: string;
    originalFileName: string;
    url: string;
    fileExtension: string;
    fileSize: string;
    uploadedAt: string; 
}

export interface MeetingDto {
    id: string;
    title: string;
    description?: string;
    startDate: string;
    endDate: string;
    createdAt: string;
    updatedAt?: string;
    isCancelled: boolean;
    cancelledAt?: string;
    userId: string;
    documents: MeetingDocumentDto[];
}
