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
    /// Identyfikator wierzchołka
    /// </summary>
    public string vertexid;

    /// <summary>
    /// Konstruktor klasy VertexProjection
    /// </summary>
    /// <param name="vertex">Punkt na płaszczyźnie</param>
    /// <param name="line">Lina rzutująca</param>
    /// <param name="vertexid">Nazwa wierzchołka</param>
    public VertexProjection(Point vertex, LineSegment line, string vertexid)
    {  
        this.vertex= vertex;
        this.line = line;
        this.vertexid = vertexid;
    }
    /// <summary>
	/// Tworzy rzut punktu na płaszczyznę
	/// </summary>
	/// <param name="VertexProjectionsDir">Katalog organizujący rzuty wierzchołków</param>
	/// <param name="vertexid">ID rzutowanego wierzchołka</param>
	/// <param name="nOfProj">Numer rzutni</param>
	/// <returns>Rzut punktu na daną płaszczyznę</returns>
	public static VertexProjection CreateVertexProjection(GameObject VertexProjectionsDir, string vertexid, int nOfProj){
		GameObject obj = new GameObject(VertexProjectionsDir.name+" P("+nOfProj+") " + vertexid);
		obj.transform.SetParent(VertexProjectionsDir.transform);
		GameObject Point = new GameObject("Point P("+nOfProj+") " + vertexid);
		Point.transform.SetParent(obj.transform);
		GameObject Line = new GameObject("Line P("+nOfProj+") " + vertexid);
		Line.transform.SetParent(obj.transform);
		//znacznik
		Point vertexObject = Point.AddComponent<Point>();		
		///linia rzutująca
		LineSegment lineseg = Line.AddComponent<LineSegment>();
		return new VertexProjection(vertexObject, lineseg, vertexid);
	}
    /// <summary>
    /// Ustawia wyświetlania rzutu wierzchołka
    /// </summary>
    /// <param name="vertexname">Etykieta punktu</param>
    /// <param name="pointColor">Kolor punktu</param>
    /// <param name="pointSize">Rozmiar punktu</param>
    /// <param name="pointLabelColor">Kolor etykiet punktu</param>
    /// <param name="pointLabelSize">Rozmiar etykiet punktu</param>
    /// <param name="projectionLineColor">Kolor linii rzutującej</param>
    /// <param name="projectionLineWidth">Grubość linii rzutującej</param>
    /// <param name="projectionLabelColor">Kolor etykiety linii rzutującej</param>
    /// <param name="projectionLabelSize">Rozmiar etykiety linii rzutującej</param>
    public void SetDisplay(string vertexname, Color pointColor, float pointSize,
        Color pointLabelColor, float pointLabelSize,
        Color projectionLineColor, float projectionLineWidth,
        Color projectionLabelColor, float projectionLabelSize
         ){
        vertex.SetStyle(pointColor, pointSize);
		vertex.SetLabel(vertexname, pointLabelSize, pointLabelColor);
        line.SetStyle(projectionLineColor, projectionLineWidth);
		line.SetLabel("", projectionLabelSize, projectionLabelColor);
    }

}