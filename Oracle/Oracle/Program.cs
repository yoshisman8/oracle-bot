using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Oracle.Services;
using Interactivity;

namespace Oracle
{
	class Program
	{
		static void Main(string[] args)
			=> new Program().MainAsync().GetAwaiter().GetResult();

		private DiscordSocketClient _client;
		private IConfiguration _config;

		public async Task MainAsync()
		{
			Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "data"));
			Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp"));

			_client = new DiscordSocketClient();
			_config = BuildConfig();

			var services = ConfigureServices();
			services.GetRequiredService<LoggingService>();
			await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);

			await _client.LoginAsync(TokenType.Bot, _config["token"]);
			await _client.StartAsync();
			await _client.SetGameAsync("With Magic | !Help");

			await Task.Delay(-1);
		}

		private IServiceProvider ConfigureServices()
		{
			return new ServiceCollection()
				// Base
				.AddSingleton(_client)
				.AddSingleton(new CommandService(new CommandServiceConfig()
				{
					DefaultRunMode = RunMode.Async,
					CaseSensitiveCommands = false
				})
				)
				.AddSingleton<CommandHandlingService>()
				// Logging
				.AddLogging()
				.AddSingleton<LoggingService>()
				// Extra
				.AddSingleton(_config)
				.AddSingleton(new InteractivityService(_client, TimeSpan.FromMinutes(1)))
				// Add additional services here...
				.AddSingleton(new LiteDatabase("Filename=" + Path.Combine(Directory.GetCurrentDirectory(), "data", "Database.db") + "; Async=true"))
				.AddSingleton<Utilities>()
				.BuildServiceProvider();
		}

		private IConfiguration BuildConfig()
		{
			return new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config.json"))
				.Build();
		}
	}
}