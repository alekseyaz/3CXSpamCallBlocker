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

        //static string ConnectionAsString(ActiveConnection ac)
        //{
        //    return $"ID={ac.ID}:CCID={ac.CallConnectionID}:S={ac.Status}:DN={ac.DN.Number}:EP={ac.ExternalParty}:REC={ac.RecordingState}";
        //}

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

        //static void CheckDNCall(Dictionary<ActiveConnection, ActiveConnection[]> ownertoparties)
        //{

        //    string lastVerifiedGoodNumber = ""; //flag

        //    try
        //    {
        //        foreach (var kv in ownertoparties)
        //        {
        //            //Console.WriteLine($"PrintDNCall Call {kv.Key.CallID}:");
        //            //var owner = kv.Key;
        //            //Console.ForegroundColor = ConsoleColor.Green;
        //            //Console.WriteLine($"    {ConnectionAsString(owner)}");
        //            //Console.ResetColor();
        //            //foreach (var party in kv.Value)
        //            //{
        //            //    Console.WriteLine($"    {ConnectionAsString(party)}");
        //            //}
        //            var ExternalParty = kv.Key.ExternalParty;

        //            if (lastVerifiedGoodNumber != ExternalParty)
        //            {

        //                if (WhoCalling.ThisBadCall(ExternalParty))
        //                {
        //                    kv.Key.Drop();
        //                    Program.MyLogger.Info($"{ExternalParty} Drop call");
        //                    break;
        //                }
        //                else
        //                {
        //                    lastVerifiedGoodNumber = ExternalParty;
        //                    Program.MyLogger.Error($"{ExternalParty} white number");
        //                }
        //            }

        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}
        
        public static void Run()
        {

            //PrintAllCalls();

            string lastVerifiedGoodNumber = ""; //flag

            using var dn = PhoneSystem.Root.GetDNByNumber("10001");
            while (!Program.Stop)
            {

                using (var connections = dn.GetActiveConnections().GetDisposer())
                {
                    var alltakenconnections = connections.ToDictionary(x => x, y => y.OtherCallParties);
                    try
                    {
                        foreach (var kv in alltakenconnections)
                        {

                            var ExternalParty = kv.Key.ExternalParty;

                            if (lastVerifiedGoodNumber != ExternalParty)
                            {

                                if (WhoCalling.ThisBadCall(ExternalParty))
                                {
                                    kv.Key.Drop();
                                    Program.MyLogger.Info($"{ExternalParty} drop call");
                                    break;
                                }
                                else
                                {
                                    lastVerifiedGoodNumber = ExternalParty;
                                    Program.MyLogger.Error($"{ExternalParty} good call");
                                }
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }

                    //CheckDNCall(alltakenconnections);
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

