using Microsoft.EntityFrameworkCore;
using MitRestApi.Data;
using MitRestApi.Converters;

var builder = WebApplication.CreateBuilder(args);

// CORS policy navn
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Tilføj CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5500") // Din frontend URL
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Tilføj Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Konfigurer controller + JSON options (med din date converter)
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new SimpleDateConverter());
    });

// DB konfiguration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=MitRestApiDb;Username=postgres;Password=313300"));

var app = builder.Build();

// Initier databasen
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbInitializer.Initialize(dbContext);
}

// Middleware rækkefølge er vigtig
app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);

// Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
