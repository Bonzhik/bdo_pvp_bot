using bdo_pvp_bot.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace bdo_pvp_bot.Services.DbServices
{
    public class CharacterService
    {
        private readonly ApplicationDbContext _context;

        public CharacterService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Character> CreateCharacterAsync(Character character)
        {
            try
            {
                await _context.Characters.AddAsync(character);
                await _context.SaveChangesAsync();
                return character;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Ошибка добавления персонажа у юзера {character.User.DiscordId}");
                return null;
            }
        }
        public async Task<Character> UpdateCharacterAsync(Character character)
        {
            try
            {
                _context.Characters.Update(character);
                await _context.SaveChangesAsync();
                return character;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Ошибка обновления персонажа {character.Id}");
                return null;
            }
        }
        public async Task<Character> GetCharacterAsync(long id) => await _context.Characters.FindAsync(id);
        public async Task<List<Character>> GetCharactersByUserAsync(ulong id) => await _context.Characters.Where(u => u.User.DiscordId == id).ToListAsync();
        public async Task<List<CharacterClass>> GetCharacterClassesAsync() => await _context.CharacterClasses.ToListAsync();
        public async Task<CharacterClass> GetClassAsync(long id) => await _context.CharacterClasses.FindAsync(id);
        public async Task<Character> DeleteCharacterAsync(long id)
        {
            try
            {
                var character = await _context.Characters.FindAsync(id);
                _context.Characters.Remove(character);
                await _context.SaveChangesAsync();
                return character;
            } catch (Exception ex) 
            {
                Log.Error(ex, $"Ошибка удаления персонажа {id}");
                return null;
            }

        }
    }
}
