using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Linq;


/// <summary>
/// Struktura przechowująca informacje identyfikujące krawędź bryły
/// </summary>
public struct EdgeInfo {

	/// <summary>
	/// Etykieta tekstowa krawędzi
	/// </summary>
	public string label;

	/// <summary>
	/// Etykiety tekstowe wierzchołków na początku i końcu krawędzi
	/// </summary>
	public Tuple<string, string> endPoints;

	/// <summary>
	/// Konstruktur, który ustawia tekst etykiet
	/// </summary>
	/// <param name="label">Etykieta tekstowa krawędzi, typ string</param>
	/// <param name="endPoints">Krotka dwóch stringów, Item1 to etykieta tekstowa wierzchołka na początku krawędzi, a Item2 na końcu krawędzi</param>
	public EdgeInfo(string label, Tuple<string, string> endPoints)
	{
		this.label = label;
		this.endPoints = endPoints;
	}
}


/// <summary>
/// Klasa statyczna zawierająca definicję bryły domyślnej (sześcianu). Bryła domyślna jest renderowana, jeśli folder zawierający pliki z rozszerzeniem .wobj nie zostanie znaleziony. 
/// </summary>
public static class DefaultSolid {

	/// <summary>
	/// Wierzchołki bryły domyślnej
	/// { klucz:<etykieta tekstowa wierzchołka>, wartość:<współrzędne 3D wierzchołka, liczone względem środka ciężkości bryły (środek cieżkości == (0,0,0))> }
	/// </summary>
	public static Dictionary<string, Vector3> Verticies = new Dictionary<string, Vector3>()
	{
		{ "A", new Vector3(-0.3f, -0.3f, -0.3f) },
		{ "B", new Vector3(-0.3f, -0.3f, 0.3f) },
		{ "C", new Vector3(-0.3f, 0.3f, -0.3f) },
		{ "D", new Vector3(-0.3f, 0.3f, 0.3f) },

		{ "E", new Vector3(0.3f, -0.3f, -0.3f) },
		{ "F", new Vector3(0.3f, -0.3f, 0.3f) },
		{ "G", new Vector3(0.3f, 0.3f, -0.3f) },
		{ "H", new Vector3(0.3f, 0.3f, 0.3f) }
	};

	/// <summary>
	/// Krawędzie bryły domyślnej
	/// label:<etykieta tekstowa krawędzi>, endPoints:(<etykieta tekstowa wierzchołka na początku krawędzi>,<etykieta tekstowa wierzchołka na końcu krawędzi>)
	/// </summary>
	public static List<EdgeInfo> Edges = new List<EdgeInfo>()
	{
		new EdgeInfo("a", new Tuple<string, string>("A", "C")),
		new EdgeInfo("b", new Tuple<string, string>("C", "D")),
		new EdgeInfo("c", new Tuple<string, string>("D", "B")),
		new EdgeInfo("d", new Tuple<string, string>("B", "A")),

		new EdgeInfo("e", new Tuple<string, string>("A", "E")),
		new EdgeInfo("f", new Tuple<string, string>("C", "G")),
		new EdgeInfo("g", new Tuple<string, string>("D", "H")),
		new EdgeInfo("h", new Tuple<string, string>("B", "F")),

		new EdgeInfo("i", new Tuple<string, string>("E", "G")),
		new EdgeInfo("j", new Tuple<string, string>("G", "H")),
		new EdgeInfo("k", new Tuple<string, string>("H", "F")),
		new EdgeInfo("l", new Tuple<string, string>("F", "E"))
	};

	/// <summary>
	/// Ściany bryły domyślnej
	/// Lista etykiet tekstowych wierzchołków ściany, wymienionych w kolejności zgodnej z ruchem wskazówek zegara, patrząc od zewnętrznej strony bryły
	/// </summary>
	public static List<List<string>> Faces = new List<List<string>>()
		{
			// ściana #1
			new List<string>(){ "A", "C", "G", "E" },

			// ściana #2
			new List<string>(){ "C", "D", "H", "G" },

			// ściana #3
			new List<string>(){ "E", "G", "H", "F" },

			// ściana #4
			new List<string>(){ "E", "F", "B", "A" },

			// ściana #5
			new List<string>(){ "F", "H", "D", "B" },

			// ściana #6
			new List<string>(){ "B", "D", "C", "A" }
		};

