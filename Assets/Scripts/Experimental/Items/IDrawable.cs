using UnityEngine;

namespace Assets.Scripts.Experimental.Items
{
    public interface IDrawable
    {
        WallInfo Plane { get; }

        void Draw(WallInfo plane, params Vector3[] positions);
    }
}
