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

    public bool[] collids = new bool[3];
    
    /// <summary>
    /// Linia
    /// </summary>
    public LineRenderer lineRenderer;

    /// <summary>
    /// Początek krawędzi
    /// </summary>
    public VertexProjection start;
    /// <summary>
    /// Koniec krawędzi
    /// </summary>
    public VertexProjection end;

    /// <summary>
    /// Konstruktor klasy EdgeProjection
    /// </summary>
    /// <param name="lineRendererObject">Obiekt LineRenderer do rysowania linii</param>
    public EdgeProjection(int nOfProj, LineRenderer lineRendererObject, VertexProjection start, VertexProjection end)
    {
        this.nOfProj = nOfProj;
        this.lineRenderer = lineRendererObject;
        this.start = start;
        this.end = end;
    }
}