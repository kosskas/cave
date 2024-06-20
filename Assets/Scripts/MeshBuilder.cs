using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder : MonoBehaviour {

    /// <summary>
    /// Opisuje zbiór rzutowanych wierzchołków na danej płaszczyźnie
    /// </summary>
    Dictionary<WallInfo, Dictionary<string, GameObject>> verticesOnWalls;
    Dictionary<string, List<string>> edges;

	// Use this for initialization
	void Start () {
        verticesOnWalls = new Dictionary<WallInfo, Dictionary<string, GameObject>>();
		edges = new Dictionary<string, List<string>>();

    }
	
	// Update is called once per frame
	void Update () {
		/*
		 * Aktualizuj info jakie wierzch. na jakich ścianach
		 * Aktualizuj info jakie krawędzie między wierzchołkami
		 * Gdy 2 o tej samej etykiecie, na innych ścianach, sprawdź czy można postawić w 3D
		 * Jak się określi krawędzie to zrób to samo w 3D
		 * Zrób grafowo-matematyczny dziki algorytm żeby wypełnić ściany
		 */
	}

	public void AddPointProjection(WallInfo wall, string label, GameObject pointobject)
	{
		verticesOnWalls[wall][label] = pointobject;
	}
}
