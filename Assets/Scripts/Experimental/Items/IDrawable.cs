using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Experimental.Items
{
    public interface IDrawable
    {
        void Draw(WallInfo plane, params Vector3[] positions);
    }
}
