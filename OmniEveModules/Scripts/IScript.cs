using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniEveModules.Scripts
{
    using OmniEveModules.Actions;

    public delegate void DoActionsEventHandler(List<IAction> actions);
    public delegate void ScriptCompleteEventHandler();

    public abstract class IScript
    {
        public event DoActionsEventHandler DoActions;
        public event ScriptCompleteEventHandler ScriptCompleted;

        public abstract void DoWork(params object[] arguments);

        public void RunScriptAsync()
        {
            _thread = new Thread(delegate()
            {
                DoWork();

                DoActions(new List<IAction>() { new ScriptComplete(ScriptCompleted) });
            });

            _thread.Start();
        }

        public void RunScriptAsync(params object[] arguments)
        {
            _thread = new Thread(delegate ()
            {
                DoWork(arguments);

                DoActions(new List<IAction>() { new ScriptComplete(ScriptCompleted) });
            });

            _thread.Start();
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
