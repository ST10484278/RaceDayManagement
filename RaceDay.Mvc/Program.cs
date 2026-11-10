using RaceDay.Mvc.Services;

namespace RaceDay.Mvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // 1. Session stores the MVC user's ID, name and role between page requests.
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // 2. Allows a service class (like RaceDayApiClient) to read the current MVC session.
            builder.Services.AddHttpContextAccessor();

            // 3. Register one named HttpClient for the Part 2 API.
            builder.Services.AddHttpClient("RaceDayApi", client =>
            {
                string baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                    ?? throw new InvalidOperationException("ApiSettings:BaseUrl is missing.");
                client.BaseAddress = new Uri(baseUrl);
            });

            // 4. Register RaceDayApiClient in Dependency Injection.
            builder.Services.AddScoped<RaceDayApiClient>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // IMPORTANT: Session must be activated AFTER UseRouting and BEFORE mapped controller endpoints[cite: 7].
            app.UseSession();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}