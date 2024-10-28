using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class WallGenerator : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //if (Input.GetKeyDown("m"))
        //{
        //    TestPunktow();
        //    
        //}
    }

    private bool CheckIfPointsAreOnTheSamePlane(List<Point> points)
    {
        bool areOnSamePlane = true;
        if (points.Count() < 3)
        {
            return false;
        }
        else
        {
            Vector3 cords1 = points[0].GetCoordinates();
            Vector3 cords2 = points[1].GetCoordinates();
            Vector3 cords3 = points[2].GetCoordinates();

            for (int i = 3; i < points.Count(); i++)
            {
                Vector3 cords4 = points[i].GetCoordinates();

                float a11a22a33 = (cords4.x - cords1.x) * (cords4.y - cords2.y) * (cords4.z - cords3.z);
                float a12a23a31 = (cords4.y - cords1.y) * (cords4.z - cords2.z) * (cords4.x - cords3.x);
                float a13a21a32 = (cords4.z - cords1.z) * (cords4.x - cords2.x) * (cords4.y - cords3.y);
                float a13a22a31 = (cords4.z - cords1.z) * (cords4.y - cords2.y) * (cords4.x - cords3.x);
                float a11a23a32 = (cords4.x - cords1.x) * (cords4.z - cords2.z) * (cords4.y - cords3.y);
                float a12a21a33 = (cords4.y - cords1.y) * (cords4.x - cords2.x) * (cords4.z - cords3.z);
                float posPart = a11a22a33 + a12a23a31 + a13a21a32;
                float negPart = a13a22a31 + a11a23a32 + a12a21a33;
                float det = posPart - negPart;
                Debug.Log("Wyznacznik macierzy: "+ det + '\n');

                if (!Mathf.Approximately(det, 0f))
                {
                    areOnSamePlane = false;
                    break;
                }
            }

        }

        return areOnSamePlane;
    }

    //private void TestPunktow()
    //{
    //    Point point1 = new Point();
    //    Point point2 = new Point();
    //    Point point3 = new Point();
    //    Point point4 = new Point();
    //    Point point5 = new Point();
    //    point1.SetCoordinates(new Vector3(1f,2f,3f));
    //    point2.SetCoordinates(new Vector3(4f,5f,6f));
    //    point3.SetCoordinates(new Vector3(7f, 0f, 5f));
    //    point4.SetCoordinates(new Vector3(3f, 1f, 4f));
    //    point5.SetCoordinates(new Vector3(2f, 4f, 2f));
    //    List<Point> points = new List<Point>
    //    {
    //        point1,
    //        point2,
    //        point3,
    //        point4,
    //        point5
    //    };

    //    CheckIfPointsAreOnTheSamePlane(points);


    //}

    private void generateWall(List<Point> points)
    {
        if (points.Count() < 3)
        {
            Debug.Log("Conajmniej 3 punkty muszą zostać wybrane, by ściana mogła powstać.");
            return;
        }
        else if (points.Count() == 3)
        {
            //wygeneruj ściane
        }
        else
        {
            if (CheckIfPointsAreOnTheSamePlane(points))
            {
                //wygeneruj ściane
            }
        }


    }
}
