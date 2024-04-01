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
    private GameObject gameObject;
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
    /// Flaga dot. pilnowania prostopadłości rzutu na ścianie
    /// </summary>    
    public bool watchPerpendicularity = false;


    private Transform oldtrans;
    private Rotation oldrotat;

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
    /// <param name="watchPerpendicularity">Flaga dotycząca pilnowania prostopadłości rzutu na ścianie.</param>
    public WallInfo(GameObject gameObject, int number, string name, bool showProjection, bool showLines, bool showReferenceLines, bool watchPerpendicularity)
    {
        this.gameObject = gameObject;
        this.number = number;
        this.name = name;
        this.showProjection = showProjection;
        this.showLines = showLines;
        this.showReferenceLines = showReferenceLines;
        this.watchPerpendicularity = watchPerpendicularity;
    }

    public Vector3 GetNormal(){
        return gameObject.transform.right;
    }
}