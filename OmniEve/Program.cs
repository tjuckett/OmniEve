using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mono.Options;

namespace OmniEve
{
    using OmniEveModules.Scripts;
    using OmniEveModules.Logging;
    using OmniEveModules.Caching;
    using OmniEveModules.Lookup;
    using DirectEve;

    static class Program
    {
        private static bool _humanInterventionRequired;
        private static bool _missingEasyHookWarningGiven = false;
        private static bool _loggedInAndReady = false;
        private static bool _showHelp;
        private static bool _loginOnly;
        private static DateTime _lastServerStatusCheckWasNotOK = DateTime.MinValue;
        private static DateTime _nextPulse;
        private static int _serverStatusCheck = 0;
        private static OmniEve _omniEve;

        public static List<string> OmniEveParamaters;
        public static DateTime StartTime = DateTime.MaxValue;
        public static DateTime StopTime = DateTime.MinValue;
        public static DateTime OmniEveProgramLaunched = DateTime.UtcNow;
        public static DateTime ReadyToLogin = DateTime.UtcNow;
        public static DateTime EVEAccountLoginStarted = DateTime.UtcNow;
        public static DateTime NextSlotActivate = DateTime.UtcNow;
        public static string SettingsINI;

        private static bool? _readyToLoginEVEAccount;
        private static bool _readyToLoginToEVEAccount
        {
            get
            {
                try
                {
                    return _readyToLoginEVEAccount ?? false;
                }
                catch (Exception ex)
                {
                    Logging.Log("Program:ReadyToLoginToEVE", "Exception [" + ex + "]", Logging.Debug);
                    return false;
                }
            }

            set
            {
                _readyToLoginEVEAccount = value;
                if (value) //if true
                {
                    ReadyToLogin = DateTime.UtcNow;
                }
            }
        }
        

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ParseArgs(args);

            // This has to come after parsing the args, otherwise we won't get the correct ini file
            LoadPreLoginSettingsFromINI();

            if (!string.IsNullOrEmpty(Logging.EVELoginUserName) && !string.IsNullOrEmpty(Logging.EVELoginPassword) && !string.IsNullOrEmpty(Logging.MyCharacterName))
            {
                _readyToLoginToEVEAccount = true;
            }

            #region Load DirectEVE

            //
            // Load DirectEVE
            //

            try
            {
                bool EasyHookExists = File.Exists(System.IO.Path.Combine(Logging.PathToCurrentDirectory, "EasyHook.dll"));
                if (!EasyHookExists && !_missingEasyHookWarningGiven)
                {
                    Logging.Log("Program:Main", "EasyHook DLL's are missing. Please copy them into the same directory as your questor.exe", Logging.Orange);
                    Logging.Log("Program:Main", "halting!", Logging.Orange);
                    _missingEasyHookWarningGiven = true;
                    return;
                }

                int TryLoadingDirectEVE = 0;
                while (Cache.Instance.DirectEve == null && TryLoadingDirectEVE < 30)
                {
                    //
                    // DE now has cloaking enabled using EasyHook, If EasyHook DLLs are missing DE should complain. We check for and complain about missing EasyHook stuff before we get this far.
                    // 
                    //
                    //Logging.Log("Program:Startup", "temporarily disabling the loading of DE for debugging purposes, halting", Logging.Debug);
                    //while (Cache.Instance.DirectEve == null)
                    //{
                    //    System.Threading.Thread.Sleep(50); //this pauses forever...
                    //}
                    try
                    {
                        Logging.Log("Program:Main", "Starting Instance of DirectEVE using StandaloneFramework", Logging.Debug);
                        Cache.Instance.DirectEve = new DirectEve(new StandaloneFramework());
                        TryLoadingDirectEVE++;
                        Logging.Log("Program:Main", "DirectEVE should now be active: see above for any messages from DirectEVE", Logging.Debug);
                    }
                    catch (Exception exception)
                    {
                        Logging.Log("Program:Main", "exception [" + exception + "]", Logging.Orange);
                        return;
                    }
                }
            }
            catch (Exception exception)
            {
                Logging.Log("Program:Main", "exception [" + exception + "]", Logging.Orange);
                return;
            }

            if (Cache.Instance.DirectEve == null)
            {
                try
                {
                    Logging.Log("Program:Main", "Error on Loading DirectEve, maybe server is down", Logging.Orange);
                    return;
                }
                catch (Exception exception)
                {
                    Logging.BasicLog("Program:Main", "Exception while logging exception, oh joy [" + exception + "]");
                    return;
                }
            }

