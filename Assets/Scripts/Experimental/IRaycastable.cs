using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
