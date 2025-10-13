using MeetingApp.Model.Entities;
using MeetingApp.Models.DTOs.User;
using System;

namespace MeetingApp.Business.Mappings
{
    public static class UserMapper
    {
        public static UserDto ToDto(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                ProfileImagePath = user.ProfileImagePath
            };
        }

        public static User ToEntity(RegisterDto dto, string passwordHash)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = dto.Email.ToLower().Trim(),
                Phone = dto.Phone.Trim(),
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}