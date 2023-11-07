using DSED_Module03_Preparation_Cours.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DSED_Module03_Preparation_Cours
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddRazorPages();

            // Ajoute les services requis pour NSwag
            builder.Services.AddSwaggerDocument();

            // Ajout de vos correspondances pour l'injection de dépendances

            // Pour avoir un objet à chaque injection
            //builder.Services.AddTransient<IInterface, ClasseConcrete>();

            // Pour avoir la même instance de classe sur tout le traitement d'une requête
            //builder.Services.AddScoped<IInterface, ClasseConcrete>();

            // Pour avoir la même instance de classe tout au long de l'exécution de l'application
            //builder.Services.AddSingleton<IInterface, ClasseConcrete>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.Run();
        }
    }
}