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
    public bool[] collids;   
    
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
    /// Nazwa
    /// </summary>
    public String name;

    /// <summary>
    /// Konstruktor klasy VertexProjection
    /// </summary>
    /// <param name="markerObject">Znacznik na planszy</param>
    /// <param name="labelObject">Tekst</param>
    /// <param name="lineRendererObject">Obiekt LineRenderer do rysowania linii</param>
    /// <param name="vertexName">Nazwa</param>
    public VertexProjection(GameObject markerObject, GameObject labelObject, LineRenderer lineRendererObject, String vertexName)
    {  
        ObjectProjecter op = (ObjectProjecter)GameObject.FindObjectOfType(typeof(ObjectProjecter));
        collids = new bool[op.GetNOfProjections()];
        marker = markerObject;
        label = labelObject;
        lineRenderer = lineRendererObject;
        name = vertexName;
    }
}