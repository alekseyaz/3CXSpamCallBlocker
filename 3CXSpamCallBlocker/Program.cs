using System;
using System.Collections.Generic;
using System.Text;
using TCX.Configuration;
using System.IO;
using System.Reflection;
using System.Linq;
using NLog;
using NLog.Targets;
using NLog.Config;


namespace _3CXSpamCallBlocker
{
    class Program
    {
        static Dictionary<string, Dictionary<string, string>> iniContent =
            new Dictionary<string, Dictionary<string, string>>(
                StringComparer.InvariantCultureIgnoreCase);

        public static bool Stop { get; private set; }

        public static readonly Logger MyLogger = LogManager.GetLogger("3CXSpamCallBlocker");

        private static void ConfigurationLogger()
        {

            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget("target1")
            {
                Layout = @"${date:format=HH\:mm\:ss} ${message} ${exception}"
            };
            config.AddTarget(consoleTarget);

            var fileTargetWhiteNumber = new FileTarget("target2")
            {
                FileName = "${basedir}/DropCallLogs/${shortdate} White number.txt",
                Layout = "${longdate} ${message}  ${exception}"
            };
            config.AddTarget(fileTargetWhiteNumber);

            var fileTargetBlackNumber = new FileTarget("target3")
            {
                FileName = "${basedir}/DropCallLogs/${shortdate} Black number.txt",
                Layout = "${longdate} ${message}  ${exception}"
            };
            config.AddTarget(fileTargetBlackNumber);

            var fileTargetAll = new FileTarget("target4")
            {
                FileName = "${basedir}/DropCallLogs/${shortdate} All.txt",
                Layout = "${longdate} ${message}  ${exception}"
            };
            config.AddTarget(fileTargetAll);

            var fileTargetFatalError = new FileTarget("target5")
            {
                FileName = "${basedir}/DropCallLogs/${shortdate} Error.txt",
                Layout = "${longdate} ${message}  ${exception}"
            };
            config.AddTarget(fileTargetFatalError);


            config.AddRuleForOneLevel(LogLevel.Info, fileTargetBlackNumber, "DropCall");
            config.AddRuleForOneLevel(LogLevel.Error, fileTargetWhiteNumber, "DropCall");
            config.AddRuleForOneLevel(LogLevel.Fatal, fileTargetFatalError, "DropCall");
            config.AddRuleForAllLevels(fileTargetAll, "DropCall");
            config.AddRuleForOneLevel(LogLevel.Info, consoleTarget, "DropCall");
            config.AddRuleForOneLevel(LogLevel.Error, consoleTarget, "DropCall");
            config.AddRuleForOneLevel(LogLevel.Fatal, consoleTarget, "DropCall");

            LogManager.Configuration = config;
        }
        static void ReadConfiguration(string filePath)
        {
            var content = File.ReadAllLines(filePath);
            Dictionary<string, string> CurrentSection = null;
            string CurrentSectionName = null;
            for (int i = 1; i < content.Length + 1; i++)
            {
                var s = content[i - 1].Trim();
                if (s.StartsWith("["))
                {
                    CurrentSectionName = s.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    CurrentSection = iniContent[CurrentSectionName] = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                }
                else if (CurrentSection != null && !string.IsNullOrWhiteSpace(s) && !s.StartsWith("#") && !s.StartsWith(";"))
                {
                    var res = s.Split("=").Select(x => x.Trim()).ToArray();
                    CurrentSection[res[0]] = res[1];
                }
                else
                {
                    //Console.WriteLine($"Ignore Line {i} in section '{CurrentSectionName}': '{s}' ");
                }
            }
            instanceBinPath = Path.Combine(iniContent["General"]["AppPath"], "Bin");
        }
        static void Bootstrap(string[] args)
        {
            PhoneSystem.CfgServerHost = "127.0.0.1";
            PhoneSystem.CfgServerPort = int.Parse(iniContent["ConfService"]["ConfPort"]);
            PhoneSystem.CfgServerUser = iniContent["ConfService"]["confUser"];
            PhoneSystem.CfgServerPassword = iniContent["ConfService"]["confPass"];
            var ps = PhoneSystem.Reset(
                PhoneSystem.ApplicationName + new Random(Environment.TickCount).Next().ToString(),
                "127.0.0.1",
                int.Parse(iniContent["ConfService"]["ConfPort"]),
                iniContent["ConfService"]["confUser"],
                iniContent["ConfService"]["confPass"]);
            ps.WaitForConnect(TimeSpan.FromSeconds(30));
            try
            {
                MonitorActiveConnections.Run();
            }
            finally
            {
                ps.Disconnect();
            }
        }

        static string instanceBinPath;

        static void Main(string[] args)
        {
            Console.OutputEncoding = new UnicodeEncoding();
            Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);

            

            try
            {
                var filePath = @"C:\Program Files\3CX Phone System\Bin\3CXPhoneSystem.ini";
                if (!File.Exists(filePath))
                {
                    throw new Exception("Cannot find 3CXPhoneSystem.ini");
                }
                ReadConfiguration(filePath);
                ConfigurationLogger();
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                Bootstrap(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        protected static void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            //
            args.Cancel = true;
            Stop = true;
        }
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;
            try
            {
                return Assembly.LoadFrom(Path.Combine(instanceBinPath, name + ".dll"));
            }
            catch
            {
                return null;
            }
        }
    }

}
