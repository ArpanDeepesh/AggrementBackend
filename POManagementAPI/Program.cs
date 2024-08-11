using BusinessLogic;
using Microsoft.EntityFrameworkCore;
using POManagementAPI.Helper;
using POManagementAPI.Services;
using POManagementDataAccessLayer.DataAccessLayer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
var connectionString = builder.Configuration.GetSection("AppSettings")["ConnectionString"];
builder.Services.AddDbContext<poMngtSQLContext>(
    dbContextOptions => dbContextOptions
        .UseMySQL(connectionString)
        // The following three options help with debugging, but should
        // be changed or removed for production.
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors()
);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IPOManagerService, POManagementService>();
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAllPolicy", policy => {
        policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});
var app = builder.Build();
app.UseCors("AllowAllPolicy");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<JWTMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
