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

        //static void PrintAllCalls()
        //{
        //    foreach (var c in PhoneSystem.Root.GetActiveConnectionsByCallID())
        //    {
        //        Console.ResetColor();
        //        Console.WriteLine($"PrintAllCalls Call {c.Key}:");
        //        foreach (var ac in c.Value.OrderBy(x => x.CallConnectionID))
        //        {
        //            Console.WriteLine($"    {ConnectionAsString(ac)}");
        //        }
        //    }
        //}

        static void CheckDNCall(Dictionary<ActiveConnection, ActiveConnection[]> ownertoparties)
        {
            try
            {
                foreach (var kv in ownertoparties)
                {
                    Console.WriteLine($"PrintDNCall Call {kv.Key.CallID}:");
                    var owner = kv.Key;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"    {ConnectionAsString(owner)}");
                    Console.ResetColor();
                    foreach (var party in kv.Value)
                    {
                        Console.WriteLine($"    {ConnectionAsString(party)}");
                    }

                    

                    if (WhoCalling.ThisBadCall(owner.ExternalParty))
                    {
                        kv.Key.Drop();
                        Program.MyLogger.Info($"{owner.ExternalParty} drop call");
                        break;
                    }

                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        public static void Run()
        {
            //var ps = PhoneSystem.Root;
            //var calls = PhoneSystem.Root.GetActiveConnectionsByCallID();

            //PhoneSystem.Root.GetByID<ActiveConnection>(int.Parse(args[2])).Drop();

            //try
            //{
            //    ac.Drop();
            //}
            //catch
            //{
            //    Console.WriteLine("fdfdfd");
            //}

            //PrintAllCalls();

 

                
                 
                using (var dn = PhoneSystem.Root.GetDNByNumber("10001"))
                {

                while (!Program.Stop)
                {

                    using (var connections = dn.GetActiveConnections().GetDisposer())
                    {

                        var alltakenconnections = connections.ToDictionary(x => x, y => y.OtherCallParties);
                        CheckDNCall(alltakenconnections);
                        foreach (var a in alltakenconnections.Values)
                        {
                            a.GetDisposer().Dispose();
                        }
                    }

                    Thread.Sleep(100);
                }

            }

        }
    }

}

