using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddMassTransit(x =>    //MassTransit configuration
{
	x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();    //where MassTransit can find the consumers

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));	//set the name of the exchange in RabbitMQ

	x.UsingRabbitMq((context, cfg) =>
	{
		cfg.ReceiveEndpoint("search-auction-created", e =>  //retry policies, if it fails to save an message from EventBus/RabbitMQ into MongoDB retries the process
        {
			e.UseMessageRetry(r => r.Interval(5, 5));   //retries 5 times with 5sec interval

            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
		});

		cfg.ConfigureEndpoints(context);
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
	try
	{
		await DbInitializer.InitDb(app);
	}
	catch (Exception e)
	{
		Console.WriteLine(e);
	}
});

app.Run();

//repeat, repeat and repeat the http request(AuctionServiceHttpClient) until the data is available/it succeeds and get a successful response back from the auction service, http polling 
//create a policy and handle the response based on what happens
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
	=> HttpPolicyExtensions
		.HandleTransientHttpError() //handle a transient error/exception
		.OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound) //handle NotFound exception
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));    //keep trying every 3sec until auction service is back up