using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Experimental
{
    public interface IAnalyzable
    {
        string GetName();
        List<Vector3> FindCrossingPoints(IAnalyzable obj);
    }
}

