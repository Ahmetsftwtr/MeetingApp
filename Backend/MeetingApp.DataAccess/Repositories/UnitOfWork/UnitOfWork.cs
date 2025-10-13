using MeetingApp.DataAccess.Context;
using MeetingApp.DataAccess.Repositories.Abstractions;
using MeetingApp.DataAccess.Repositories.Implementations;
using MeetingApp.DataAccess.Repositories.Interfaces;
using MeetingApp.Model.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace MeetingApp.DataAccess.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MeetingAppDbContext _context;

        public IUserRepository Users { get; }
        public IMeetingRepository Meetings { get; }
        public IMeetingDocumentRepository MeetingDocuments { get; }


        public UnitOfWork(MeetingAppDbContext context,
                          IUserRepository userRepository,
                           IMeetingDocumentRepository meetingDocumentRepository,
                          IMeetingRepository meetingRepository)
        {
            _context = context;
            Users = userRepository;
            Meetings = meetingRepository;
            MeetingDocuments = meetingDocumentRepository;

        }


        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
    }
}
