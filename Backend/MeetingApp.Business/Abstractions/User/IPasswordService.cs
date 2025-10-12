using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.Business.Abstractions.User
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        bool IsPasswordValid(string password);
    }
}
