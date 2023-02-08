using Consul;
using Microsoft.Extensions.Options;

internal class ConsulServiceRegister : IHostedService, IConsulServiceRegister
{
    private readonly CommandConfig commandCfg;
    private readonly ConsulConfig consulCfg;
    private readonly IConsulClient client;

    public ConsulServiceRegister(IConsulClient client,
    IOptions<CommandConfig> commandCfg,
    IOptions<ConsulConfig> consulCfg)
    {
        this.consulCfg = consulCfg.Value;
        this.commandCfg = commandCfg.Value;
        this.client = client;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var commandUri = new Uri(commandCfg.Url);
        var agentServiceRegistration = new AgentServiceRegistration()
        {
            Address = commandUri.Host,
            Port = commandUri.Port,
            Name = commandCfg.ServiceName,
            ID = commandCfg.ServiceId

        };
        try
        {            
            Console.WriteLine($"--> Starting to register {agentServiceRegistration.Address}");
            await client.Agent.ServiceDeregister(commandCfg.ServiceId, cancellationToken);
            var r = await client.Agent.ServiceRegister(agentServiceRegistration, cancellationToken);
            Console.WriteLine($" Service registered status:{r.StatusCode}");
        }
        catch (System.Net.Http.HttpRequestException ex)
        {
            Console.WriteLine($"Connection err occured on consul: {ex}");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await client.Agent.ServiceDeregister(commandCfg.ServiceId, cancellationToken);
        }

        catch (System.Net.Http.HttpRequestException)
        {
        }

        catch (Exception ex)
        {
            System.Console.WriteLine("deregister error: " + ex);
        }
    }
}