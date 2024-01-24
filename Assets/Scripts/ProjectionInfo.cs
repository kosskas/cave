using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class ProjectionInfo
{
    /// <summary>
    /// Znacznik na planszy
    /// </summary>
    public GameObject marker;
    
    /// <summary>
    /// Tekst
    /// </summary>
    public GameObject label;
    
    /// <summary>
    /// Linia
    /// </summary>
    public LineRenderer lineRenderer;

    /// <summary>
    /// Konstruktor klasy RayInfo
    /// </summary>
    /// <param name="markerObject">Znacznik na planszy</param>
    /// <param name="labelObject">Tekst</param>
    /// <param name="lineRendererObject">Obiekt LineRenderer do rysowania linii</param>
    public ProjectionInfo(GameObject markerObject, GameObject labelObject, LineRenderer lineRendererObject)
    {
        marker = markerObject;
        label = labelObject;
        lineRenderer = lineRendererObject;
    }
}