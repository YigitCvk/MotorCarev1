# Skill: Implement .NET MAUI Frontend

## Objective
Build the cross-platform SaaS frontend using .NET MAUI with MVVM pattern.

## Instructions

1. **Read Specification**: Load `production_artifacts/Technical_Specification.md` and API specs.

2. **Scaffold MAUI Project**:
   ```bash
   dotnet new maui -n SaaSApp.Maui -f net8.0
   ```
   Add to the solution and reference shared DTOs from Application layer.

3. **Project Structure**:
   ```
   SaaSApp.Maui/
   ├── App.xaml / App.xaml.cs
   ├── AppShell.xaml                    # Shell navigation
   ├── MauiProgram.cs                   # DI configuration
   ├── Views/
   │   ├── LoginPage.xaml
   │   ├── DashboardPage.xaml
   │   └── {Feature}Page.xaml
   ├── ViewModels/
   │   ├── BaseViewModel.cs
   │   ├── LoginViewModel.cs
   │   ├── DashboardViewModel.cs
   │   └── {Feature}ViewModel.cs
   ├── Services/
   │   ├── IApiService.cs              # Refit interface
   │   ├── INavigationService.cs
   │   ├── AuthService.cs
   │   └── LocalCacheService.cs        # SQLite
   ├── Models/
   │   └── (client-side DTOs)
   ├── Resources/
   │   ├── Styles/
   │   │   ├── Colors.xaml
   │   │   └── Styles.xaml
   │   ├── Fonts/
   │   └── Images/
   └── Converters/
       └── BoolToColorConverter.cs
   ```

4. **Base ViewModel**:
   ```csharp
   public partial class BaseViewModel : ObservableObject
   {
       [ObservableProperty] private bool _isBusy;
       [ObservableProperty] private string _title = string.Empty;
       [ObservableProperty] private string? _errorMessage;

       protected async Task ExecuteAsync(Func<Task> operation)
       {
           if (IsBusy) return;
           try
           {
               IsBusy = true;
               ErrorMessage = null;
               await operation();
           }
           catch (HttpRequestException ex)
           {
               ErrorMessage = "Bağlantı hatası. Lütfen tekrar deneyin.";
           }
           catch (Exception ex)
           {
               ErrorMessage = "Beklenmeyen bir hata oluştu.";
           }
           finally
           {
               IsBusy = false;
           }
       }
   }
   ```

5. **API Integration with Refit**:
   ```csharp
   public interface IApiService
   {
       [Get("/api/v1/{endpoint}")]
       Task<ApiResponse<T>> GetAsync<T>(string endpoint);

       [Post("/api/v1/{endpoint}")]
       Task<ApiResponse<T>> PostAsync<T>(string endpoint, [Body] object body);
   }
   ```

6. **DI Registration** (MauiProgram.cs):
   ```csharp
   builder.Services.AddRefitClient<IApiService>()
       .ConfigureHttpClient(c => c.BaseAddress = new Uri(Settings.ApiBaseUrl));
   builder.Services.AddTransient<LoginViewModel>();
   builder.Services.AddTransient<LoginPage>();
   ```

## NuGet Packages
```xml
CommunityToolkit.Mvvm
CommunityToolkit.Maui
Refit, Refit.HttpClientFactory
sqlite-net-pcl          <!-- offline cache -->
```

## Rules
- ZERO code-behind logic (except InitializeComponent and navigation wiring)
- All API calls wrapped in try-catch with user-friendly Turkish error messages
- Loading indicators (ActivityIndicator) on every async operation
- CollectionView over ListView, always
- Use Shell routing: `await Shell.Current.GoToAsync("//dashboard")`
- Support minimum: Android 8.0+ and Windows 10+
- Test on both platforms before marking complete
