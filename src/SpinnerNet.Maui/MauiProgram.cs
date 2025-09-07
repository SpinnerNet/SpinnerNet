using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MudBlazor.Services;
using SpinnerNet.Maui.Extensions;
using Microsoft.EntityFrameworkCore;
using SpinnerNet.Core.Data;
using MediatR;
using FluentValidation;
using System.Reflection;

namespace SpinnerNet.Maui;

/// <summary>
/// MAUI Program configuration for Spinner.Net cross-platform AI companion
/// Implements local-first data sovereignty with optional cloud sync
/// </summary>
public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		
		// Configure MAUI app with custom fonts
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Add Blazor WebView for hybrid functionality
		builder.Services.AddMauiBlazorWebView();
		
		// Add MudBlazor for consistent cross-platform UI
		builder.Services.AddMudServices();
		
		// Configure local-first SQLite database for data sovereignty
		// COMMENTED OUT FOR SPRINT 1 - Using Cosmos DB only, Entity Framework for future sprints
		// ConfigureLocalDatabase(builder.Services);
		
		// Configure SpinnerNet Core services (MediatR, CQRS, Validation)
		ConfigureSpinnerNetServices(builder.Services, builder.Configuration);
		
		// Configure optional Cosmos DB for cloud sync
		ConfigureCloudServices(builder.Services, builder.Configuration);

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
		builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

		return builder.Build();
	}
	
	// COMMENTED OUT FOR SPRINT 1 - Using Cosmos DB only, Entity Framework for future sprints
	/*
	/// <summary>
	/// Configure local SQLite database for offline-first data sovereignty
	/// </summary>
	private static void ConfigureLocalDatabase(IServiceCollection services)
	{
		// Local SQLite database path for each platform
		var databasePath = Path.Combine(FileSystem.AppDataDirectory, "spinnernet-local.db");
		
		services.AddDbContext<SpinnerNetDbContext>(options =>
			options.UseSqlite($"Data Source={databasePath}"));
			
		// Ensure database is created on app startup
		services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
	}
	*/
	
	/// <summary>
	/// Configure SpinnerNet core services following established patterns
	/// </summary>
	private static void ConfigureSpinnerNetServices(IServiceCollection services, IConfiguration configuration)
	{
		// Add MAUI-specific vertical slice services (MediatR + FluentValidation, no controllers)
		services.AddMauiVerticalSliceServices();
	}
	
	/// <summary>
	/// Configure optional Cosmos DB for cloud data sync (respects user data sovereignty)
	/// </summary>
	private static void ConfigureCloudServices(IServiceCollection services, IConfiguration configuration)
	{
		// Only add Cosmos DB if user opts into cloud sync
		var cosmosConnectionString = configuration.GetConnectionString("CosmosDb");
		if (!string.IsNullOrEmpty(cosmosConnectionString))
		{
			var databaseName = configuration["CosmosDb:DatabaseName"] ?? "SpinnerNetDev";
			services.AddMauiCosmosDbRepositories(cosmosConnectionString, databaseName);
		}
	}
}

// COMMENTED OUT FOR SPRINT 1 - Using Cosmos DB only, Entity Framework for future sprints
/*
/// <summary>
/// Database initializer for ensuring local SQLite database is ready
/// </summary>
public interface IDatabaseInitializer
{
	Task InitializeAsync();
}

public class DatabaseInitializer : IDatabaseInitializer
{
	private readonly SpinnerNetDbContext _context;
	private readonly ILogger<DatabaseInitializer> _logger;
	
	public DatabaseInitializer(SpinnerNetDbContext context, ILogger<DatabaseInitializer> logger)
	{
		_context = context;
		_logger = logger;
	}
	
	public async Task InitializeAsync()
	{
		try
		{
			await _context.Database.EnsureCreatedAsync();
			_logger.LogInformation("Local SQLite database initialized successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to initialize local database");
			throw;
		}
	}
}
*/
