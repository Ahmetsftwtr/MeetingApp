using MeetingApp.Models.DTOs.File;
using MeetingApp.Models.ReturnTypes.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingApp.Business.Abstractions.File
{
    public interface IFileService
    {
        Task<IResult> UploadFileAsync(FileUploadDto dto, string category, string? subFolder = null);
        Task<bool> DeleteFileAsync(string relativePath);
        string GetFileUrl(string? relativePath);
    }
}
