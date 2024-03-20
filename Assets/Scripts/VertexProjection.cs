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
    /// Tablica określająca zakrycie wierzchołka według danej (1,2,3) rzutni
    /// </summary>
    //public bool[] collids;  

    /// <summary>
    /// Referencja na rzutowany punkt w 3D
    /// </summary>
    public Vector3 vertex3D;
    /// <summary>
    /// Znacznik na płaszczyźnie
    /// </summary>
    public Point vertex;
    /// <summary>
    /// Linia rzutująca punkt
    /// </summary>
    public LineSegment line;

    /// <summary>
    /// Nazwa
    /// </summary>
    public String vertexName;

    /// <summary>
    /// Konstruktor klasy VertexProjection
    /// </summary>
    /// <param name="markerObject">Znacznik na planszy</param>
    /// <param name="labelObject">Tekst</param>
    /// <param name="lineRendererObject">Obiekt LineRenderer do rysowania linii</param>
    /// <param name="vertexName">Nazwa</param>
    public VertexProjection(ref Vector3 vertex3D, ref Point vertex, ref LineSegment line, String vertexName)
    {  
        //ObjectProjecter op = (ObjectProjecter)GameObject.FindObjectOfType(typeof(ObjectProjecter));
        //collids = new bool[op.GetNOfProjections()];
        this.vertex3D = vertex3D;
        this.vertex= vertex;
        this.line = line;
        this.vertexName = vertexName;
    }
}