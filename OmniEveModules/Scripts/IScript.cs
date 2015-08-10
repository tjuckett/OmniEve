using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniEveModules.Scripts
{
    using OmniEveModules.Caching;
    using OmniEveModules.Actions;

    public delegate void DoActionsEventHandler(List<IAction> actions);
    public delegate void ScriptCompleteEventHandler();

    public abstract class IScript
    {
        public event DoActionsEventHandler DoActions;
        public event ScriptCompleteEventHandler ScriptCompleted;

        public abstract void Update();
        public abstract bool IsDone();

        public void Start()
        {
            Cache.Instance.DirectEve.OnFrame += OnFrame;
        }

        public void Stop()
        {
            Cache.Instance.DirectEve.OnFrame -= OnFrame;
            
            DoActions(new List<IAction>() { new ScriptComplete(ScriptCompleted) });
        }

        public void OnFrame(object sender, EventArgs e)
        {
            if(IsDone() == false)
                Update();
            else
                Stop();
        }

        public void RunAction(IAction action)
        {
            List<IAction> actions = new List<IAction>() { action };

            DoActions(actions);
        }

        public void RunActions(List<IAction> actions)
        {
            DoActions(actions);
        }

        private List<IAction> _actions = new List<IAction>();
        private Thread _thread;
    }
}
