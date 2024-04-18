using BiddingService.Consumers;
using BiddingService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Driver;
using MongoDB.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHostedService<CheckAuctionFinished>();
builder.Services.AddScoped<GrpcAuctionClient>();

builder.Services.AddMassTransit(x =>    //MassTransit configuration
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();    //add consumer

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("bids", false));   //set the name of the exchange in RabbitMQ

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });

        cfg.ConfigureEndpoints(context);    //Configure the endpoints for all defined consumer, saga, and activity types using an optional endpoint name formatter.
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)  //Authentication config
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServiceUrl"];    //set Authority
        options.RequireHttpsMetadata = false;   //false because IdentityServer is running on HTTP
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

await DB.InitAsync("BidDb", MongoClientSettings.FromConnectionString(builder.Configuration.GetConnectionString("BidDbConnection")));

app.Run();
