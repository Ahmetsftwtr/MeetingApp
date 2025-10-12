using MeetingApp.DataAccess.Repositories.Abstractions;
using MeetingApp.DataAccess.Repositories.Interfaces;
using MeetingApp.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.DataAccess.Repositories.UnitOfWork
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IMeetingRepository Meetings { get; }
        IMeetingDocumentRepository MeetingDocuments { get; }

        Task<int> SaveChangesAsync();
    }

}
