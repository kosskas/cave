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
    /// Rysowana krawędź
    /// </summary>
    public LineSegment line;

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
    public EdgeProjection(int nOfProj, LineSegment line, VertexProjection start, VertexProjection end)
    {
        this.nOfProj = nOfProj;
        this.line = line;
        this.start = start;
        this.end = end;
    }
}