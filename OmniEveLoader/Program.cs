using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EasyHook;
using mscoree;

namespace OmniEveLoader
{
    public class Main : IEntryPoint
    {
        public static string _appDomainNameToUse;
        public static string _pathToOmniEveEXE;
        public static string _OmniEveDLLSettingsINI;
        public static EXEBootStrapper _exeBootStrapper;
        public static bool _RestartOmniEveIfClosed;
        public static bool _DebugAppDomains;
        public static DateTime _OmniEveLoaderStarted;
        public static DateTime _lastAppDomainWasClosed;

        public Main(RemoteHooking.IContext InContext, string omniEveLoaderParameters, string logFileDirectory, string logFileName)
        {
        }

        public void Run(RemoteHooking.IContext InContext, string omniEveLoaderParameters, string logFileDirectory, string logFileName)
        {
            try
            {
                _OmniEveLoaderStarted = DateTime.UtcNow;

                if(string.IsNullOrEmpty(logFileDirectory) == false)
                    Logging.ConsoleLogPath = logFileDirectory;
                if (string.IsNullOrEmpty(logFileName) == false)
                    Logging.ConsoleLogFile = logFileName;

                if(string.IsNullOrEmpty(logFileDirectory) == false && string.IsNullOrEmpty(logFileName) == false)
                    Logging.SaveConsoleLog = true;

                Logging.Log("OmniEveLoader", "OmniEveLoader has started", Logging.White);

                int i = 0;
                Logging.Log("OmniEveLoader", "OmniEveLoader Parameters we were passed [" + i + "] - [" + omniEveLoaderParameters + "]", Logging.White);

                while (true)
                {
                    if (PrepareToLoadPreLoginSettingsFromINI(omniEveLoaderParameters))
                    {
                        if (DateTime.UtcNow < _OmniEveLoaderStarted.AddSeconds(5) || _RestartOmniEveIfClosed)
                        {
                            Logging.Log("OmniEveLoader", "Starting OmniEve", Logging.White);
                            EXEBootstrapper_StartOmniEve();
                        }

                        while (EXEBootStrapper.EnumAppDomains().Any(e => e.FriendlyName == Main._appDomainNameToUse))
                        {
                            try
                            {
                                System.Threading.Thread.Sleep(30000);
                                if (_DebugAppDomains)
                                {
                                    IEnumerable<AppDomain> CurrentlyExistingAppdomains = EXEBootStrapper.EnumAppDomains().ToList();
                                    if (CurrentlyExistingAppdomains != null && CurrentlyExistingAppdomains.Any())
                                    {
                                        int intAppdomain = 0;
                                        foreach (AppDomain _appdomain in EXEBootStrapper.EnumAppDomains())
                                        {
                                            intAppdomain++;
                                            Logging.Log("OmniEveLoader", "[" + intAppdomain + "] AppDomain [" + _appdomain.FriendlyName + "]", Logging.White);
                                        }
                                    }
                                    else
                                    {
                                        Logging.Log("OmniEveLoader", "No AppDomains found.", Logging.White);
                                    }    
                                }
                            }
                            catch (Exception ex)
                            {
                                Logging.Log("OmniEveLoader", "exception [" + ex + "]", Logging.White);
                            }
                        }

                        Logging.Log("OmniEveLoader", "The AppDomain [" + Main._appDomainNameToUse + "] was closed. Note: _RestartOmniEveIfClosed is [" + _RestartOmniEveIfClosed + "]", Logging.White);
                        _lastAppDomainWasClosed = DateTime.UtcNow;

                        while (DateTime.UtcNow < _lastAppDomainWasClosed.AddSeconds(30)) //wait for 30 seconds
                        {
                            if (_RestartOmniEveIfClosed) Logging.Log("OmniEveLoader", "Waiting another [" + Math.Round(_lastAppDomainWasClosed.AddSeconds(30).Subtract(DateTime.UtcNow).TotalSeconds, 0) + "] sec before restarting OmniEve", Logging.White);
                            System.Threading.Thread.Sleep(2000);
                        }
                    }
                    else
                    {
                        Logging.Log("OmniEveLoader", "unable to load settings from ini, halting]", Logging.White);
                    }
                }

                //Logging.Log("OmniEveLoader", "OmniEveLoader: done.", Logging.White);
            }
            catch (Exception ex)
            {
                Logging.Log("OmniEveLoader", "exception [" + ex + "]", Logging.White);
            }
        }

