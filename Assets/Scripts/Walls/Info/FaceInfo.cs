using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceInfo {

    
    public List<KeyValuePair<string, Vector3>> Points { get; set; }
    public GameObject FaceObject { get; set; }

    public FaceInfo(List<KeyValuePair<string, Vector3>> points, GameObject faceObject)
    {
        Points = points;
        FaceObject = faceObject;
    }
}
