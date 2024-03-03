using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Struktura zawierające informacje o rzucie krawędzi
/// </summary>
public class EdgesProjectionInfo
{
   
    /// <summary>
    /// Tekst
    /// </summary>
    public int nOfProj;
    
    /// <summary>
    /// Linia
    /// </summary>
    public LineRenderer lineRenderer;

    /// <summary>
    /// Początek krawędzi
    /// </summary>
    public ProjectionInfo start;
    /// <summary>
    /// Koniec krawędzi
    /// </summary>
    public ProjectionInfo end;

    /// <summary>
    /// Konstruktor klasy EdgesProjectionInfo
    /// </summary>
    /// <param name="lineRendererObject">Obiekt LineRenderer do rysowania linii</param>
    public EdgesProjectionInfo(int nOfProj, LineRenderer lineRendererObject, ProjectionInfo start, ProjectionInfo end)
    {
        this.nOfProj = nOfProj;
        this.lineRenderer = lineRendererObject;
        this.start = start;
        this.end = end;
    }
}