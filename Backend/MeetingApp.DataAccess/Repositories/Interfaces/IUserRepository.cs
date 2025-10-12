using MeetingApp.DataAccess.Repositories.Interfaces;
using MeetingApp.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.DataAccess.Repositories.Abstractions
{
    public interface IUserRepository : IBaseRepository<User>
    {
        IQueryable<User> GetAllQueryable();
        Task<User?> GetByEmailAsync(string email);
    }
}