        private static bool PrepareToLoadPreLoginSettingsFromINI(string arg)
        {
            //
            // Load PathToOmniEveEXE and AppDomainToCreateForOmniEve from an ini
            //
            if (arg.ToLower().EndsWith(".ini"))
            {
                _OmniEveDLLSettingsINI = System.IO.Path.Combine(Directory.GetCurrentDirectory(), arg);

                if (!string.IsNullOrEmpty(_OmniEveDLLSettingsINI) && File.Exists(_OmniEveDLLSettingsINI))
                {
                    Logging.Log("OmniEveLoader", "Found [" + _OmniEveDLLSettingsINI + "] loading OmniEve PreLogin Settings", Logging.White);
                    if (!PreLoginSettings(_OmniEveDLLSettingsINI))
                    {
                        Logging.Log("OmniEveLoader", "Failed to load PreLogin settings from [" + _OmniEveDLLSettingsINI + "]", Logging.Debug);
                        return false;
                    }

                    Logging.Log("OmniEveLoader", "_pathToOmniEveEXE is [" + _pathToOmniEveEXE + "]", Logging.Debug);
                    Logging.Log("OmniEveLoader", "_appDomainNameToUse is [" + _appDomainNameToUse + "]", Logging.Debug);
                    return true;
                }

                return false;
            }

            return false;
        }

        public static void EXEBootstrapper_StartOmniEve()
        {
            try
            {
                _exeBootStrapper = new EXEBootStrapper();
                EXEBootStrapper.StartOmniEve();
            }
            catch (Exception ex)
            {
                Logging.Log("OmniEveLoader", "exception [" + ex + "]", Logging.White);
            }
            finally
            {
                //while (true)
                //{
                //    Thread.Sleep(50);
                //}
                Logging.Log("OmniEveLoader", "done", Logging.White);
            }
        }

        public static bool PreLoginSettings(string iniFile)
        {
            try
            {
                if (!File.Exists(iniFile))
                {
                    Logging.Log("PreLoginSettings", "Could not find a file named [" + iniFile + "]", Logging.Debug);
                    return false;
                }

                //foreach (string line in File.ReadAllLines(iniFile))
                //{
                //    Logging.Log("PreLoginSettings", "Contents of INI [" + line + "]", Logging.Debug);
                //}

                int index = 0;
                foreach (string line in File.ReadAllLines(iniFile))
                {
                    index++;
                    if (line.StartsWith(";"))
                        continue;

                    if (line.StartsWith("["))
                        continue;

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (string.IsNullOrEmpty(line))
                        continue;

                    //Logging.Log("PreLoginSettings", "Contents of INI Lines we Process [" + line + "]", Logging.Debug);

                    string[] sLine = line.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    //if (sLine.Count() != 2 && !sLine[0].Equals(ProxyUsername) && !sLine[0].Equals(ProxyPassword) )
                    if (sLine.Count() != 2)
                    {
                        Logging.Log("PreLoginSettings", "IniFile not right format at line: [" + index + "]", Logging.Debug);
                    }

                    //Logging.Log("PreLoginSettings", "Contents of INI Values we Processed [" + sLine[1] + "]", Logging.Debug);
                    switch (sLine[0].ToLower())
                    {
                        case "pathtoomnieveexe":
                            _pathToOmniEveEXE = sLine[1];
                            break;

                        case "appdomaintocreateforomnieve":
                            _appDomainNameToUse = sLine[1];
                            break;

                        case "restartomnieveifclosed":
                            _RestartOmniEveIfClosed = Boolean.Parse(sLine[1]);
                            break;

                        case "debugappdomains":
                            _DebugAppDomains = Boolean.Parse(sLine[1]);
                            break;
                    }
                }

                if (_pathToOmniEveEXE == null || string.IsNullOrEmpty(_pathToOmniEveEXE))
                {
                    Logging.Log("PreLoginSettings", "Missing: PathToOmniEveEXE in [" + iniFile + "]: We cannot launch Omni EVE if we do not know where it is. Ex. PathToOmniEveEXE=c:\\eveoffline\\DotNetPrograms\\OmniEve.exe", Logging.Debug);
                }

                if (_appDomainNameToUse == null || string.IsNullOrEmpty(_appDomainNameToUse))
                {
                    _appDomainNameToUse = "q1";
                }

                return true;
            }
            catch (Exception exception)
            {
                Logging.Log("OmniEveLoader", "Exception [" + exception + "]", Logging.Debug);
                return false;
            }
        }
    }

