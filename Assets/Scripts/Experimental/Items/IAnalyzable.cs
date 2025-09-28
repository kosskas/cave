using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Experimental
{
    public interface IAnalyzable
    {
        List<Vector3> FindCrossingPoints(IAnalyzable obj);

        IAnalyzable GetElement();
    }
}

