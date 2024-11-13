using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Experimental
{
    public delegate void DrawAction(
        IRaycastable hitObject,
        Vector3 hitPosition,
        WallInfo hitPlane,
        bool isEnd
    );
}
