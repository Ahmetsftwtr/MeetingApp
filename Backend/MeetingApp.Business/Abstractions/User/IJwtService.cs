using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace MeetingApp.Business.Abstractions.User
{
    public interface IJwtService
    {
        string GenerateAccessToken(Guid userId, string email, string fullName);
        ClaimsPrincipal? ValidateToken(string token);

    }
}
