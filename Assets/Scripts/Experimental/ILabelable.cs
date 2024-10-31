using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Experimental
{
    public interface ILabelable
    {
        void NextLabel();

        void PrevLabel();
    }
}
