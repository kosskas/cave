using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Zawiera informacje dot. wyświetlania rzutowanych wierzchołków i krawędzi
/// </summary>
public class ProjectionInfo
{
    /// <summary>
    /// Grubość linii krawędzi.
    /// </summary>
    public float edgeLineWidth = 0.05f;

    /// <summary>
    /// Kolor linii krawędzi.
    /// </summary>
    public Color edgeColor = Color.black;

    /// <summary>
    /// Długość linii rzutowania.
    /// </summary>
    public float projectionLineWidth = 0.01f;

    /// <summary>
    /// Określa czy linie rzutowania powinny być wyświetlane.
    /// </summary>
    public bool showProjectionLines = false;

    /// <summary>
    /// Konstruktor bezparametrowy.
    /// </summary>
    public ProjectionInfo(){}

    /// <summary>
    /// Konstruktor z parametrami.
    /// </summary>
    /// <param name="edgeLineWidth">Grubość linii krawędzi.</param>
    /// <param name="edgeColor">Kolor linii krawędzi.</param>
    /// <param name="projectionLineWidth">Długość linii rzutowania.</param>
    /// <param name="showProjectionLines">Określa czy linie rzutowania powinny być wyświetlane.</param>
    public ProjectionInfo(float edgeLineWidth, Color edgeColor, float projectionLineWidth, bool showProjectionLines)
    {
        this.edgeLineWidth = edgeLineWidth;
        this.edgeColor = edgeColor;
        this.projectionLineWidth = projectionLineWidth;
        this.showProjectionLines = showProjectionLines;
    }
}