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
    /* Styl rzutowanego punktu */

    /// <summary>
    /// Kolor punktów.
    /// </summary>
    public Color pointColor = Color.black;

    /// <summary>
    /// Kolor etykiet punktów.
    /// </summary>
    public Color pointLabelColor = Color.white;

    /// <summary>
    /// Rozmiar punktów.
    /// </summary>
    public float pointSize = 0.04f;

    /// <summary>
    /// Rozmiar etykiet punktów.
    /// </summary>
    public float pointLabelSize = 0.04f;


    /* Styl rzutowanej krawędzi */

    /// <summary>
    /// Kolor krawędzi.
    /// </summary>
    public Color edgeLineColor = Color.black;

    /// <summary>
    /// Kolor etykiet krawędzi.
    /// </summary>
    public Color edgeLabelColor = Color.white;

    /// <summary>
    /// Grubość krawędzi.
    /// </summary>
    public float edgeLineWidth = 0.01f;

    /// <summary>
    /// Rozmiar etykiet krawędzi.
    /// </summary>
    public float edgeLabelSize = 0.01f;

    /* Styl linii rzutującej */

    /// <summary>
    /// Kolor linii rzutującej.
    /// </summary>
    public Color projectionLineColor = Color.gray;

    /// <summary>
    /// Kolor etykiet linii rzutującej.
    /// </summary>
    public Color projectionLabelColor = Color.white;

    /// <summary>
    /// Grubość linii rzutującej.
    /// </summary>
    public float projectionLineWidth = 0.01f;

    /// <summary>
    /// Rozmiar etykiet linii rzutującej.
    /// </summary>
    public float projectionLabelSize = 0.01f;


    /*  Linie odnoszące  */

    /// <summary>
    /// Kolor linii odnoszącej
    /// </summary>
    public Color referenceLineColor = Color.gray;
    /// <summary>
    /// Kolor etykiet linii odnoszącej.
    /// </summary>
    public Color referenceLabelColor = Color.black;
    
    /// <summary>
    /// Grubość linii odnoszącej.<
    /// </summary>
    public float referenceLineWidth = 0f;
    
    /// <summary>
    /// Rozmiar etykiet linii odnoszącej.
    /// </summary>
    public float referenceLabelSize = 0f;

    /* Flagi do rzutowania */

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
    /// <param name="pointColor">Kolor punktów.</param>
    /// <param name="pointLabelColor">Kolor etykiet punktów.</param>
    /// <param name="pointSize">Rozmiar punktów.</param>
    /// <param name="pointLabelSize">Rozmiar etykiet punktów.</param>
    /// <param name="edgeLineColor">Kolor krawędzi.</param>
    /// <param name="edgeLabelColor">Kolor etykiet krawędzi.</param>
    /// <param name="edgeLineWidth">Grubość krawędzi.</param>
    /// <param name="edgeLabelSize">Rozmiar etykiet krawędzi.</param>
    /// <param name="projectionLineColor">Kolor linii rzutującej.</param>
    /// <param name="projectionLabelColor">Kolor etykiet linii rzutującej.</param>
    /// <param name="projectionLineWidth">Grubość linii rzutującej.</param>
    /// <param name="projectionLabelSize">Rozmiar etykiet linii rzutującej.</param>
    /// <param name="referenceLineColor">Kolor linii odnoszącej.</param>
    /// <param name="referenceLabelColor">Kolor etykiet linii odnoszącej.</param>
    /// <param name="referenceLineWidth">Grubość linii odnoszącej.</param>
    /// <param name="referenceLabelSize">Rozmiar etykiet linii odnoszącej.</param>
    /// <param name="showProjectionLines">Określa czy linie rzutowania powinny być wyświetlane.</param>
    public ProjectionInfo(Color pointColor, Color pointLabelColor, float pointSize, float pointLabelSize,
                          Color edgeLineColor, Color edgeLabelColor, float edgeLineWidth, float edgeLabelSize,
                          Color projectionLineColor, Color projectionLabelColor, float projectionLineWidth, float projectionLabelSize,
                          Color referenceLineColor, Color referenceLabelColor, float referenceLineWidth, float referenceLabelSize,
                          bool showProjectionLines)
    {
        this.pointColor = pointColor;
        this.pointLabelColor = pointLabelColor;
        this.pointSize = pointSize;
        this.pointLabelSize = pointLabelSize;
        this.edgeLineColor = edgeLineColor;
        this.edgeLabelColor = edgeLabelColor;
        this.edgeLineWidth = edgeLineWidth;
        this.edgeLabelSize = edgeLabelSize;
        this.projectionLineColor = projectionLineColor;
        this.projectionLabelColor = projectionLabelColor;
        this.projectionLineWidth = projectionLineWidth;
        this.projectionLabelSize = projectionLabelSize;
        this.referenceLineColor = referenceLineColor;
        this.referenceLabelColor = referenceLabelColor;
        this.referenceLineWidth = referenceLineWidth;
        this.referenceLabelSize = referenceLabelSize;
        this.showProjectionLines = showProjectionLines;
    }
}