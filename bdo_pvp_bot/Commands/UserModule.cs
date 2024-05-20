using bdo_pvp_bot.Services.DbServices;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Domain.Entities;
using Serilog;
using System.Runtime.ConstrainedExecution;

namespace bdo_pvp_bot.Commands
{
    public class UserModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly UserService _userService;
        private readonly CharacterService _characterService;
        public UserModule(UserService userService, CharacterService characterService)
        {
            _userService = userService;
            _characterService = characterService;
        }
        [SlashCommand("stat", "Статистика на текущем персонаже")]
        public async Task GetStat()
        {
            var userGuild = Context.User as IGuildUser;
            var roleId = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Player").Id;

            Log.Information($"{userGuild.DisplayName} Попытка получить статистику");

            if (!userGuild.RoleIds.Contains(roleId))
            {
                await RespondAsync("Нет персонажей", ephemeral: true);
                return;
            }
            var id = Context.User.Id;
            var user = await _userService.FindAsync(id);
            var CharClass = (user.CurrentCharacter.ClassType != null) ? user.CurrentCharacter.ClassType.ToString() : "";
            var builder = new EmbedBuilder()
                .WithTitle("Информация об игроке")
                .WithDescription("Статистика на текущем персонаже")
                .AddField("Player", user.Nickname, true)
                .AddField("Class", $"{user.CurrentCharacter.Class.Name}(" + $"{CharClass})", true)
                .AddField("Elo", user.CurrentCharacter.Elo, true)
                .WithColor(Color.Green);

            await RespondAsync(embed: builder.Build(), ephemeral: true);
        }
        [SlashCommand("registration", "Регистрация")]
        public async Task Registration(string nickname)
        {
            var userGuild = Context.User as IGuildUser;
            var usedDb = await _userService.FindAsync(userGuild.Id);

            Log.Information($"{userGuild.DisplayName} Попытка зарегистрироваться");

            if (usedDb != null)
            {
                await RespondAsync("Уже зарегистрирован", ephemeral: true);
                return;
            }
            if (await _userService.CreateUserAsync(new User { Nickname = nickname, IsInMatch = false, DiscordId = userGuild.Id }))
            {
                var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == "User");
                await userGuild.AddRoleAsync(role);
                await RespondAsync("Успех", ephemeral: true);
                return;
            }
            Log.Information($"{userGuild.DisplayName} Ошибка при регистрации");
            await RespondAsync("Ошибка", ephemeral: true);
        }
        [SlashCommand("update-nickname", "Обновить фамилию")]
        public async Task UpdateNickname(string nickname)
        {
            var userGuild = Context.User as IGuildUser;
            var usedDb = await _userService.FindAsync(userGuild.Id);

            Log.Information($"{userGuild.DisplayName} Попытка сменить фамилию");

            if (usedDb == null)
            {
                await RespondAsync("Не зарегистрирован", ephemeral: true);
                return;
            }
            usedDb.Nickname = nickname;
            if (await _userService.UpdateUserAsync(usedDb))
            {
                await RespondAsync("Успех", ephemeral: true);
                return;
            }
            Log.Information($"{userGuild.DisplayName} Ошибка смены фамилии");
            await RespondAsync("Ошибка", ephemeral: true);
        }
        [SlashCommand("add-character", "Добавить персонажа")]
        public async Task AddCharacter()
        {
            var userGuild = Context.User as IGuildUser;
            var roleId = Context.Guild.Roles.FirstOrDefault(r => r.Name == "User").Id;

            Log.Information($"{userGuild.DisplayName} Попытка добавить персонажа");

            if (!userGuild.RoleIds.Contains(roleId))
            {
                await RespondAsync("Надо зарегистрироваться", ephemeral: true);
                return;
            }

            var user = await _userService.FindAsync(Context.User.Id);
            var characterCount = user.Characters.Count;
            if (characterCount >= 10)
            {
                await RespondAsync("Максимум 10 персонажей", ephemeral: true);
                return;
            }

            var character = new Character
            {
                Elo = 1000,
                User = user
            };
            var createdCharacter = await _characterService.CreateCharacterAsync(character);
            if (createdCharacter != null)
            {
                var selectMenu1 = new SelectMenuBuilder()
                    .WithCustomId($"createCharId-{createdCharacter.Id}-1")
                    .WithPlaceholder("Выберите класс - 1 страница")
                    .WithMinValues(1)
                    .WithMaxValues(1);

                var selectMenu2 = new SelectMenuBuilder()
                    .WithCustomId($"createCharId-{createdCharacter.Id}-2")
                    .WithPlaceholder("Выберите класс - 2 страница")
                    .WithMinValues(1)
                    .WithMaxValues(1);

                var classes = await _characterService.GetCharacterClassesAsync();

                var classPage1 = classes.Take(25).ToList();
                var classPage2 = classes.Skip(25).Take(2).ToList();

                foreach (var charClass in classPage1)
                    selectMenu1.AddOption(charClass.Name, charClass.Id.ToString());
                foreach (var charClass in classPage2)
                    selectMenu2.AddOption(charClass.Name, charClass.Id.ToString());

                var row1 = new ActionRowBuilder().WithSelectMenu(selectMenu1);
                var row2 = new ActionRowBuilder().WithSelectMenu(selectMenu2);
                var rows = new List<ActionRowBuilder>() { row1, row2 };

                var builder = new ComponentBuilder()
                    .WithRows(rows).Build();

                await RespondAsync("Выберите класс персонажа", ephemeral: true, components: builder);
            }
            else
            {
                Log.Information($"{userGuild.DisplayName} Ошибка добавления персонажа");
                await RespondAsync("Ошибка", allowedMentions: null, ephemeral: true);
            }
        }
        [SlashCommand("select-character", "Выбрать своего персонажа")]
        public async Task SelectCharacter()
        {
            var userGuild = Context.User as IGuildUser;
            var roleId = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Player").Id;
            var userDb = await _userService.FindAsync(Context.User.Id);

            Log.Information($"{userGuild.DisplayName} Попытка выбрать персонажа");

            if (userDb.IsInMatch)
            {
                await RespondAsync("Вы участвуете в формировании матча", ephemeral: true);
                return;
            }

            if (!userGuild.RoleIds.Contains(roleId))
            {
                await RespondAsync("Нет персонажей", ephemeral: true);
                return;
            }


            var characters = await _characterService.GetCharactersByUserAsync(Context.User.Id);

            var selectMenu = new SelectMenuBuilder()
                .WithCustomId($"selectChar")
                .WithPlaceholder("Выберите класс")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (var character in characters)
            {
                string classTypeName = character.ClassType != null ? classTypeName = character.ClassType.ToString() : classTypeName = "";
                selectMenu.AddOption($"{character.Class.Name}(" + $"{classTypeName})", character.Id.ToString());
            }

            var builder = new ComponentBuilder()
                .WithSelectMenu(selectMenu);

            await RespondAsync("Выберите персонажа", ephemeral: true, components: builder.Build());
        }

        [SlashCommand("delete-character", "Удалить персонажа")]
        public async Task DeleteCharacter()
        {
            var userGuild = Context.User as IGuildUser;
            var roleId = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Player").Id;
            var userDb = await _userService.FindAsync(Context.User.Id);

            Log.Information($"{userGuild.DisplayName} Попытка удалить персонажа");

            if (userDb.IsInMatch)
            {
                await RespondAsync("Вы участвуете в формировании матча", ephemeral: true);
                return;
            }

            if (!userGuild.RoleIds.Contains(roleId))
            {
                await RespondAsync("Нет персонажей", ephemeral: true);
                return;
            }

            var characters = await _characterService.GetCharactersByUserAsync(Context.User.Id);

            var selectMenu = new SelectMenuBuilder()
                .WithCustomId("deleteChar")
                .WithPlaceholder("Выберите класс")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (var character in characters)
            {
                string classTypeName = character.ClassType != null ? classTypeName = character.ClassType.ToString() : classTypeName = "";
                selectMenu.AddOption($"{character.Class.Name}(" + $"{classTypeName})", character.Id.ToString());
            }

            var builder = new ComponentBuilder()
                .WithSelectMenu(selectMenu);

            await RespondAsync("Выберите персонажа", ephemeral: true, components: builder.Build());
        }
    }
}
