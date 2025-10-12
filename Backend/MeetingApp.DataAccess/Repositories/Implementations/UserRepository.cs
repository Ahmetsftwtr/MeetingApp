using MeetingApp.DataAccess.Context;
using MeetingApp.DataAccess.Repositories.Abstractions;
using MeetingApp.DataAccess.Repositories.Base;
using MeetingApp.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingApp.DataAccess.Repositories.Implementations
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(MeetingAppDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public IQueryable<User> GetAllQueryable()
        {
            return _dbSet.AsQueryable();
        }

 
    }
}