	/// <summary>
	/// Siatka ścian bryły domyślnej
	/// Definicja trójkątów budujących ściany bryły, wartości należy traktować jako indeks wierzchołka w tablicy Faces.ToArray()
	/// </summary>
	public static List<int> Triangles = new List<int>()
	{
		// ściana #1
		0, 1, 2,	// A C G
		0, 2, 3,	// A G E

		// ściana #2
		4, 5, 6,	// C D H
		4, 6, 7,	// C H G 

		// ściana #3
		8, 9, 10,	// E G H
		8, 10, 11,	// E H F

		// ściana #4
		12, 13, 14,	// E F B
		12, 14, 15,	// E B A

		// ściana #5
		16, 17, 18,	// F H D
		16, 18, 19,	// F D B

		// ściana #6
		20, 21, 22,	// B D C
		20, 22, 23	// B C A
	};
}


/// <summary>
/// Klasa SolidImporter służy do parsowania i ładowania brył zapisanych w formacie .wobj to aplikacji.
/// </summary>
public class SolidImporter : MonoBehaviour {

	/// <summary>
	/// Ścieżka względna dostępu do katalogu zawierającego pliki w formacie .wobj
	/// </summary>
	#if UNITY_EDITOR
		private const string pathToFolderWithSolids = "./Assets/Figures3D";
	#else
		private const string pathToFolderWithSolids = "./Figures3D";
	#endif

	/// <summary>
	/// Flaga informająca czy katalog 'pathToFolderWithSolids' został odnaleziony i czy zawiera pliki w formacie .wobj
	/// </summary>
	private bool isSolidFolderValid = false;

	/// <summary>
	/// Rozszerzenie plików zawierających opis cystomowych brył
	/// </summary>
	private const string solidFileExt = "*.wobj";
	
	/// <summary>
	/// Maksymalna odległość wierzchołka od środka ciężkości bryły po przeskalowaniu 
	/// </summary>
	private const float MAX_RADIUS_TRESHOLD = 0.8f;

	/// <summary>
	/// Współczynnik skalowania rozmiaru bryły, tak aby wartość 1 w pliku .wobj odpowiadała 'SCALING_FACTOR' [m] w jaskini
	/// </summary>
	private const float SCALING_FACTOR = 0.5f;

	/// <summary>
	/// Nazwy plików .wobj znalezione w katalogu 'pathToFolderWithSolids'
	/// </summary>
	private string[] solidFiles = null;

	/// <summary>
	/// Indeks aktualnie wczytanego pliku .wobj z 'solidFiles'
	/// </summary>
	private int currentSolidFileIndex;

	/// <summary>
	/// Wierzchołki bryły
	/// { klucz:<etykieta tekstowa wierzchołka>, wartość:<współrzędne 3D wierzchołka, liczone względem środka ciężkości bryły (środek cieżkości == (0,0,0))> }
	/// </summary>
	private Dictionary<string, Vector3> labeledVertices = new Dictionary<string, Vector3>();

	/// <summary>
	/// Ściany bryły
	/// Lista etykiet tekstowych wierzchołków ściany, wymienionych w kolejności zgodnej z ruchem wskazówek zegara, patrząc od zewnętrznej strony bryły
	/// </summary>
	private List<List<string>> faces = new List<List<string>>();

	/// <summary>
	/// Lista współrzędnych wierzchołków bryły. Ustawione są w tej samej kolejności, co w 'labeledVertices'.
	/// </summary>
	private List<Vector3> vertices = new List<Vector3>();

	/// <summary>
	/// Siatka ścian bryły domyślnej
	/// Definicja trójkątów budujących ściany bryły, wartości należy traktować jako indeks wierzchołka w tablicy faces.ToArray()
	/// </summary>
	private List<int> triangles = new List<int>();

	/// <summary>
	/// Krawędzie bryły
	/// EdgeInfo( label:<etykieta tekstowa krawędzi>, endPoints:(<etykieta tekstowa wierzchołka na początku krawędzi>,<etykieta tekstowa wierzchołka na końcu krawędzi>) )
	/// </summary>
	private List<EdgeInfo> edges = new List<EdgeInfo>();

