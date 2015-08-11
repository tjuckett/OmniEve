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

        public abstract bool IsDone();
        public abstract void OnFrame();

        public void OnFrame(object sender, EventArgs e)
        {
            OnFrame();

            if (IsDone() == true)
                Stop();
        }

        public void Start()
        {
            Cache.Instance.DirectEve.OnFrame += OnFrame;
        }

        private void Stop()
        {
            DoActions(new List<IAction>() { new ScriptComplete(ScriptCompleted) });
        }

        protected void RunAction(IAction action)
        {
            List<IAction> actions = new List<IAction>() { action };

            DoActions(actions);
        }

        protected void RunActions(List<IAction> actions)
        {
            DoActions(actions);
        }
    }
}
