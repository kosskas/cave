using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class Projection
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
    /// Czy przeszedł przez inny obiekt
    /// </summary>
    public bool is_hit;

    /// <summary>
    /// Konstruktor klasy RayInfo
    /// </summary>
    /// <param name="markerObject">Znacznik na planszy</param>
    /// <param name="labelObject">Tekst</param>
    /// <param name="lineRendererObject">Obiekt LineRenderer do rysowania linii</param>
    /// <param name="initialIsHit">Początkowa wartość is_hit</param>
    public Projection(GameObject markerObject, GameObject labelObject, LineRenderer lineRendererObject, bool initialIsHit = false)
    {
        marker = markerObject;
        label = labelObject;
        lineRenderer = lineRendererObject;
        is_hit = initialIsHit;
    }
}