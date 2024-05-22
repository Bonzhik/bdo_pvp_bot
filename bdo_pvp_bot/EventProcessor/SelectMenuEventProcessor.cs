using bdo_pvp_bot.Services.DbServices;
using Discord;
using Discord.WebSocket;

namespace bdo_pvp_bot.EventProcessor
{
    public class SelectMenuEventProcessor
    {
        private readonly CharacterService _characterService;
        private readonly UserService _userService;
        public SelectMenuEventProcessor(CharacterService characterService, UserService userService)
        {
            _characterService = characterService;
            _userService = userService;
        }

        public async Task ClassMenuProcess(SocketMessageComponent component)
        {
            await component.UpdateAsync(msg => { msg.Components = null; msg.Content = "Класс персонажа выбран"; });

            var parts = component.Data.CustomId.Split('-');
            int idValue = int.Parse(parts[1]);
            var character = await _characterService.GetCharacterAsync(idValue);
            var characterClass = await _characterService.GetClassAsync(long.Parse(component.Data.Values.FirstOrDefault()));
            

            character.Class = characterClass;

            var selectMenu = new SelectMenuBuilder()
                .WithCustomId($"c{component.Data.CustomId}")
                .WithPlaceholder("Выберите стойку")
                .AddOption("Наследие", "1")
                .AddOption("Пробуждение", "2")
                .WithMinValues(1)
                .WithMaxValues(1);

            var builder = new ComponentBuilder().WithSelectMenu(selectMenu);

            if (await _characterService.UpdateCharacterAsync(character) != null)
            {
                    var guildUser = component.User as SocketGuildUser;
                    var role = guildUser.Guild.Roles.FirstOrDefault(n => n.Name == "Player");
                    await guildUser.AddRoleAsync(role.Id);

                    await component.FollowupAsync("Успех", ephemeral: true);
                    return;
            }
            else
                await component.FollowupAsync("Ошибка", ephemeral: true);
        }
        public async Task SelectClassMenuProcess(SocketMessageComponent component)
        {
            await component.UpdateAsync(msg => { msg.Components = null; msg.Content = "Персонаж выбран"; });

            long idChar = long.Parse(component.Data.Values.FirstOrDefault());
            ulong userId = component.User.Id;
            var user = await _userService.FindAsync(userId);
            var character = await _characterService.GetCharacterAsync(idChar);
            user.CurrentCharacter = character;

            if (await _userService.UpdateUserAsync(user))
                await component.FollowupAsync("Успех", ephemeral: true);
            else
                await component.FollowupAsync("Ошибка", ephemeral: false);
        }
        public async Task DeleteCharProcess(SocketMessageComponent component)
        {
            var userGuild = component.User as SocketGuildUser;
            var guild = userGuild.Guild;
            var user = await _userService.FindAsync(userGuild.Id);

            await component.UpdateAsync(msg => { msg.Components = null; msg.Content = "Персонаж на удаление выбран"; });

            if (await _characterService.DeleteCharacterAsync(long.Parse(component.Data.Values.FirstOrDefault())) != null)
            {
                if (user.Characters.Count == 0)
                    await userGuild.RemoveRoleAsync(guild.Roles.FirstOrDefault(r => r.Name == "Player"));
                await component.FollowupAsync("Успех", ephemeral: true);
            }
            else
                await component.FollowupAsync("Ошибка", ephemeral: true);
        }
    }
}
