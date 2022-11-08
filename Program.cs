using Tracker.Data;
using Microsoft.EntityFrameworkCore;
using Tracker.Models;
using Tracker.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlite<TrackerContext>("Data Source=Tracker.db");
builder.Services.AddTransient<DbInitializer>();

builder.Services.AddScoped<ICheckinService, CheckinService>();
builder.Services.AddScoped<ICheckpointService, CheckpointService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IMonitorService, MonitorService>();
builder.Services.AddScoped<IParticipantService, ParticipantService>();
builder.Services.AddScoped<IRaceService, RaceService>();
builder.Services.AddScoped<ISegmentService, SegmentService>();
builder.Services.AddScoped<IWatcherService, WatcherService>();
builder.Services.AddScoped<ILeaderService, LeaderService>();
builder.Services.AddScoped<IAlertMessageService, AlertMessageService>();
builder.Services.AddSingleton<SlackService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string Auth0Domain = builder.Configuration["Auth0Config:Domain"];
string Auth0Audience = builder.Configuration["Auth0Config:Audience"];
string Auth0ClientId = builder.Configuration["Auth0Config:ClientId"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = Auth0Domain;
        options.Audience = Auth0Audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidAudience = Auth0Audience,
            ValidIssuer = Auth0Domain,
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });

builder.Services
    .AddAuthorization(options =>
    {
        options.AddPolicy("admin", policy => policy.RequireAuthenticatedUser().RequireClaim("scope", "admin"));
    });

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Tracker API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapGet("/seed", (DbInitializer initializer) =>
{
    initializer.Initialize();
}).RequireAuthorization();

app.MapGet("/races", async (IRaceService raceService) =>
{
    return await raceService.GetRacesAsync();
});

app.MapGet("/races/{raceId}", async (Guid raceId, IRaceService raceService) =>
{
    return await raceService.GetRaceAsync(raceId);
}).RequireAuthorization();

app.MapGet("/segments", async (ISegmentService segmentService) =>
{
    return await segmentService.GetSegmentsAsync();
});

app.MapGet("/segments/{segmentId}", async (Guid segmentId, ISegmentService segmentService) =>
{
    return await segmentService.GetSegmentAsync(segmentId);
});

app.MapGet("/checkpoints", async (ICheckpointService checkpointService) =>
{
    return await checkpointService.GetCheckpointsAsync();
});

app.MapGet("/checkpoints/{checkpointId}", async (Guid checkpointId, ICheckpointService checkpointService) =>
{
    return await checkpointService.GetCheckpointAsync(checkpointId);
});

app.MapGet("/participants", async (IParticipantService participantService) =>
{
    return await participantService.GetParticipantsAsync();
});

app.MapGet("/participants/{participantId}", async (Guid participantId, IParticipantService participantService) =>
{
    return await participantService.GetParticipantAsync(participantId);
});

app.Run();