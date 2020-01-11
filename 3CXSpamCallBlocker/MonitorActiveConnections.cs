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
      
        public static void Run()
        {

            string lastVerifiedGoodNumber = ""; //flag

            using var dn = PhoneSystem.Root.GetDNByNumber("10001"); //We get the DN object by sip gateway number
            while (!Program.Stop)
            {

                using (var connections = dn.GetActiveConnections().GetDisposer())
                {
                    try
                    {
                        foreach (var ac in connections)
                        {

                            var ExternalParty = ac.ExternalParty;

                            if (lastVerifiedGoodNumber != ExternalParty)
                            {

                                if (WhoCalling.ThisBadCall(ExternalParty))
                                {
                                    ac.Drop();
                                    Program.MyLogger.Info($"{ExternalParty}");
                                    break;
                                }
                                else
                                {
                                    lastVerifiedGoodNumber = ExternalParty;
                                    Program.MyLogger.Error($"{ExternalParty}");
                                }
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Program.MyLogger.Fatal(e, "exceptions");
                    }

                    //foreach (var a in connections)
                    //{
                    //    a.Dispose();
                    //}

                }

                Thread.Sleep(100);
            }

        }
    }

}