	/// <summary>
	/// Referencja na dynamicznie dodawany obiekt renderowanej bryły
	/// </summary>
	private GameObject customSolid;

	/// <summary>
	/// Referencja na stale obecny obiekt, do którego dynamicznie dołączany jest obiekt 'customSolid'
	/// </summary>
	private GameObject mainObj;

	private int direction = 1;
	// Use this for initialization
	void Start ()
	{
		mainObj = GameObject.Find("MainObject");

		try 
		{
			solidFiles = Directory.GetFiles(pathToFolderWithSolids, solidFileExt);
			Debug.Log(solidFiles);
		}
		catch (System.Exception) 
		{
			solidFiles = null;
			Debug.LogError("[CAVE] It seems that folder " + Application.dataPath + pathToFolderWithSolids + " does not exist.");
		}
		
		currentSolidFileIndex = 0;

		isSolidFolderValid = (solidFiles == null || solidFiles.Length == 0) ? false : true;
	}


	/// <summary>
	/// Zawiera cały proces usuniecia dotychczas renderowanej bryły i załadowania oraz wyrenderowania kolejnej bryły z 'solidFiles'
	/// Szuka następną bryłę do załadowania w folderze i ładuje ją. W trakcie działania parsuje plik w formacie wobj, środkuje bryłę względem 0,0,0. Ładuje Object3D -centralną, klasę, która podłącza resztę komponentów
	/// </summary>
	public void ImportSolid () 
	{
		DeleteMainObjChild();
		DeleteSolid();
		ClearSolid();

		if (isSolidFolderValid)
		{
			PickNextSolid();
			ReadSolid();
			CentralizePosition();
			ScaleSolid();
			NormalizeSolid();
			SetUpVertices();
			SetUpTriangles();
		}

		CreateSolidObject();

		LogStatus();
	}

	/// <summary>
	/// Usuń dziecko obiektu 'mainObj', jeśli ono istnieje
	/// </summary>
	private void DeleteMainObjChild() 
	{
		if (mainObj.transform.childCount > 0) 
		{
			Destroy(mainObj.transform.GetChild(0).gameObject);
		}
	}

	/// <summary>
	/// Usuń obiekt 'customSolid' jeśli istnieje
	/// </summary>
	private void DeleteSolid()
	{
		Destroy(customSolid);
	}

	/// <summary>
	/// Wyczyść zawartość struktur zawierających informacje o aktualnie wczytanej bryle
	/// </summary>
	private void ClearSolid() 
	{
		labeledVertices.Clear();
		faces.Clear();
		edges.Clear();
		vertices.Clear();
		triangles.Clear();
	}
	/// <summary>
	/// Ustawia kolejność ładowania kolejnych obiektów w górę
	/// </summary>
	public void SetUpDirection()
	{
		direction = 1;
	}
    /// <summary>
    /// Ustawia kolejność ładowania kolejnych obiektów w dół
    /// </summary>
    public void SetDownDirection()
	{
		direction = -1;
    }
	/// <summary>
	/// Metoda inkrementuje w sposób zapętlony w górę bądź dół (modulo liczba plików w 'solidFiles') indeks pliku do załadowania
	/// </summary>
	private void PickNextSolid() 
	{
        currentSolidFileIndex = (currentSolidFileIndex + direction) % solidFiles.Length;
        if (currentSolidFileIndex < 0)
        {
            currentSolidFileIndex += solidFiles.Length;
        }
    }

