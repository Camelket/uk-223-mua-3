using L_Bank_W_Backend.DbAccess;
using Microsoft.EntityFrameworkCore;

namespace L_Bank.Api;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;
    private readonly string AllowSpecificOrigins = "_AllowSpecificOrigins";

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                Configuration.GetConnectionString("ConnectionString"),
                x => x.MigrationsAssembly("L-Bank.DbAccess")
            )
        );

        /* ---
        add dependency injections
        --- */
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseCors(AllowSpecificOrigins);

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
