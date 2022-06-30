using System;
using System.IO;
using System.Threading.Tasks;
using NServiceBus;
using Microsoft.Extensions.Configuration;

class Program
{
    static async Task Main()
    {


        var builder = new ConfigurationBuilder()
               // .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false);

        IConfiguration config = builder.Build();


        Console.Title = "Samples.ASB.Publisher";

        var endpointConfiguration = new EndpointConfiguration("Samples.ASB.Publisher");
        
        endpointConfiguration.EnableInstallers("shiras");
        endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
        endpointConfiguration.Conventions().DefiningEventsAs(type => type.Name == nameof(EventTwo) || type.Name == nameof(EventOne));

        var connectionString = config.GetConnectionString("DefaultConnection");
       // var connectionString = "Endpoint=sb://shassan.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=w6R0+ASmMNG/Y2VrsE1n8byXRWj3EEmpoKjge+2PiKc="; //Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
        { 
            throw new Exception("Could not read the 'AzureServiceBus_ConnectionString' environment variable. Check the sample prerequisites.");
        }

        try
        {
            var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
            transport.ConnectionString(connectionString);
            // transport.TopicName("abhas");

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);
            Console.WriteLine("Press any key to publish events");
            Console.ReadKey();
            Console.WriteLine();

            await endpointInstance.Publish(new EventOne
            {
                Content = $"{nameof(EventOne)} sample content",
                PublishedOnUtc = DateTime.UtcNow
            });

            await endpointInstance.Publish(new EventTwo
            {
                Content = $"{nameof(EventTwo)} sample content",
                PublishedOnUtc = DateTime.UtcNow
            });

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
       catch(Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            Console.WriteLine(e);
        }

        Console.ReadKey();
    }
}
