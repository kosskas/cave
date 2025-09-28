using UnityEngine;
/// <summary>
/// Struktura zawierające informacje o rzucie krawędzi
/// </summary>
public class EdgeProjection
{
    /// <summary>
    /// Oznaczenie linii
    /// </summary>
    public string label;
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
    /// <param name="label">Oznaczenie linii</param>
    /// <param name="nOfProj">Numer rzutni</param>
    /// <param name="line">Obiekt LineRenderer do rysowania linii</param>
    /// <param name="start">punkt początkowy</param>
    /// <param name="end">punkt końcowy</param>
    public EdgeProjection(string label, int nOfProj, LineSegment line, VertexProjection start, VertexProjection end)
    {
        this.label = label;
        this.nOfProj = nOfProj;
        this.line = line;
        this.start = start;
        this.end = end;
    }
    /// <summary>
	/// Tworzy rzut krawędzi na płaszczyznę
	/// </summary>
	/// <param name="EdgeProjectionsDir">Katalog organizujący rzuty krawędzi</param>
	/// <param name="p1">Pierwszy zrzutowany punkt krawędzi</param>
	/// <param name="p2">Drugi zrzutowany punkt krawędzi</param>
	/// <param name="nOfProj">Numer rzutni</param>
	/// <param name="label">Etykieta krawędzi</param>
	/// <returns>Rzut krawędzi na daną płaszczyznę</returns>
    public static EdgeProjection CreateEgdeProjection(GameObject EdgeProjectionsDir, VertexProjection p1, VertexProjection p2, string label, int nOfProj){
		GameObject edge = new GameObject(EdgeProjectionsDir.name+" P("+nOfProj +") " + p1.vertexid+p2.vertexid);
		edge.transform.SetParent(EdgeProjectionsDir.transform);
		LineSegment drawEdge = edge.AddComponent<LineSegment>();	
		return new EdgeProjection(label, nOfProj, drawEdge, p1, p2);		
	}
    /// <summary>
    /// Ustawia wyświetlanie krawędzi
    /// </summary>
    /// <param name="edgeLineColor">Kolor krawędzi.</param>
    /// <param name="edgeLineWidth">Grubość krawędzi.</param>
    /// <param name="edgeLabelColor">Kolor etykiet krawędzi.</param>
    /// <param name="edgeLabelSize">Rozmiar etykiet krawędzi.</param>
    public void SetDisplay(Color edgeLineColor, float edgeLineWidth, Color edgeLabelColor, float edgeLabelSize){
        line.SetStyle(edgeLineColor, edgeLineWidth);
		line.SetLabel(label, edgeLabelSize, edgeLabelColor);
    }
}