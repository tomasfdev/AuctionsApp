using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(x =>    //MassTransit configuration
{
    x.AddEntityFrameworkOutbox<AuctionDbContext>(opt => //Outbox service config
    {
        opt.QueryDelay = TimeSpan.FromSeconds(10);  //If EventBus/RabbitMQ is down, every 10secs it will check the outbox for any messages to deliver and resend them

        opt.UsePostgres();
        opt.UseBusOutbox();
    });

    x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();   //where MassTransit can find the consumers

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));   //set the name of the exchange in RabbitMQ

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);    //Configure the endpoints for all defined consumer, saga, and activity types using an optional endpoint name formatter.
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e);
}

app.Run();
