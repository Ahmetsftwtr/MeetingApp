using Azure.Core;
using MeetingApp.Model.Entities;
using MeetingApp.Models.DTOs.File;
using MeetingApp.Models.DTOs.User;
using MeetingApp.Models.ReturnTypes.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.Business.Abstractions.User
{
    public interface IUserService
    {
        Task<IResult> Register(RegisterDto dto, FileUploadDto? profileImageDto);
        Task<IResult> Login(LoginDto dto);
        Task<IResult> GetProfile(Guid userId);
        Task<IResult> ChangePassword(Guid userId, string oldPassword, string newPassword);
    }
}