            #endregion Load DirectEVE

            try
            {
                Cache.Instance.DirectEve.OnFrame += LoginOnFrame;
            }
            catch (Exception ex)
            {
                Logging.Log("Program:Main", string.Format("DirectEVE.OnFrame: Exception {0}...", ex), Logging.White);
            }

            // Sleep until we're LoggedInAndReady
            while (!_loggedInAndReady)
            {
                System.Threading.Thread.Sleep(50); //this runs while we wait to login
            }

            if (_loggedInAndReady)
            {
                try
                {
                    Cache.Instance.DirectEve.OnFrame -= LoginOnFrame;
                }
                catch (Exception ex)
                {
                    Logging.Log("Program:Main", "DirectEVE.Dispose: Exception [" + ex + "]", Logging.White);
                }


                // If the last parameter is false, then we only auto-login
                if (_loginOnly)
                {
                    Logging.Log("Program:Main", "LoginOnly: done and exiting", Logging.Teal);
                    return;
                }


                StartTime = DateTime.Now;

                //
                // We should only get this far if run if we are already logged in...
                // launch questor
                //
                try
                {
                    Logging.Log("Program:Main", "We are logged in.", Logging.Teal);
                    Logging.Log("Program:Main", "Launching OmniEve", Logging.Teal);
                    _omniEve = new OmniEve();

                    int intdelayOmniEveUI = 0;
                    while (intdelayOmniEveUI < 50) //2.5sec = 50ms x 50
                    {
                        intdelayOmniEveUI++;
                        System.Threading.Thread.Sleep(50);
                    }

                    Logging.Log("Program:Main", "Launching OmniEveUI", Logging.Teal);

                    Automation automation = new Automation();
                    _omniEve.RunScript(automation);

                    //Application.Run(new OmniEveUI(_omniEve));

                    while (_omniEve.State != OmniEveModules.States.OmniEveState.CloseOmniEve)
                    {
                        System.Threading.Thread.Sleep(50); //this runs while omniEve is running.
                    }


                    Logging.Log("Program:Main", "Exiting OmniEve", Logging.Teal);

                }
                catch (Exception ex)
                {
                    Logging.Log("Program:Main", "Exception [" + ex + "]", Logging.Teal);
                }
                finally
                {
                    //Cleanup.DirecteveDispose();
                    AppDomain.Unload(AppDomain.CurrentDomain);
                }
            }    
        }

