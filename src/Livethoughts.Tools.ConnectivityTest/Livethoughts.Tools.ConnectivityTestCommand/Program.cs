using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Livethoughts.Tools.ConnectivityTestCommand
{
    class Program
    {
        static readonly string USAGE_STRING = "USAGE: ConnectivityTest [[-silent]|[-showDebug]|[-hideErrors]|[-hideWarnings]|[-hideInfo]] [CofigurationFilePath] | [[IP_Address] [Port_Number]]\n Example: ConnectivityTest 192.168.1.1 80 \n Example: ConnectivityTest config.json";

        static ConnectivityTestSettings SETTINGS_FILE = null;

        static bool SILENTMODE_ENABLED = false;
        static bool DEBUGMODE_ENABLED = false;
        static bool SHOW_ERRORS = true;
        static bool SHOW_WARNINGS = true;
        static bool SHOW_INFO = true;


        static int Main(string[] args)
        {
            int destinationPort = 0;
            bool wasLoaded;

            string[] arguments = ProcessFlags(args);

            if (arguments.Length == 0)
            {
                wasLoaded = LoadConfigFile(".\\config.json");
                if (!wasLoaded)
                {
                    WriteLineWarning("***MISSING OR INVALID DEFAULT SETTINGS FILE (config.json)***");
                    WriteLineWarning(USAGE_STRING);
                    return -1;
                }

            }
            else if (arguments.Length == 1)
            {
                wasLoaded = LoadConfigFile(arguments[0]);
                if (!wasLoaded)
                {
                    WriteLineWarning($"***MISSING OR INVALID SETTINGS FILE ({arguments[0]})***");
                    WriteLineWarning(USAGE_STRING);
                    return -1;
                }

            }
            else if (arguments.Length == 2)
            {
                if (!int.TryParse(arguments[1], out destinationPort))
                {
                    WriteLineWarning("***INVALID PORT***");
                    WriteLineWarning(USAGE_STRING);
                    return -1;
                }
            }
            else
            {
                WriteLineWarning("***MISSING PARAMETERS***");
                WriteLineWarning(USAGE_STRING);
                return -1;
            }

            if (SETTINGS_FILE == null && arguments.Length < 2)
            {
                WriteLineWarning("***MISSING PARAMETERS***");
                WriteLineWarning(USAGE_STRING);
                return -1;
            }

            if (SETTINGS_FILE == null)
            {
                return TestConnectivity(arguments[0], destinationPort);
            }
            else
            {
                var lastError = 0;
                foreach (var connetivityTestDestination in SETTINGS_FILE.Destinations)
                {
                    foreach (var port in connetivityTestDestination.Ports)
                    {
                        var result = TestConnectivity(connetivityTestDestination.IPAddress, port);
                        lastError = result != 0 ? result : lastError;
                    }
                }

                return lastError;
            }
        }

        private static string[] ProcessFlags(string[] args)
        {

            List<string> result = new List<string>();

            foreach (var arg in args)
            {
                switch (arg.ToLower(CultureInfo.InvariantCulture))
                {
                    case "-silent":
                        SILENTMODE_ENABLED = true;
                        break;
                    case "-showDebug":
                        DEBUGMODE_ENABLED = true;
                        break;
                    case "-hideErrors":
                        SHOW_ERRORS = true;
                        break;
                    case "-hideWarnings":
                        SHOW_WARNINGS = true;
                        break;
                    case "-hideInfo":
                        SHOW_INFO = false;
                        break;
                    default:
                        result.Add(arg);
                        break;
                }
            }

            return result.ToArray();
        }

        private static bool LoadConfigFile(string filepath)
        {
            if (!File.Exists(filepath))
                return false;

            try
            {
                using StreamReader file = File.OpenText(filepath);
                SETTINGS_FILE = Newtonsoft.Json.JsonConvert.DeserializeObject<ConnectivityTestSettings>(file.ReadToEnd());
            }
            catch (Exception ex)
            {
                WriteLineDebug("***FILE LOAD ERROR***");
                WriteLineDebug($"Exception:{ex.GetType().Name}");
                WriteLineDebug($"Message:{ex.Message}");
                WriteLineDebug($"Stack:{ex.StackTrace}");
                return false;
            }


            return true;
        }

        static int TestConnectivity(string ipAddress, int port)
        {
            if (IPAddress.TryParse(ipAddress, out IPAddress IP))
            {
                Socket s = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

                try
                {
                    WriteLineInfo($"Attempting to connect to {ipAddress}, on port {port}");

                    s.Connect(IP, port);

                    WriteLineSuccess($"Connection Sucessfull - {ipAddress}:{port}");

                }
                catch (SocketException socketExeption)
                {
                    WriteLineFailure($"Failed to Connect ({socketExeption.SocketErrorCode}) - {ipAddress}:{port}");
                    return -4;
                }
                catch (Exception ex)
                {

                    WriteLineError("***ERROR***");
                    WriteLineError($"Exception:{ex.GetType().Name}");
                    WriteLineError($"Message:{ex.Message}");
                    WriteLineError($"Stack:{ex.StackTrace}");
                    return -5;
                }
            }
            else
            {
                WriteLineError("***INVALID IP Address***");
                WriteLineError(USAGE_STRING);
                return -3;
            }

            return 0;
        }

        static void WriteLineDebug(string message)
        {
            if (SILENTMODE_ENABLED || !DEBUGMODE_ENABLED)
                return;

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine(message);
            Console.ResetColor();

        }
        static void WriteLineInfo(string message)
        {
            if (SILENTMODE_ENABLED || !SHOW_INFO)
                return;

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(message);
            Console.ResetColor();

        }
        static void WriteLineWarning(string message)
        {
            if (SILENTMODE_ENABLED || !SHOW_WARNINGS)
                return;

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine(message);
            Console.ResetColor();

        }
        static void WriteLineError(string message)
        {
            if (SILENTMODE_ENABLED || !SHOW_ERRORS)
                return;

            Console.ResetColor();
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(message);
            Console.ResetColor();

        }

        static void WriteLineFailure(string message)
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkRed;

            Console.WriteLine(message);
            Console.ResetColor();

        }

        static void WriteLineSuccess(string message)
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine(message);
            Console.ResetColor();

        }
    }
}
