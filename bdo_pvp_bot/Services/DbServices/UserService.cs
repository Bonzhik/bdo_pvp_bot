using bdo_pvp_bot.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace bdo_pvp_bot.Services.DbServices
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при создании пользователя");
                return false;
            }
        }
        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при обновлении пользователя");
                return false;
            }
        }
        public async Task<User> FindAsync(ulong id) => await _context.Users.FirstOrDefaultAsync(u => u.DiscordId == id);
        public async Task<User> FindAsyncWithoutLazy(ulong id) => await _context.Users.Include(u => u.CurrentCharacter).ThenInclude(c => c.Class).FirstOrDefaultAsync(u => u.DiscordId == id);
    }
}
