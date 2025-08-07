using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Zawiera informacje dot. wyświetlania rzeczy na ścianie
/// </summary>
public class WallInfo
{
    /// <summary>
    /// Obiekt ściany Unity
    /// </summary>
    public GameObject gameObject;
    /// <summary>
    /// Numer ściany
    /// </summary>
    public int number;
    /// <summary>
    /// Nazwa ściany
    /// </summary>
    public string name;
    /// <summary>
    /// Flaga dot. wyświetlania na ścianie rzutów
    /// </summary>
    public bool showProjection = true;
    /// <summary>
    /// Flaga dot. wyświetlania linii rzutujących
    /// </summary>
    public bool showLines = true;
    /// <summary>
    /// Flaga dot. wyświetlania linii odnoszących
    /// </summary>
    public bool showReferenceLines = true;
    /// <summary>
    /// Flaga dot. możliwości usuwania ściany
    /// </summary>
    public bool canDelete = true;

    /// <summary>
    /// Konstruktor inicjujący parametry klasy WallInfo z domyślnie ustawionymi flagami.
    /// </summary>
    /// <param name="gameObject">Obiekt ściany Unity</param>
    /// <param name="number">Numer ściany.</param>
    /// <param name="name">Nazwa ściany.</param>
    public WallInfo(GameObject gameObject, int number, string name){
        this.gameObject = gameObject;
        this.number = number;
        this.name = name;
    }

    /// <summary>
    /// Konstruktor inicjujący wszystkie parametry klasy WallInfo.
    /// </summary>
    /// <param name="gameObject">Obiekt ściany Unity</param>
    /// <param name="number">Numer ściany.</param>
    /// <param name="name">Nazwa ściany.</param>
    /// <param name="showProjection">Flaga dotycząca wyświetlania na ścianie rzutów.</param>
    /// <param name="showLines">Flaga dotycząca wyświetlania linii rzutujących.</param>
    /// <param name="showReferenceLines">Flaga dotycząca wyświetlania linii odnoszących.</param>
    /// <param name="canDelete">Flaga dot. możliwości usuwania ściany.</param>
    public WallInfo(GameObject gameObject, int number, string name, bool showProjection, bool showLines, bool showReferenceLines, bool canDelete)
    {
        this.gameObject = gameObject;
        this.number = number;
        this.name = name;
        this.showProjection = showProjection;
        this.showLines = showLines;
        this.showReferenceLines = showReferenceLines;
        this.canDelete = canDelete;
    }

    /// <summary>
    /// Funkcja zwraca wektor normalny ściany, przyjęty w projektcie jako vector.right
    /// </summary>
    /// <returns>Wektor normalny</returns>
    public Vector3 GetNormal(){
        return gameObject.transform.right;
    }
    /// <summary>
    /// Ustawia flagi na określone wartości
    /// </summary>
    /// <param name="showProjection">Flaga dotycząca wyświetlania na ścianie rzutów.</param>
    /// <param name="showLines">Flaga dotycząca wyświetlania linii rzutujących.</param>
    /// <param name="showReferenceLines">Flaga dotycząca wyświetlania linii odnoszących.</param>
    public void SetFlags(bool showProjection, bool showLines, bool showReferenceLines)
    {
        this.showProjection = showProjection;
        this.showLines = showLines;
        this.showReferenceLines = showReferenceLines;
    }
}