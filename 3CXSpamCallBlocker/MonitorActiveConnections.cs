using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCX.Configuration;

namespace _3CXSpamCallBlocker
{
    static class MonitorActiveConnections
    {
        static string ConnectionAsString(ActiveConnection ac)
        {
            return $"ID={ac.ID}:CCID={ac.CallConnectionID}:S={ac.Status}:DN={ac.DN.Number}:EP={ac.ExternalParty}:REC={ac.RecordingState}";
        }

        static void PrintAllCalls()
        {
            foreach (var c in PhoneSystem.Root.GetActiveConnectionsByCallID())
            {
                Console.ResetColor();
                Console.WriteLine($"Call {c.Key}:");
                foreach (var ac in c.Value.OrderBy(x => x.CallConnectionID))
                {
                    Console.WriteLine($"    {ConnectionAsString(ac)}");
                }
            }
        }

        static void PrintDNCall(Dictionary<ActiveConnection, ActiveConnection[]> ownertoparties)
        {
            try
            {
                foreach (var kv in ownertoparties)
                {
                    Console.WriteLine($"Call {kv.Key.CallID}:");
                    var owner = kv.Key;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"    {ConnectionAsString(owner)}");
                    Console.ResetColor();
                    foreach (var party in kv.Value)
                    {
                        Console.WriteLine($"    {ConnectionAsString(party)}");
                    }
                }
            }
            finally
            {
                Console.ResetColor();
            }
        }
        
        public static void Run()
        {
            var ps = PhoneSystem.Root;
            //var calls = PhoneSystem.Root.GetActiveConnectionsByCallID();

            PhoneSystem.Root.GetByID<ActiveConnection>(00).Drop();

            PrintAllCalls();

            using (var dn = PhoneSystem.Root.GetDNByNumber("00"))
            {
                using (var connections = dn.GetActiveConnections().GetDisposer())
                {
                    var alltakenconnections = connections.ToDictionary(x => x, y => y.OtherCallParties);
                    PrintDNCall(alltakenconnections);
                    foreach (var a in alltakenconnections.Values)
                    {
                        a.GetDisposer().Dispose();
                    }
                }
            }


    
        }
    }

}

