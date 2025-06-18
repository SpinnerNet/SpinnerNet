namespace SpinnerNet.Maui;

/// <summary>
/// Main application class for Spinner.Net MAUI Blazor Hybrid app
/// Implements local-first data sovereignty with cross-platform support
/// </summary>
public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		
		// Initialize database asynchronously on app startup
		Task.Run(async () => await InitializeDatabaseAsync());
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new MainPage()) 
		{ 
			Title = "Spinner.Net - AI Time Companion"
		};
	}
	
	/// <summary>
	/// Initialize local SQLite database for offline-first data sovereignty
	/// </summary>
	private async Task InitializeDatabaseAsync()
	{
		try
		{
			// Get database initializer from DI container
			using var scope = Handler.MauiContext?.Services.CreateScope();
			var databaseInitializer = scope?.ServiceProvider.GetService<IDatabaseInitializer>();
			
			if (databaseInitializer != null)
			{
				await databaseInitializer.InitializeAsync();
			}
		}
		catch (Exception ex)
		{
			// Log error but don't crash the app
			System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
		}
	}
}