    public class CrossDomainTest : MarshalByRefObject
    {
        //  Call this method via a proxy.
        public void SomeMethod(string callingDomainName)
        {
            // Get this AppDomain's settings and display some of them.
            AppDomainSetup ads = AppDomain.CurrentDomain.SetupInformation;
            Console.WriteLine("AppName={0}, AppBase={1}, ConfigFile={2}",
                ads.ApplicationName,
                ads.ApplicationBase,
                ads.ConfigurationFile
            );

            // Display the name of the calling AppDomain and the name
            // of the second domain.
            // NOTE: The application's thread has transitioned between
            // AppDomains.
            Console.WriteLine("Calling from '{0}' to '{1}'.",
                callingDomainName,
                Thread.GetDomain().FriendlyName
            );
        }
    }

    public class EXEBootStrapper : MarshalByRefObject
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static EXEBootStrapper() //used during as the  OmniEve.exe entry point
        {

        }

        public static void StartOmniEve()
        {
            try
            {
                Logging.Log("OmniEveLoader", "------------------------------------------------------", Logging.Debug);
                Logging.Log("OmniEveLoader", "------------------------------------------------------", Logging.Debug);
                Logging.Log("OmniEveLoader", "Main._pathToOmniEveEXE [" + Main._pathToOmniEveEXE + "]", Logging.Debug);
                Logging.Log("OmniEveLoader", "------------------------------------------------------", Logging.Debug);
                Logging.Log("OmniEveLoader", "------------------------------------------------------", Logging.Debug);

                AppDomain OmniEvesAppDomain = null;

                if (EnumAppDomains().All(i => i.FriendlyName != Main._appDomainNameToUse))
                {
                    // Create a new AppDomain (what happens if this AppDomain already exists!?!)
                    OmniEvesAppDomain = System.AppDomain.CreateDomain(Main._appDomainNameToUse);
                    Logging.Log("EXEBootStrapper", "AppDomain [" + Main._appDomainNameToUse + "] created", Logging.White);
                }
                else
                {
                    OmniEvesAppDomain = EnumAppDomains().First(i => i.FriendlyName == Main._appDomainNameToUse);
                    Logging.Log("EXEBootStrapper", "AppDomain [" + Main._appDomainNameToUse + "] already exists, reusing.", Logging.White);
                }


                // Load the assembly and call the default entry point:
                try
                {
                    OmniEvesAppDomain.ExecuteAssembly(Main._pathToOmniEveEXE, new string[] { "-i="+Main._OmniEveDLLSettingsINI, "-d="+Logging.ConsoleLogPath, "-f="+Logging.ConsoleLogFile, "-s="+Logging.SaveConsoleLog });
                }
                catch (AppDomainUnloadedException)
                {
                    Logging.Log("EXEBootStrapper", "AppDomain [" + Main._appDomainNameToUse + "] unloaded", Logging.White);
                }

                Logging.Log("EXEBootStrapper", "ExecuteAssembly [" + Main._pathToOmniEveEXE + "] finished", Logging.White);
                //Main.UnthawEVEProcess = true;

                // Create an instance of MarshalbyRefType in the second AppDomain.
                // A proxy to the object is returned.
                CrossDomainTest mbrt =
                    (CrossDomainTest)OmniEvesAppDomain.CreateInstanceAndUnwrap(
                        Main._pathToOmniEveEXE,
                        typeof(CrossDomainTest).FullName
                    );

                // Call a method on the object via the proxy, passing the
                // default AppDomain's friendly name in as a parameter.
                mbrt.SomeMethod(Main._appDomainNameToUse);
            }
            catch (Exception ex)
            {
                Logging.Log("EXEBootStrapper", "exception [" + ex + "]", Logging.White);
            }
            finally
            {
                Logging.Log("EXEBootStrapper", "done.", Logging.White);
            }
        }

        //
        //https://stackoverflow.com/questions/388554/list-appdomains-in-process
        //
        //Remember to reference COM object \WINDOWS\Microsoft.NET\Framework\vXXX\mscoree.tlb, set reference mscoree "Embed Interop Types" as "False".

        public static IEnumerable<AppDomain> EnumAppDomains()
        {
            IList<AppDomain> appDomains = new List<AppDomain>();
            IntPtr enumHandle = IntPtr.Zero;
            ICorRuntimeHost host = null;

            try
            {
                host = new CorRuntimeHost();
                host.EnumDomains(out enumHandle);
                object domain = null;

                do
                {
                    host.NextDomain(enumHandle, out domain);
                    if (domain != null)
                    {
                        yield return (AppDomain)domain;
                    }
                }
                while (domain != null);
            }
            finally
            {
                if (host != null)
                {
                    if (enumHandle != IntPtr.Zero)
                    {
                        host.CloseEnum(enumHandle);
                    }

                    Marshal.ReleaseComObject(host);
                }
            }
        }
    }
}
