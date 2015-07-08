using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniEveModules.Lookup
{
    public class Time
    {
        private static readonly Time _instance = new Time();
        public static Time Instance
        {
            get { return _instance; }
        }

        public int OmniEvePulseInSpace_milliseconds = 600;                  // Used to delay the next pulse, units: milliseconds. Default is 600
        public int OmniEvePulseInStation_milliseconds = 400;                // Used to delay the next pulse, units: milliseconds. Default is 600
        public int EVEAccountLoginDelayMinimum_seconds = 7;
        public int EVEAccountLoginDelayMaximum_seconds = 10;
        public int CharacterSelectionDelayMinimum_seconds = 2;
        public int CharacterSelectionDelayMaximum_seconds = 4;
        public int OmniEveBeforeLoginPulseDelay_milliseconds = 5000;        // Pulse Delay for Program.cs: Used to control the speed at which the program will retry logging in and retry checking the schedule

        //
        //
        //
        public DateTime LastFrame = DateTime.UtcNow;
        public DateTime LastInStation = DateTime.MinValue;
        public DateTime LastInSpace = DateTime.MinValue;
        public DateTime LastInWarp = DateTime.UtcNow;
        public DateTime LastKnownGoodConnectedTime { get; set; }
        public DateTime LastLoggingAction = DateTime.MinValue;
        public DateTime LastSessionChange = DateTime.UtcNow;
        public DateTime LastOpenHangar = DateTime.UtcNow;
        public DateTime LastSessionIsReady = DateTime.UtcNow;
        public DateTime LastLogMessage = DateTime.UtcNow;

        public DateTime OmniEveStarted_DateTime = DateTime.UtcNow;
        public static DateTime EnteredCloseOmniEve_DateTime { get; set; }
    }
}