	/// <summary>
	/// Metoda rozpoznaje sekcje wczytanego pliku i odpowiednio je interpretuje
	/// </summary>
	private void ReadSolid() 
	{
		if (File.Exists(GetCurrentSolid()))
        {
            using (StreamReader reader = new StreamReader(GetCurrentSolid()))
            {
				int part = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
					if (line.Contains("###")) part++;
					else if (String.IsNullOrEmpty(line)) continue;
					else if (part == 1) { ReadVertex(line); }
					else if (part == 2) { ReadFace(line); ReadEdges(line); }
					else if (part > 2) break;
                }
            }
        }
        else
        {
			Debug.LogError(Path.GetFileName(GetCurrentSolid()) + " not found!");
        }
	}

	/// <summary>
	/// Metoda wczytuje dane wierzchołków z pliku
	/// </summary>
	/// <param name="line">Wiersz z pliku .wobj</param>
	private void ReadVertex(string line) 
	{
		string[] vertexData = line.Trim().Split(' ');

		string label = vertexData[0];
		float x = float.Parse(vertexData[1], CultureInfo.InvariantCulture);
		float y = float.Parse(vertexData[2], CultureInfo.InvariantCulture);
		float z = float.Parse(vertexData[3], CultureInfo.InvariantCulture);

		labeledVertices[label] = new Vector3(x, y, z);
	}

	/// <summary>
	/// Metoda wczytuje dane ścian z pliku
	/// </summary>
	/// <param name="line">Wiersz z pliku .wobj</param>
	private void ReadFace(string line)
	{
		string[] faceData = line.Trim().Split(',');

		List<string> face = new List<string>();
		Array.ForEach(faceData, label => face.Add(label));

		faces.Add(face);
	}

	/// <summary>
	/// Metoda wczytuje dane krawędzi z pliku
	/// </summary>
	/// <param name="line">Wiersz z pliku .wobj</param>
	private void ReadEdges(string line) 
	{
		string[] vLabelsOfFace = line.Trim().Split(',');

		for (int i = 0; i < vLabelsOfFace.Length; i++)
		{
			int v1 = i % vLabelsOfFace.Length;
			int v2 = (i+1) % vLabelsOfFace.Length;

			if (edges.Exists(edge => 
					(edge.endPoints.Item1 == vLabelsOfFace[v1] && edge.endPoints.Item2 == vLabelsOfFace[v2]) ||
					(edge.endPoints.Item2 == vLabelsOfFace[v1] && edge.endPoints.Item1 == vLabelsOfFace[v2]) 
			))
			{
				continue;
			}

			edges.Add(
				new EdgeInfo("", new Tuple<string, string>(vLabelsOfFace[v1], vLabelsOfFace[v2]))
			);
		}
	}

	/// <summary>
	/// Metoda zwraca nazwę pliku .wobj, który ma zostać wczytany
	/// </summary>
	/// <returns></returns>
	private string GetCurrentSolid()
	{
		return solidFiles[currentSolidFileIndex];
	}

	/// <summary>
	/// Metoda dokonuje translacji współrzędnych wierzchołków bryły, tak aby środek cieżkości bryły znalazł sie w punkcie o współrzędnych (0,0,0). Złożoność metody: O(n).
	/// </summary>
	private void CentralizePosition()
	{
		Vector3 centerPoint = new Vector3(0, 0, 0);
		int n = labeledVertices.Count;

		foreach(Vector3 vertex in labeledVertices.Values)
		{
			centerPoint += vertex;
		}

		centerPoint /= n;

		labeledVertices = labeledVertices.ToDictionary(entry => entry.Key, entry => entry.Value - centerPoint);
	}


	private void ScaleSolid()
	{
		labeledVertices = labeledVertices.ToDictionary(entry => entry.Key, entry => entry.Value * SCALING_FACTOR);
	}


	/// <summary>
	/// Metoda normalizuje rozmiar bryły, tak aby niezależnie od współrzędnych wierzchołków zdefiniowanych w pliku .wobj,
	/// nie przyjmowała większego rozmiaru niż 'MAX_DISTANCE'*'MAX_DISTANCE' x 'MAX_DISTANCE'*'MAX_DISTANCE' x 'MAX_DISTANCE'*'MAX_DISTANCE'.
	/// </summary>
	private void NormalizeSolid()
	{
		float maxRadius = 0.0f;

		foreach(Vector3 vertex in labeledVertices.Values)
		{
			float radius = (float)Math.Sqrt(
				(float)Math.Pow(vertex.x, 2) + 
				(float)Math.Pow(vertex.y, 2) +
				(float)Math.Pow(vertex.z, 2)
			);

			maxRadius = (radius > maxRadius) ? radius : maxRadius;
		}

		if (maxRadius > MAX_RADIUS_TRESHOLD)
		{
			labeledVertices = labeledVertices.ToDictionary(entry => entry.Key, entry => (entry.Value * MAX_RADIUS_TRESHOLD) / maxRadius);
		}
	}

	/// <summary>
	/// Metoda dla każdej ściany dodaje do listy wierzchołków 'vertices' współrzędne wierzchołków budujących ścianę, z zachowaniem kolejności wierzchołków
	/// </summary>
	private void SetUpVertices() 
	{
		faces.ForEach( face => face.ForEach( label => vertices.Add(labeledVertices[label])));
	}

	/*  SetUpTriangles() 
	* face is a sequence of n vertices indexed from 0 to n-1
	* used algorithm to cover face with minimal number of trangles without intersected areas:
	* - i-th triangle - | - indexes -
	* 			1		|	0 1 2
	*			2		|	0 2 3
	*			3		|	0 3 4
	*			...		|	...
	*			n-2		|	0 n-2 n-1
	*/
	/// <summary>
	/// Ściana bryły jest opisana sekwencją n wierzchołków indeksowanych od 0 do n-1.
	/// Algorytm zaimplementowany w metodzie pokrywa każdą ścianę minimalną liczbą trójkątów, które nie nachodzą na siebie (ich pola są rozłączne).
	///  - i-ty trójkąt - | - indeksy wierzchołków bryły, tworzących trójkąt -
	///			1		  |	  0 1 2
	///			2		  |	  0 2 3
	///			...		  |	  ...
	///			n-2		  |	  0 n-2 n-1
	/// </summary>
	private void SetUpTriangles()
	{
		int startVertexIndex = 0;

		faces.ForEach( face => {
			const int firstVertexOffset = 0;
			int secondVertexOffset = 1;
			int thirdVertexOffset = 2;

			for (int ithTriangle = 1; ithTriangle <= face.Count - 2; ithTriangle++)
			{
				triangles.AddRange(new List<int>{
					startVertexIndex + firstVertexOffset,
					startVertexIndex + secondVertexOffset,
					startVertexIndex + thirdVertexOffset
				});

				secondVertexOffset++;
				thirdVertexOffset++;
			}

			startVertexIndex += face.Count;
		});
	}

	/// <summary>
	/// Metoda dodaje do 'mainObj' obiekt 'customSolid' i inicjuje utworzenie oraz wyrenderowanie bryły
	/// </summary>
	private void CreateSolidObject()
	{
		// Create a new GameObject to hold the custom solid
        customSolid = new GameObject("CustomSolid");

		// Set parent as MainObject
		customSolid.transform.SetParent(mainObj.transform);

		// Zerowanie współrzędnych położenia 'mainObj' i 'customSolid'
		transform.position = Vector3.zero;
        customSolid.transform.position = Vector3.zero;

		// Dodanie Object3D - centralnej klasy która podepnie resztę komponentów
		Object3D object3D = customSolid.AddComponent<Object3D>();
		
		if (isSolidFolderValid)
		{
			object3D.InitObject(labeledVertices, edges, faces, triangles);
		}
		else
		{
			object3D.InitObject(DefaultSolid.Verticies, DefaultSolid.Edges, DefaultSolid.Faces, DefaultSolid.Triangles);
		}
	}

	/// <summary>
	/// Metoda odpowiada za logowanie w konsoli listy nazw znalezionych plików .wobj z wyróżnieniem aktualnie wczytanego i renderowanego.
	/// Jeśli nie znaleziono żadnych plików, metoda wypisuje w konsoli ostrzeżenie o załadowaniu domyślnej bryły.
	/// </summary>
    private void LogStatus() 
	{
		if (solidFiles != null && solidFiles.Length > 0)
		{
			StringBuilder infoString = new StringBuilder();
			infoString.Append(String.Format(" Current solid: {0}", Path.GetFileName(GetCurrentSolid())));
			infoString.Append(String.Format("\n | All solids:"));

			foreach (string solidFile in solidFiles)
			{
				infoString.Append(String.Format(" {0}", Path.GetFileName(solidFile)));
			}

			Debug.Log(infoString.ToString());
		}
		else
		{
			Debug.LogWarning("[CAVE] No solids found! Default solid has loaded.");
		}
	}

}
