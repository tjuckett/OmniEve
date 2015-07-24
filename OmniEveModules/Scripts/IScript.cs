using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniEveModules.Scripts
{
    public interface IScript
    {
        void Initialize();
        void Process();
        bool IsDone();
    }
}
