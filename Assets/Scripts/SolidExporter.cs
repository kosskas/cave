using System.Collections;
using System.Collections.Generic;
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
    private const string solidFileExt = "*.wobj";
}
