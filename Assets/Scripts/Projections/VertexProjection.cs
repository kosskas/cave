using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Struktura zawierające informacje o rzucie wierzchołka
/// </summary>
public class VertexProjection
{
    /// <summary>
    /// Znacznik na płaszczyźnie
    /// </summary>
    public Point vertex;
    /// <summary>
    /// Linia rzutująca punkt
    /// </summary>
    public LineSegment line;

    /// <summary>
    /// Nazwa wierzchołka
    /// </summary>
    public string vertexName;

    /// <summary>
    /// Konstruktor klasy VertexProjection
    /// </summary>
    /// <param name="vertex">Punkt na płaszczyźnie</param>
    /// <param name="line">Lina rzutująca</param>
    /// <param name="vertexName">Nazwa wierzchołka</param>
    public VertexProjection(Point vertex, LineSegment line, string vertexName)
    {  
        this.vertex= vertex;
        this.line = line;
        this.vertexName = vertexName;
    }


}