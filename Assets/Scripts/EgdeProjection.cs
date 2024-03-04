using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Struktura zawierające informacje o rzucie krawędzi
/// </summary>
public class EdgeProjection
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
    public GameObject start;
    /// <summary>
    /// Koniec krawędzi
    /// </summary>
    public GameObject end;

    /// <summary>
    /// Konstruktor klasy EdgeProjection
    /// </summary>
    /// <param name="lineRendererObject">Obiekt LineRenderer do rysowania linii</param>
    public EdgeProjection(int nOfProj, LineRenderer lineRendererObject, GameObject start, GameObject end)
    {
        this.nOfProj = nOfProj;
        this.lineRenderer = lineRendererObject;
        this.start = start;
        this.end = end;
    }
}