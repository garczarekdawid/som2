using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SOM2.Application.Common;
using SOM2.Application.Interfaces;
using SOM2.Application.Services;
using SOM2.Domain.Interfaces;
using SOM2.Infrastructure.Persistence;
using SOM2.Infrastructure.Repositories;
using SOM2.Infrastructure.Workers;

namespace SOM2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddScoped<IManagedHostRepository, ManagedHostRepository>();
            builder.Services.AddScoped<IManagedHostService, ManagedHostService>();

            builder.Services.AddScoped<IHostActionRepository, HostActionRepository>();
            //builder.Services.AddScoped<IHostActionExecutor, HostActionExecutor>();


            builder.Services.Configure<AnsibleOptions>(
            builder.Configuration.GetSection("Ansible"));

            builder.Services.AddScoped<IHostActionExecutor>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<AnsibleOptions>>().Value;
                return new AnsibleHostActionExecutor(options);
            });


            // Zarejestruj brakuj¹cy read-repository u¿ywany przez HostActionQueryService
            builder.Services.AddScoped<IHostActionReadRepository, HostActionReadRepository>();

            builder.Services.AddScoped<IHostActionQueryService, HostActionQueryService>();


            builder.Services.AddHostedService<HostActionWorker>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=ManagedHostsList}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
