using System.Text;
using Api;
using Api.Services.AI;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Api.Services.Auth;
using Api.Services.Password;
using Api.Services.Playlist;
using Api.Services.R2;
using Api.Services.Song;
using Api.Services.Token;
using Api.Services.User;
using FHHelper;
using GeniusLyrics.NET;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost", "http://173.212.255.171")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
}); 

builder.Services.AddDbContext<MusicDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication("JwtAuth")
    .AddJwtBearer("JwtAuth", options =>
    {
        var jwtSection = builder.Configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);
        var issuer = jwtSection["Issuer"]!;
        var audience = jwtSection["Audience"]!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                

                var token = context.Request.Cookies["jwt"];
                if (!string.IsNullOrEmpty(token))
                    context.Token = token;

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication Failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });


var featureHubUrl = builder.Configuration["FeatureHub:Url"];
var sdkKey = builder.Configuration["FeatureHub:SdkKey"];

builder.Services.AddSingleton<IFeatureStateProvider>(
    new FeatureStateProvider(featureHubUrl!, sdkKey!)
);

builder.Services.AddHttpClient("AiBackend", client =>
{
    var url = builder.Configuration["AiBackend:BaseUrl"] 
              ?? throw new InvalidOperationException("AiBackend:BaseUrl is not configured");
    client.BaseAddress = new Uri(url);
    client.Timeout = TimeSpan.FromMinutes(10);
});

var geniusApiKey = builder.Configuration["Genius:APIKey"];
if (!string.IsNullOrEmpty(geniusApiKey))
    builder.Services.AddSingleton(new GeniusClient(geniusApiKey));
else
    builder.Services.AddSingleton<GeniusClient>(_ => null!);

builder.Services.AddScoped<ISeeder, Seeder>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IR2Service, R2Service>();
builder.Services.AddScoped<ISongService, SongService>();
builder.Services.AddScoped<IPlaylistService, PlaylistService>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<ISongMetadataService, SongMetadataService>();
builder.Services.AddControllers();
builder.Services.AddOpenApiDocument();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MusicDbContext>();
    await db.Database.MigrateAsync();
    
    var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
    await seeder.Seed();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseOpenApi();
app.UseSwaggerUi();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
