using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniEveModules.Actions
{
    public interface IAction
    {
        void Initialize();
        void Process();
        bool IsDone();
    }
}
