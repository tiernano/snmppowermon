using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program {
    static async Task Main(string[] args)
    {
        string identifier = "1.3.6.1.4.1.318.1.1.12.1.16.0";
        string IPAddress = "10.244.10.10";
        string community = "public";
        string dataFileName = "data.txt";
        string runningTotalFileName = "total.txt";

        decimal totalWHs = 0;
        if(File.Exists(runningTotalFileName))
        {
            var data = await File.ReadAllTextAsync(runningTotalFileName);

            if(Decimal.TryParse(data, out totalWHs)){
                Console.WriteLine($"last result: {totalWHs}");
            }
        }

        while(true){

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

            await File.WriteAllTextAsync(runningTotalFileName, totalWHs.ToString());
            File.AppendAllText(dataFileName, $"{DateTime.Now.Ticks} - {watts} - {wattHs}{Environment.NewLine}");

            Console.WriteLine($"{DateTime.Now} - Current Watts: {watts}, current wHs: {wattHs}. Running Total: {totalWHs}");
            Thread.Sleep(TimeSpan.FromSeconds(15));
        }


    }
}
