using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniEve
{
    using DirectEve;
    using OmniEveModules.Actions;
    using OmniEveModules.Caching;
    using OmniEveModules.Logging;
    using OmniEveModules.Lookup;
    using OmniEveModules.States;
    using OmniEveModules.Status;
    using OmniEveModules.Scripts;

    public class OmniEve : IDisposable
    {
        private DateTime _lastPulse;
        private OmniEveState _state = OmniEveState.Idle;
        private IAction _currentAction = null;
        private static DateTime _nextOmniEveAction = DateTime.UtcNow.AddHours(-1);

        private volatile List<IAction> _actions = new List<IAction>();

        public bool Cleanup { get; set; }
        public OmniEveState State { get { return _state; } }

        public OmniEve()
        {
            _lastPulse = DateTime.UtcNow;
            _state = OmniEveState.Idle;

            if (Cache.Instance.DirectEve == null)
            {
                Logging.Log("OmniEve:OmniEve", "Error on Loading DirectEve, maybe server is down", Logging.Orange);
                return;
            }

            try
            {
                //
                // setup the [ Cache.Instance.DirectEve.OnFrame ] Event triggered on every new frame to call EVEOnFrame()
                //
                Cache.Instance.DirectEve.OnFrame += EVEOnFrame;
            }
            catch (Exception ex)
            {
                Logging.Log("OmniEve:OmniEve", string.Format("DirectEVE.OnFrame: Exception {0}...", ex), Logging.White);
            }
        }

        public void RunScript(IScript script, params object[] arguments)
        {
            script.DoActions += RunScriptDoActions;
            script.RunScriptAsync(arguments);
        }

        private void RunScriptDoActions(List<IAction> actions)
        {
            foreach (IAction action in actions)
            {
                AddAction(action);
            }
        }

        public void AddAction(IAction action)
        {
            lock (_actions)
            {
                _actions.Add(action);
            }
        }        

        public bool IsActionQueueEmpty()
        {
            lock (_actions)
            {
                return _actions.Count <= 0 && (_currentAction == null || _currentAction.IsDone());
            }
        }

        public bool OnFrameValidate()
        {
            Time.Instance.LastFrame = DateTime.UtcNow;

            if (Cache.Instance.DirectEve.Login.AtLogin)
            {
                //if we somehow manage to get the questor GUI running on the login screen, do nothing.
                return false;
            }

            if (DateTime.UtcNow < Time.Instance.OmniEveStarted_DateTime.AddSeconds(Cache.Instance.RandomNumber(1, 4)))
            {
                return false;
            }

            // Only pulse after 20 seconds of entering a station or space
            /*if (Status.Instance.InSpace && DateTime.UtcNow > Time.Instance.LastInStation.AddSeconds(20) || (Status.Instance.InStation && DateTime.UtcNow > Time.Instance.LastInSpace.AddSeconds(20)))
            {
                return false;
            }*/

            // Only pulse state changes every 1.5s
            if (Status.Instance.InSpace && DateTime.UtcNow.Subtract(_lastPulse).TotalMilliseconds < Time.Instance.OmniEvePulseInSpace_milliseconds) //default: 600ms
            {
                return false;
            }

            if (Status.Instance.InStation && DateTime.UtcNow.Subtract(_lastPulse).TotalMilliseconds < Time.Instance.OmniEvePulseInStation_milliseconds) //default: 400ms
            {
                return false;
            }

            _lastPulse = DateTime.UtcNow;

            /*if (DateTime.UtcNow < Time.Instance.OmniEveStarted_DateTime.AddSeconds(30))
            {
                Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
            }*/

            // Session is not ready yet, do not continue
            if (!Cache.Instance.DirectEve.Session.IsReady)
            {
                return false;
            }
            else 
            {
                Time.Instance.LastSessionIsReady = DateTime.UtcNow;
            }

            return true;
        }

        private void EVEOnFrame(object sender, EventArgs e)
        {
            try
            {
                if (!OnFrameValidate()) return;

                //Logging.Log("OmniEve", "OnFrame: this is OmniEve.cs [" + DateTime.UtcNow + "] by default the next InSpace pulse will be in [" + Time.Instance.OmniEvePulseInSpace_milliseconds + "]milliseconds", Logging.Teal);

                /*if (DateTime.UtcNow < _nextOmniEveAction)
                {
                    Time.Instance.LastKnownGoodConnectedTime = DateTime.UtcNow;
                    Logging.Log("OmniEve:EVEOnFrame", "Logging last known good connection " + Time.Instance.LastKnownGoodConnectedTime, Logging.Teal);
                }*/

                // When in warp there's nothing we can do, so ignore everything
                if (Status.Instance.InSpace && Status.Instance.InWarp)
                {
                    Logging.Log("OmniEve:EVEOnFrame", "We are in space and in warp, we can't do anything, skipping frame", Logging.Teal);
                    return;
                }

                switch (_state)
                {
                    case OmniEveState.Idle:
                        if (_actions.Count > 0)
                        {
                            Logging.Log("OmniEve:EVEOnFrame", "Action entered queue going to NextAction state", Logging.Teal);
                            _state = OmniEveState.NextAction;
                        }
                        break;
                    case OmniEveState.Cleanup:
                        // Remove the eve frame and move to the close frame
                        Cache.Instance.DirectEve.Dispose();

                        _state = OmniEveState.CloseOmniEve;
                        Cache.Instance.DirectEve.OnFrame -= EVEOnFrame;
                        break;
                    case OmniEveState.NextAction:
                        
                        if (_actions.Count > 0)
                            _currentAction = _actions.FirstOrDefault();

                        if (_currentAction != null)
                        {
                            Logging.Log("OmniEve:EVEOnFrame", "Popping next action off the queue", Logging.Teal);

                            lock (_actions)
                            {
                                _actions.Remove(_currentAction);
                            }
                            
                            _state = OmniEveState.InitAction;
                        }
                        else
                        {
                            _state = OmniEveState.Idle;
                        }

                        break;
                    case OmniEveState.InitAction:
                        if (_currentAction != null)
                        {
                            Logging.Log("OmniEve:EVEOnFrame", "Initializing current action", Logging.Teal);
                            _currentAction.Initialize();
                            _state = OmniEveState.ProcessAction;
                        }
                        break;
                    case OmniEveState.ProcessAction:
                        if (_currentAction != null)
                        {
                            _currentAction.Process();
                        
                            // If the current action is now done we can stop processing and go back to the idle state
                            if (_currentAction.IsDone())
                            {
                                Logging.Log("OmniEve:EVEOnFrame", "Current action is done, going back to idle state", Logging.Teal);
                                _state = OmniEveState.Idle;
                                _currentAction = null;
                            }
                        }
                        break;
                    case OmniEveState.CloseOmniEve:
                        break;
                    case OmniEveState.Error:
                        break;
                }
            }
            catch(Exception ex)
            {
                Logging.Log("OmniEve:EVEOnFrame", "Exception [" + ex + "]", Logging.Debug);
            }
        }

        #region IDisposable implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
 
        private bool m_Disposed = false;
 
        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    //
                    // Close any open files here...
                    //

                }
 
                // Unmanaged resources are released here.
 
                m_Disposed = true;
            }
        }

        ~OmniEve()    
        {        
            Dispose(false);
        }
 
        #endregion
    }
}
