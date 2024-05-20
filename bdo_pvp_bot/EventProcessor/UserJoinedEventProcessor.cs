using bdo_pvp_bot.Services.DbServices;
using Discord.WebSocket;

namespace bdo_pvp_bot.EventProcessor
{
    public class UserJoinedEventProcessor
    {
        private readonly UserService _userService;
        public UserJoinedEventProcessor(UserService userService)
        {
            _userService = userService;
        }

        public async Task CheckUser(SocketGuildUser user)
        {
            var userCheck = await _userService.FindAsync(user.Id);

            if (userCheck == null)
                return;
            if (userCheck.Characters.Count >= 0 && ((userCheck.Characters.Any(c => c.Class.Name == "Archer")) || (userCheck.Characters.Any(c => c.ClassType != null))))
            {
                await user.AddRoleAsync(user.Guild.Roles.FirstOrDefault(r => r.Name == "User"));
                await user.AddRoleAsync(user.Guild.Roles.FirstOrDefault(r => r.Name == "Player"));
            }
            else
            {
                await user.AddRoleAsync(user.Guild.Roles.FirstOrDefault(r => r.Name == "User"));
            }
        }
    }
}
