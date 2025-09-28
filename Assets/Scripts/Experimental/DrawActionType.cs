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
