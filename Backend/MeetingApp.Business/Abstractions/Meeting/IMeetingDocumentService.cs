using MeetingApp.Models.DTOs.File;
using MeetingApp.Models.DTOs.Meeting;
using MeetingApp.Models.ReturnTypes.Abstract;


namespace MeetingApp.Business.Abstractions.Meeting
{
    public interface IMeetingDocumentService
    {
        Task<IResult> UploadDocumentAsync(Guid meetingId, Guid userId, FileUploadDto dto);
        Task<IResult> GetDocumentsByMeetingIdAsync(Guid meetingId, Guid userId);
        Task<IResult> DownloadDocumentAsync(Guid documentId, Guid userId);
        Task<IResult> DeleteDocumentAsync(Guid documentId, Guid userId);
    }
}