        private static void LoginOnFrame(object sender, EventArgs e)
        {
            Time.Instance.LastFrame = DateTime.UtcNow;
            Time.Instance.LastSessionIsReady = DateTime.UtcNow;

            if (DateTime.UtcNow < _lastServerStatusCheckWasNotOK.AddSeconds(Cache.Instance.RandomNumber(10, 20)))
            {
                Logging.Log("Program:LoginOnFrame", "lastServerStatusCheckWasNotOK = [" + _lastServerStatusCheckWasNotOK.ToShortTimeString() + "] waiting 10 to 20 seconds.", Logging.White);
                return;
            }

            _lastServerStatusCheckWasNotOK = DateTime.UtcNow.AddDays(-1); //reset this so we never hit this twice in a row w/o another server status check not being OK.

            if (DateTime.UtcNow < _nextPulse)
            {
                //Logging.Log("if (DateTime.UtcNow.Subtract(_lastPulse).TotalSeconds < _pulsedelay) then return");
                return;
            }

            _nextPulse = DateTime.UtcNow.AddMilliseconds(Time.Instance.OmniEveBeforeLoginPulseDelay_milliseconds);

            if (DateTime.UtcNow < OmniEveProgramLaunched.AddSeconds(5))
            {
                //
                // do not login for the first 5 seconds, wait...
                //
                return;
            }

            if (_humanInterventionRequired)
            {
                Logging.Log("Startup", "OnFrame: HumanInterventionRequired is true (this will spam every second or so)", Logging.Orange);
                return;
            }

            // If the session is ready, then we are done :)
            if (Cache.Instance.DirectEve.Session.IsReady)
            {
                Logging.Log("Program:LoginOnFrame", "We have successfully logged in", Logging.White);
                Time.Instance.LastSessionIsReady = DateTime.UtcNow;
                _loggedInAndReady = true;
                return;
            }

            if (Cache.Instance.DirectEve.Windows.Count != 0)
            {
                foreach (DirectWindow window in Cache.Instance.DirectEve.Windows)
                {
                    if (string.IsNullOrEmpty(window.Html))
                        continue;
                    Logging.Log("Startup", "WindowTitles:" + window.Name + "::" + window.Html, Logging.White);

                    //
                    // Close these windows and continue
                    //
                    if (window.Name == "telecom")
                    {
                        Logging.Log("Startup", "Closing telecom message...", Logging.Yellow);
                        Logging.Log("Startup", "Content of telecom window (HTML): [" + (window.Html).Replace("\n", "").Replace("\r", "") + "]", Logging.Yellow);
                        window.Close();
                        continue;
                    }

                    // Modal windows must be closed
                    // But lets only close known modal windows
                    if (window.Name == "modal")
                    {
                        bool close = false;
                        bool restart = false;
                        bool needHumanIntervention = false;
                        bool sayYes = false;
                        bool sayOk = false;
                        bool quit = false;

                        //bool update = false;

                        if (!string.IsNullOrEmpty(window.Html))
                        {
                            //errors that are repeatable and unavoidable even after a restart of eve/questor
                            needHumanIntervention = window.Html.Contains("reason: Account subscription expired");

                            //update |= window.Html.Contains("The update has been downloaded");

                            // Server going down
                            //Logging.Log("[Startup] (1) close is: " + close);
                            close |= window.Html.ToLower().Contains("please make sure your characters are out of harms way");
                            close |= window.Html.ToLower().Contains("accepting connections");
                            close |= window.Html.ToLower().Contains("could not connect");
                            close |= window.Html.ToLower().Contains("the connection to the server was closed");
                            close |= window.Html.ToLower().Contains("server was closed");
                            close |= window.Html.ToLower().Contains("make sure your characters are out of harm");
                            close |= window.Html.ToLower().Contains("connection to server lost");
                            close |= window.Html.ToLower().Contains("the socket was closed");
                            close |= window.Html.ToLower().Contains("the specified proxy or server node");
                            close |= window.Html.ToLower().Contains("starting up");
                            close |= window.Html.ToLower().Contains("unable to connect to the selected server");
                            close |= window.Html.ToLower().Contains("could not connect to the specified address");
                            close |= window.Html.ToLower().Contains("connection timeout");
                            close |= window.Html.ToLower().Contains("the cluster is not currently accepting connections");
                            close |= window.Html.ToLower().Contains("your character is located within");
                            close |= window.Html.ToLower().Contains("the transport has not yet been connected");
                            close |= window.Html.ToLower().Contains("the user's connection has been usurped");
                            close |= window.Html.ToLower().Contains("the EVE cluster has reached its maximum user limit");
                            close |= window.Html.ToLower().Contains("the connection to the server was closed");
                            close |= window.Html.ToLower().Contains("client is already connecting to the server");

                            //close |= window.Html.Contains("A client update is available and will now be installed");
                            //
                            // eventually it would be nice to hit ok on this one and let it update
                            //
                            close |= window.Html.ToLower().Contains("client update is available and will now be installed");
                            close |= window.Html.ToLower().Contains("change your trial account to a paying account");

                            //
                            // these windows require a restart of eve all together
                            //
                            restart |= window.Html.ToLower().Contains("the connection was closed");
                            restart |= window.Html.ToLower().Contains("connection to server lost."); //INFORMATION
                            restart |= window.Html.ToLower().Contains("local cache is corrupt");
                            sayOk |= window.Html.ToLower().Contains("local session information is corrupt");
                            restart |= window.Html.ToLower().Contains("The client's local session"); // information is corrupt");
                            restart |= window.Html.ToLower().Contains("restart the client prior to logging in");

                            //
                            // these windows require a quit of eve all together
                            //
                            quit |= window.Html.ToLower().Contains("the socket was closed");

                            //
                            // Modal Dialogs the need "yes" pressed
                            //
                            //sayYes |= window.Html.Contains("There is a new build available. Would you like to download it now");
                            //sayOk |= window.Html.Contains("The update has been downloaded. The client will now close and the update process begin");
                            sayOk |= window.Html.Contains("The transport has not yet been connected, or authentication was not successful");

                            //Logging.Log("[Startup] (2) close is: " + close);
                            //Logging.Log("[Startup] (1) window.Html is: " + window.Html);
                        }

                        //if (update)
                        //{
                        //    int secRestart = (400 * 3) + Cache.Instance.RandomNumber(3, 18) * 100 + Cache.Instance.RandomNumber(1, 9) * 10;
                        //    LavishScript.ExecuteCommand("uplink exec Echo [${Time}] timedcommand " + secRestart + " OSExecute taskkill /IM launcher.exe");
                        //}

                        if (sayYes)
                        {
                            Logging.Log("Startup", "Found a window that needs 'yes' chosen...", Logging.White);
                            Logging.Log("Startup", "Content of modal window (HTML): [" + (window.Html).Replace("\n", "").Replace("\r", "") + "]", Logging.White);
                            window.AnswerModal("Yes");
                            continue;
                        }

                        if (sayOk)
                        {
                            Logging.Log("Startup", "Found a window that needs 'ok' chosen...", Logging.White);
                            Logging.Log("Startup", "Content of modal window (HTML): [" + (window.Html).Replace("\n", "").Replace("\r", "") + "]", Logging.White);
                            window.AnswerModal("OK");
                            if (window.Html.Contains("The update has been downloaded. The client will now close and the update process begin"))
                            {
                                //
                                // schedule the closing of launcher.exe via a timedcommand (10 min?) in the uplink...
                                //
                            }
                            continue;
                        }

                        if (quit)
                        {
                            Logging.Log("Startup", "Restarting eve...", Logging.Red);
                            Logging.Log("Startup", "Content of modal window (HTML): [" + (window.Html).Replace("\n", "").Replace("\r", "") + "]", Logging.Red);
                            window.AnswerModal("quit");

                            //_directEve.ExecuteCommand(DirectCmd.CmdQuitGame);
                        }

                        if (restart)
                        {
                            Logging.Log("Startup", "Restarting eve...", Logging.Red);
                            Logging.Log("Startup", "Content of modal window (HTML): [" + (window.Html).Replace("\n", "").Replace("\r", "") + "]", Logging.Red);
                            window.AnswerModal("restart");
                            continue;
                        }

                        if (close)
                        {
                            Logging.Log("Startup", "Closing modal window...", Logging.Yellow);
                            Logging.Log("Startup", "Content of modal window (HTML): [" + (window.Html).Replace("\n", "").Replace("\r", "") + "]", Logging.Yellow);
                            window.Close();
                            continue;
                        }

                        if (needHumanIntervention)
                        {
                            Logging.Log("Startup", "ERROR! - Human Intervention is required in this case: halting all login attempts - ERROR!", Logging.Red);
                            Logging.Log("Startup", "window.Name is: " + window.Name, Logging.Red);
                            Logging.Log("Startup", "window.Html is: " + window.Html, Logging.Red);
                            Logging.Log("Startup", "window.Caption is: " + window.Caption, Logging.Red);
                            Logging.Log("Startup", "window.Type is: " + window.Type, Logging.Red);
                            Logging.Log("Startup", "window.ID is: " + window.Id, Logging.Red);
                            Logging.Log("Startup", "window.IsDialog is: " + window.IsDialog, Logging.Red);
                            Logging.Log("Startup", "window.IsKillable is: " + window.IsKillable, Logging.Red);
                            Logging.Log("Startup", "window.Viewmode is: " + window.ViewMode, Logging.Red);
                            Logging.Log("Startup", "ERROR! - Human Intervention is required in this case: halting all login attempts - ERROR!", Logging.Red);
                            _humanInterventionRequired = true;
                            return;
                        }
                    }

                    if (string.IsNullOrEmpty(window.Html))
                        continue;

                    if (window.Name == "telecom")
                        continue;
                    Logging.Log("Startup", "We have an unexpected window, auto login halted.", Logging.Red);
                    Logging.Log("Startup", "window.Name is: " + window.Name, Logging.Red);
                    Logging.Log("Startup", "window.Html is: " + window.Html, Logging.Red);
                    Logging.Log("Startup", "window.Caption is: " + window.Caption, Logging.Red);
                    Logging.Log("Startup", "window.Type is: " + window.Type, Logging.Red);
                    Logging.Log("Startup", "window.ID is: " + window.Id, Logging.Red);
                    Logging.Log("Startup", "window.IsDialog is: " + window.IsDialog, Logging.Red);
                    Logging.Log("Startup", "window.IsKillable is: " + window.IsKillable, Logging.Red);
                    Logging.Log("Startup", "window.Viewmode is: " + window.ViewMode, Logging.Red);
                    Logging.Log("Startup", "We have got an unexpected window, auto login halted.", Logging.Red);
                    return;
                }
                return;
            }

            if (Cache.Instance.DirectEve.Login.AtLogin && Cache.Instance.DirectEve.Login.ServerStatus != "Status: OK")
            {
                if (_serverStatusCheck <= 20) // at 10 sec a piece this would be 200+ seconds
                {
                    Logging.Log("Startup", "Server status[" + Cache.Instance.DirectEve.Login.ServerStatus + "] != [OK] try later", Logging.Orange);
                    _serverStatusCheck++;
                    //retry the server status check twice (with 1 sec delay between each) before kicking in a larger delay
                    if (_serverStatusCheck > 2)
                    {
                        _lastServerStatusCheckWasNotOK = DateTime.UtcNow;
                    }

                    return;
                }

                _serverStatusCheck = 0;
                //Cleanup.ReasonToStopQuestor = "Server Status Check shows server still not ready after more than 3 min. Restarting Questor. ServerStatusCheck is [" + ServerStatusCheck + "]";
                //Logging.Log("Startup", Cleanup.ReasonToStopQuestor, Logging.Red);
                Time.EnteredCloseOmniEve_DateTime = DateTime.UtcNow;
                //Cleanup.CloseQuestor(Cleanup.ReasonToStopQuestor, true);
                return;
            }

            if (Cache.Instance.DirectEve.Login.AtLogin && !Cache.Instance.DirectEve.Login.IsLoading && !Cache.Instance.DirectEve.Login.IsConnecting)
            {
                //
                // we must have support instances available, after a delay, login
                //
                if (DateTime.UtcNow.Subtract(ReadyToLogin).TotalMilliseconds > Cache.Instance.RandomNumber(Time.Instance.EVEAccountLoginDelayMinimum_seconds * 1000, Time.Instance.EVEAccountLoginDelayMaximum_seconds * 1000))
                {
                    Logging.Log("Startup", "Login account [" + Logging.EVELoginUserName + "]", Logging.White);
                    Cache.Instance.DirectEve.Login.Login(Logging.EVELoginUserName, Logging.EVELoginPassword);
                    EVEAccountLoginStarted = DateTime.UtcNow;
                    Logging.Log("Startup", "Waiting for Character Selection Screen", Logging.White);
                    return;
                }
            }

            if (Cache.Instance.DirectEve.Login.AtCharacterSelection && Cache.Instance.DirectEve.Login.IsCharacterSelectionReady && !Cache.Instance.DirectEve.Login.IsConnecting && !Cache.Instance.DirectEve.Login.IsLoading)
            {
                if (DateTime.UtcNow.Subtract(EVEAccountLoginStarted).TotalMilliseconds > Cache.Instance.RandomNumber(Time.Instance.CharacterSelectionDelayMinimum_seconds * 1000, Time.Instance.CharacterSelectionDelayMaximum_seconds * 1000) && DateTime.UtcNow > NextSlotActivate)
                {
                    foreach (DirectLoginSlot slot in Cache.Instance.DirectEve.Login.CharacterSlots)
                    {
                        if (slot.CharId.ToString(CultureInfo.InvariantCulture) != Logging.MyCharacterName && System.String.Compare(slot.CharName, Logging.MyCharacterName, System.StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            continue;
                        }

                        Logging.Log("Startup", "Activating character [" + slot.CharName + "]", Logging.White);
                        NextSlotActivate = DateTime.UtcNow.AddSeconds(5);
                        slot.Activate();
                        //EVECharacterSelected = DateTime.UtcNow;
                        return;
                    }
                    Logging.Log("Startup",
                        "Character id/name [" + Logging.MyCharacterName + "] not found, retrying in 10 seconds",
                        Logging.White);
                }
            }
        }

        private static void ParseArgs(IEnumerable<string> args)
        {
            if (!string.IsNullOrEmpty(Logging.EVELoginUserName) &&
                !string.IsNullOrEmpty(Logging.EVELoginPassword) &&
                !string.IsNullOrEmpty(Logging.MyCharacterName))
            {
                return;
            }

            OptionSet p = new OptionSet
            {
                "Usage: questor [OPTIONS]",
                "Buy and sell stuff on the auction house.",
                "",
                "Options:",
                {"u|user=", "the {USER} we are logging in as.", v => Logging.EVELoginUserName = v},
                {"p|password=", "the user's {PASSWORD}.", v => Logging.EVELoginPassword = v},
                {"c|character=", "the {CHARACTER} to use.", v => Logging.MyCharacterName = v},
                {"l|loginOnly", "login only and exit.", v => _loginOnly = v != null},
                {"h|help", "show this message and exit", v => _showHelp = v != null},
                {"f|logfile=", "log file name", v => Logging.ConsoleLogFile = v},
                {"d|logdirectory=", "log file directory", v => Logging.ConsoleLogPath = v},
                {"s|savelogfile=", "save the log file", v => Logging.SaveConsoleLog = v != null},
                {"i|settingsini=", "the settings ini file", v => SettingsINI = v}
            };

            try
            {
                OmniEveParamaters = p.Parse(args);
                //Logging.Log(string.Format("questor: extra = {0}", string.Join(" ", extra.ToArray())));
            }
            catch (OptionException ex)
            {
                Logging.Log("Program:ParseArgs", "omnieve: ", Logging.White);
                Logging.Log("Program:ParseArgs", ex.Message, Logging.White);
                Logging.Log("Program:ParseArgs", "Try `omnieve --help' for more information.", Logging.White);
                return;
            }

            if (_showHelp)
            {
                System.IO.StringWriter sw = new System.IO.StringWriter();
                p.WriteOptionDescriptions(sw);
                Logging.Log("Program:ParseArgs", sw.ToString(), Logging.White);
                return;
            }
        }

        private static void LoadPreLoginSettingsFromINI()
        {
            if (!string.IsNullOrEmpty(Logging.EVELoginUserName) &&
                !string.IsNullOrEmpty(Logging.EVELoginPassword) &&
                !string.IsNullOrEmpty(Logging.MyCharacterName))
            {
                return;
            }

            //
            // (optionally) Load EVELoginUserName, EVELoginPassword, MyCharacterName (and other settings) from an ini
            //
            if (!string.IsNullOrEmpty(SettingsINI))
               SettingsINI = System.IO.Path.Combine(Directory.GetCurrentDirectory(), SettingsINI);
 
            if(File.Exists(SettingsINI))
            {
                if (!PreLoginSettings(SettingsINI)) Logging.Log("Program:LoadPreLoginSettingsFromINI", "Failed to load PreLogin settings from [" + SettingsINI + "]", Logging.Debug);
            }
        }

        public static bool PreLoginSettings(string iniFile)
        {
            string functionName = "Program:PreLoginSettings";

            try
            {
                if (!File.Exists(iniFile))
                {
                    Logging.Log(functionName, "Could not find a file named [" + iniFile + "]", Logging.Debug);
                }

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

                    string[] sLine = line.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    //if (sLine.Count() != 2 && !sLine[0].Equals(ProxyUsername) && !sLine[0].Equals(ProxyPassword) )
                    if (sLine.Count() != 2)
                    {
                        Logging.Log(functionName, "IniFile not right format at line: [" + index + "]", Logging.Debug);
                    }

                    switch (sLine[0].ToLower())
                    {
                        case "eveloginusername":
                            Logging.EVELoginUserName = sLine[1];
                            break;

                        case "eveloginpassword":
                            Logging.EVELoginPassword = sLine[1];
                            break;

                        case "characternametologin":
                            Logging.MyCharacterName = sLine[1];
                            break;

                        case "omnieveloginonly":
                            _loginOnly = bool.Parse(sLine[1]);
                            break;

                        case "debugdisableautologin":
                            Logging.DebugDisableAutoLogin = bool.Parse(sLine[1]);
                            break;
                    }
                }

                if (Logging.EVELoginUserName == null)
                {
                    Logging.Log(functionName, "Missing: EVELoginUserName in [" + iniFile + "]: omnieve cant possibly AutoLogin without the EVE Login UserName", Logging.Debug);
                }

                if (Logging.EVELoginPassword == null)
                {
                    Logging.Log(functionName, "Missing: EVELoginPassword in [" + iniFile + "]: omnieve cant possibly AutoLogin without the EVE Login Password!", Logging.Debug);
                }

                if (Logging.MyCharacterName == null)
                {
                    Logging.Log(functionName, "Missing: CharacterNameToLogin in [" + iniFile + "]: omnieve cant possibly AutoLogin without the EVE CharacterName to choose", Logging.Debug);
                }

                return true;
            }
            catch (Exception exception)
            {
                Logging.Log(functionName, "Exception [" + exception + "]", Logging.Debug);
                return false;
            }
        }
    }
}
