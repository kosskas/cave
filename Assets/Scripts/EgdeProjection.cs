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
    /// Numer rzutni
    /// </summary>
    public int nOfProj;
 
    /// <summary>
    /// Linia
    /// </summary>
    public LineRenderer lineRenderer;

    /// <summary>
    /// Początek krawędzi - punkt początkowy
    /// </summary>
    public VertexProjection start;
    /// <summary>
    /// Koniec krawędzi - punkt końcowy
    /// </summary>
    public VertexProjection end;

    /// <summary>
    /// Konstruktor EdgeProjection
    /// </summary>
    /// <param name="nOfProj">Numer rzutni</param>
    /// <param name="lineRendererObject">Obiekt LineRenderer do rysowania linii</param>
    /// <param name="start">punkt początkowy</param>
    /// <param name="end">punkt końcowy</param>
    public EdgeProjection(int nOfProj, LineRenderer lineRendererObject, VertexProjection start, VertexProjection end)
    {
        this.nOfProj = nOfProj;
        this.lineRenderer = lineRendererObject;
        this.start = start;
        this.end = end;
    }
}