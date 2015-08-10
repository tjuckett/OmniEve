using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniEveModules.Actions
{
    using OmniEveModules.Scripts;

    public class ScriptComplete : IAction
    {
        private bool _isDone = false;
        private event ScriptCompleteEventHandler _scriptComplete;

        public ScriptComplete(ScriptCompleteEventHandler scriptComplete)
        {
            _scriptComplete = scriptComplete;
    }

        public void Initialize() { }

        public void Process()
        {
            _isDone = true;

            if(_scriptComplete != null)
                _scriptComplete();
        }

        public bool IsDone()
        {
            return _isDone;
        }
    }
}
