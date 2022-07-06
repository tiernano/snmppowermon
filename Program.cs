using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Microsoft.Extensions.Logging;

class Program {
    static async Task Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
                .AddConsole();
        });
        ILogger logger = loggerFactory.CreateLogger<Program>();
        
        string identifier = "1.3.6.1.4.1.318.1.1.12.1.16.0";
        var identifierEnv = Environment.GetEnvironmentVariable("SNMPIdentifier");
        if(!string.IsNullOrEmpty(identifierEnv)){
            logger.LogDebug($"Env Found for Identifier: {identifierEnv}");
            identifier = identifierEnv;
        }

        string IPAddress = "10.244.10.10";
        var IPAddressEnv = Environment.GetEnvironmentVariable("SNMPIPAddress");
        if(!string.IsNullOrEmpty(IPAddressEnv)){
            logger.LogDebug($"Env Found for IPAddressEnv: {IPAddressEnv}");
            IPAddress = IPAddressEnv;
        }

        string community = "public";

        var communityEnv = Environment.GetEnvironmentVariable("SNMPcommunity");
        if(!string.IsNullOrEmpty(communityEnv)){
            logger.LogDebug($"Env Found for communityEnv: {communityEnv}");
            community = communityEnv;
        }


        string dataFileName =  Path.Combine("data", "data.txt");
        string runningTotalFileName = Path.Combine("data", "total.txt");

        decimal totalWHs = 0;
        if(File.Exists(runningTotalFileName))
        {
            var data = await File.ReadAllTextAsync(runningTotalFileName);

            if(Decimal.TryParse(data, out totalWHs)){
                logger.LogInformation($"last result: {totalWHs}");
            }
        }

        AppDomain.CurrentDomain.ProcessExit +=  (_, _) => {
            logger.LogInformation("Process Exist...");
            WriteToFile(dataFileName, runningTotalFileName, totalWHs, 0, 0).GetAwaiter().GetResult();
            logger.LogInformation("Data Written");
        };

        Console.CancelKeyPress +=  (_,_) => {
            logger.LogInformation("Cancel Key Press");
            WriteToFile(dataFileName, runningTotalFileName, totalWHs, 0, 0).GetAwaiter().GetResult();
            logger.LogInformation("Data Written");
        };

        while(true)
        {

            var data = await Messenger.GetAsync(VersionCode.V1, 
                new System.Net.IPEndPoint(System.Net.IPAddress.Parse(IPAddress), 161),
                new OctetString(community),
                new List<Variable> { new Variable(new ObjectIdentifier(identifier)) });

           
            decimal watts;
            decimal wattHs = 0;

            if(decimal.TryParse(data.First().Data.ToString(), out watts))
            {
                wattHs = Math.Round(watts / 240, 2);
            }
            totalWHs += wattHs;

            await WriteToFile(dataFileName, runningTotalFileName, totalWHs, watts, wattHs);
           
            logger.LogInformation($"{DateTime.Now} - Current Watts: {watts}, current wHs: {wattHs}. Running Total: {totalWHs}");
            Thread.Sleep(TimeSpan.FromSeconds(15));
        }
    }

    static async Task WriteToFile(string dataFileName, string runningTotalFileName, decimal totalWHs, decimal watts, decimal wattHs)
    {
            await File.WriteAllTextAsync(runningTotalFileName, totalWHs.ToString());
            File.AppendAllText(dataFileName, $"{DateTime.Now.Ticks} - {watts} - {wattHs}{Environment.NewLine}");
    }
}
