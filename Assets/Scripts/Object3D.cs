using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;

/// <summary>
/// Klasa zarządzająca wyświetlanym obiektem
/// </summary>
public class Object3D : MonoBehaviour {

	private List<Vector3> vertices = new List<Vector3>();
	private List<int> triangles = new List<int>();
	private Dictionary<string, Vector3> labeledVertices = new Dictionary<string, Vector3>();
	//private List<List<string>> faces = new List<List<string>>();
	private MeshFilter meshFilter;

	/// <summary>
	/// Inicjalizuje obiekt, dodaje skrypty.
	/// </summary>
	/// <param name="vertices">Lista wierzchołków</param>
	/// <param name="triangles">Lista trójkątów</param>
	/// <param name="labeledVertices">Oznaczenia wierzchołków</param>
	public void InitObject(List<Vector3> vertices, List<int> triangles, Dictionary<string, Vector3> labeledVertices){
		this.vertices = vertices;
		this.triangles = triangles;
		this.labeledVertices = labeledVertices;
		CreateMesh();
		AddVertexLabels();
		AddCamera();
		//TODO
		//Dodanie oznaczeń i wyświetlanie krawędzi


		//TODO
		//Dodanie projekcji rzutów
	}
	/// <summary>
	/// Tworzy siatkę dla nowo powstałego obiektu
	/// </summary>
    private void CreateMesh() {
        // Create a new mesh
        Mesh mesh = new Mesh();
        
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();

        // Recalculate normals and bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Add a MeshFilter component to the GameObject
        meshFilter = gameObject.AddComponent<MeshFilter>();

        // Add a MeshRenderer component to the GameObject
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // Set the material for the MeshRenderer (you can create your own material or use an existing one)
        meshRenderer.material = new Material(Shader.Find("Standard"));

        // Assign the generated mesh to the MeshFilter
        meshFilter.mesh = mesh;

		//Dodanie collidera potrzebnego do obracania
		MeshCollider meshColl = gameObject.AddComponent<MeshCollider>();
    }
	/// <summary>
	/// Dodaje oznaczenia przy wyświetlaniu wierzchołków
	/// </summary>
    private void AddVertexLabels() {
        
		VertexLabels vl = gameObject.AddComponent<VertexLabels>();
		vl.InitLabels(meshFilter, labeledVertices);	
    }
	/// <summary>
	/// Dodaje kamerę potrzebną do obracania
	/// </summary>
	private void AddCamera(){
		
		GameObject camObject = GameObject.Find("CameraObject");
		CameraScript camScript = camObject.GetComponent<CameraScript>();
		GameObject staticCam = camScript.cam2;
		ObjectRotator rotator = gameObject.AddComponent<ObjectRotator>();
		rotator.cam = staticCam.GetComponent<Camera>();
	}

}
