using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class SolidExporter{
    /// <summary>
    /// Ścieżka względna dostępu do katalogu zawierającego pliki w formacie .wobj
    /// </summary>
    #if UNITY_EDITOR
        private const string pathToFolderWithSolids = "./Assets/Figures3D";
    #else
        private const string pathToFolderWithSolids = "./Figures3D";
    #endif

    /// <summary>
    /// Rozszerzenie plików zawierających opis cystomowych brył
    /// </summary>
    private const string solidFileExt = ".wobj";
    private const string startSection = "###\n";
    private const string newline = "\n";
    /// <summary>
    /// Eksportuje punkty i krawędzie wyświetlane w 3D do pliku .wobj
    /// </summary>
    /// <param name="points">Zbiór punktów w 3D</param>
    /// <param name="edges">Zbiór krawędzi w 3D</param>
    /// <returns>Zwraca dane obiektu zapisane w formacie wobj lub null jeśli coś pójdzie nie tak</returns>
    public static string ExportSolid(Dictionary<string, Vector3> points, List<Tuple<string, string>> edges, Dictionary<GameObject, List<string>> faces)
    {
        if(points == null || points.Count == 0 || edges == null)
        {
            return null;
        }
        if(!Directory.Exists(pathToFolderWithSolids))
        {
            Debug.LogError("[CAVE] It seems that folder " + Application.dataPath + pathToFolderWithSolids + " does not exist.");
            return null;
        }

        string points_section = AddPointSection(points);
        string edgesAndFaces_section = AddEdgesAndFacesSection(edges, faces);

        string solid = points_section + edgesAndFaces_section + startSection;
        string fileName = DateTime.Now.ToString("HHmmss")+solidFileExt;
        string path = Path.Combine(pathToFolderWithSolids, fileName);
        try
        {
            File.WriteAllText(path, solid);
            Debug.Log($"[CAVE] Saved file as {fileName}");
        }
        catch (Exception e)
        {
            Debug.LogError("[CAVE] Cannot save file");
            return null;
        }
        return solid;
    }
    private static string AddPointSection(Dictionary<string, Vector3> points)
    {
        string section = startSection;
        foreach (string key in points.Keys)
        {
            section += $"{key} {points[key].x.ToString(CultureInfo.InvariantCulture)} " +
                       $"{points[key].y.ToString(CultureInfo.InvariantCulture)} " +
                       $"{points[key].z.ToString(CultureInfo.InvariantCulture)}\n";
        }

        section += newline;
        return section;
    }
    private static string AddEdgesAndFacesSection(List<Tuple<string, string>> edges, Dictionary<GameObject, List<string>> faces)
    {
        string section = startSection;
        if (edges != null)
        {
            foreach (var pair in edges)
            {
                section += $"{pair.Item1},{pair.Item2}\n";
            }
        }
        if (faces != null)
        {
			section += newline;
            foreach (var edgelist in faces)
            {
                section += string.Join(",", edgelist.Value)+newline;
            }
        }
        section += newline;
        return section;
    }
}
