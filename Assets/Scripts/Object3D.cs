using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;

/// <summary>
/// Centralna klasa sterująca wyświetlanym obiektem
/// </summary>
public class Object3D : MonoBehaviour {

	private List<Vector3> vertices = new List<Vector3>();
	private List<int> triangles = new List<int>();
	private Dictionary<string, Vector3> labeledVertices = new Dictionary<string, Vector3>();
	private List<List<string>> faces = new List<List<string>>();
	private MeshFilter meshFilter;

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
    private void AddVertexLabels() {
        //Dodanie oznaczeń i wyświetlanie wierzchołków
		VertexLabels vl = gameObject.AddComponent<VertexLabels>();
		vl.InitLabels(meshFilter, labeledVertices);	
    }
	private void AddCamera(){
		//Dodanie kamery potrzebnej do obracania
		GameObject camObject = GameObject.Find("CameraObject");
		CameraScript camScript = camObject.GetComponent<CameraScript>();
		GameObject staticCam = camScript.cam2;
		ObjectRotator rotator = gameObject.AddComponent<ObjectRotator>();
		rotator.cam = staticCam.GetComponent<Camera>();
	}

}
