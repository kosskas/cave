using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using UnityEngine;

/*
 * Obiekt tej klasy powinien rysować litery dopiero gdy SolidImporter załaduje kształt
 * solidImporter wywołuje InitLabels
 * VertexLabels vl = FindObjectOfType<VertexLabels>();
*/
/// <summary>
/// Klasa VertexLabels opisuje właściwości wsyświetlania informacji o wierzchołkach bryły
/// </summary>
public class VertexLabels : MonoBehaviour
{
    /// <summary>
    /// Przesunięcie nazwy od wierzchołka
    /// </summary>
    [SerializeField] public float labelOffset = 0.0f;
    /// <summary>
    /// Rodzaj fontu dla wyświetlanych nazw wierzchołków
    /// </summary>
    [SerializeField] public Font font;
    /// <summary>
    /// Domyślmy kolor dla wyświetlanych nazw wierzchołków
    /// </summary>
    [SerializeField] public Color color = Color.black;
    /// <summary>
    /// Rozmiar wyświetlanych nazw wierzchołków
    /// </summary>
    [SerializeField] public float characterSize = 0.1f;
    /// <summary>
    /// Tablica wierzchołków
    /// </summary>
    Vector3[] vertices = null;
    /// <summary>
    /// Tablica znaczników
    /// </summary>
    GameObject[] labels = null;
    /// <summary>
    /// Referencja na Obiekt3D
    /// </summary>
    Object3D OBJECT3D;
    /// <summary>
    /// Aktualizuje położenie oznaczeń wierzchołków
    /// </summary>
    void Update()
    {
        if (OBJECT3D!=null && labels != null && vertices != null)
        {
            //Quaternion rotation = transform.rotation;
            ///NOTE: Rezygnacja z używania Rotation bo labele są dziećmi CustomSolid
            for (int i = 0; i < labels.Length; i++)
            {
                Vector3 rotatedVertex = vertices[i] * (1.0f+labelOffset);
                Vector3 worldPosition = transform.TransformPoint(rotatedVertex);
                labels[i].transform.position = worldPosition;
                Vector3 playerPos = OBJECT3D.player.transform.position;
                Vector3 directionToPlayer = (playerPos + 2*Vector3.up - labels[i].transform.position).normalized;
                //literki są twarzą do postaci gracza
                labels[i].transform.rotation = Quaternion.LookRotation(-directionToPlayer);
            }
        }
        else
        {
            ClearLabels();
        }
    }  
    /// <summary>
    /// Inicjalizuje wyświetlanie wierzchołków bryły, dla każdego wierzchołka bryły tworzy obiekt, który będzie wyświetlał jego nazwę
    /// </summary>
    /// <param name="obj">Obiekt sterujący bryłą</param>
    /// <param name="meshFilter">Siatka bryły</param>
    /// <param name="labeledVertices">Oznaczenia wierzchołków wraz z ich współrzędnymi</param>
    public void InitLabels(Object3D obj, MeshFilter meshFilter, Dictionary<string, Vector3> labeledVertices)
    {
        this.OBJECT3D = obj;
        ClearLabels();
        if(meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }
    
        if (meshFilter != null)
        {
            vertices = GetUniqueVertices(meshFilter.mesh.vertices);
            //Debug.Log(vertices.Length);
            labels = new GameObject[vertices.Length];
            string[] labelsnames = null;
            if (labeledVertices == null)
                labelsnames = GetBaseLabels();
            else
            {
                labelsnames = ResolveLabels(labeledVertices);
            }
          
            for (int i = 0; i < labels.Length; i++)
            {
                GameObject label = new GameObject("VertexLabel" + i);
                TextMesh textMesh = label.AddComponent<TextMesh>();
                textMesh.text = labelsnames[i];
                textMesh.characterSize = characterSize;
                textMesh.color = color;
                textMesh.font = font;
                labels[i] = label;

                ///podepnij pod wczytany obiekt
                labels[i].transform.SetParent(gameObject.transform);
            }
            
        }
        else
        {
            Debug.LogError("Brak MeshFilter potrzebnego do działania VertexLabels.cs");
        }
    }
    /// <summary>
    /// Odnajduje nazwy wierzchołków
    /// </summary>
    /// <param name="labeledVertices">Oznaczenia wierzchołków wraz z ich położeniem</param>
    /// <returns>Tablica nazw wierzchołków</returns>
    string[] ResolveLabels(Dictionary<string, Vector3> labeledVertices)
    {
        Dictionary<Vector3, string> resolver = new Dictionary<Vector3, string>();
        foreach (var pair in labeledVertices)
        {
            string key = pair.Key;
            Vector3 value = pair.Value;
            resolver.Add(value, key);
            //Debug.Log($"Key: {key}, Value: {value}");
        }
        string[] names = new string[vertices.Length];
        for(int i = 0; i < names.Length; i++)
        {
            names[i] = resolver[vertices[i]];
        }
        return names;
    }
    /// <summary>
    /// Nazywa wierzchołki według domyślnego nazewnictwa
    /// </summary>
    /// <returns>Tablica oznaczeń zaczynającej się od "A"</returns>
    string[] GetBaseLabels()
    {
        int MAXLABELSNUMBER = labels.Length; //max liczba wierzchołków do nazwania
      
        string[] labelsnames = new string[MAXLABELSNUMBER];
        
        for(int i = 0; i < labelsnames.Length; i++ )
        {
            int n = i+1;
            string name = "";
            do
            {
                n--;
                name = ((char)('A'+n%26)).ToString() + name;
                n /= 26;
            } while (n > 0);
            labelsnames[i] = name;
        }
        return labelsnames;
    }
    /// <summary>
    /// Czyszci oznaczenia
    /// </summary>
    void ClearLabels()
    {
        //jeśli był jakiś obiekt wcześniej to usuń dane o nim
        if (labels != null)
        {
            for (int i = 0; i < labels.Length; i++)
                Destroy(labels[i]);
            labels = null;
        }
    
    }
    /// <summary>
    /// Na podstawie siatki obiektu wyciąga listę wierzchołków obiektu bez powtórzeń
    /// </summary>
    /// <param name="meshVertices">Siatka obiektu</param>
    /// <returns>Zwraca tablice wierzchołków obiektu bez powtórzeń</returns>
    Vector3[] GetUniqueVertices(Vector3[] meshVertices)
    {
        HashSet<Vector3> set = new HashSet<Vector3>();
        for (int i = 0; i < meshVertices.Length; i++)
        {
            set.Add(meshVertices[i]);
        }
        Vector3[] uniquevertices = new Vector3[set.Count];
        set.CopyTo(uniquevertices);
        return uniquevertices;
    }
}

/*
    public void SetVars(float labelOffset, Font font, Color color, float characterSize)
    {
        this.labelOffset = labelOffset;
        this.font = font;
        this.color = color;
        this.characterSize = characterSize;
    }*/