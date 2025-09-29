using System;
using UnityEngine;

namespace Assets.Scripts.Experimental
{
    public interface IRaycastable
    {
        void OnHoverEnter();

        void OnHoverAction(Action<GameObject> action);

        void OnHoverExit();
    }
}
