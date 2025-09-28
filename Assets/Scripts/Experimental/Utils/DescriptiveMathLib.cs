using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Experimental.Utils
{
    public static class DescriptiveMathLib
    {
        private const float EPS = 1e-5f;
        public static Tuple<Vector3, Vector3> FindLLIntersections(Vector3 p1, Vector3 n1, Vector3 p2, Vector3 n2)
        {
            /* Komentarz
             * ( '*' to możenie skalarne, czyli powstaje liczba)
             * Proste:
             * l1(t) = p1 + t * n1
             * l2(s) = p2 + s * n2
             *
             * Szukamy najkrótszego odcinka (t, s) między prostymi l1 i l2 takiego że 't' in l1, 's' in l2
             *
             * Funkcja odl. d(s, t) = |l1(t) - l2(s)|^2
             * Szukamy takich 's' i 't', że 'd' jest MIN
             * MIN jest wtedy kiedy d'(t) = d'(s) = 0
             *
             * d'(t) = 2n1 (p1 - p2 - s*n2 + t*n1) = 0
             * d'(s) = -2u2 (p1 - p2 - s*n2 + t*n1) = 0
             *
             * dla uproszczenia r = p1 - p2
             *
             * Układ równań (to wektory więc nie można dzielić)
             * 1. n1 (r - s*n2 + t*n1) = 0
             * 2. n2 (r - s*n2 + t*n1) = 0
             *
             * 1. n1*n1 * t - n1*n2 * s = -n1 * r
             * 2. n1*n2 * t - n2*n2 * s = -n2 * r
             *
             * Niech:
             * a = n1 * n1
             * b = n1 * n2
             * c = n2 * n2
             * d = -n1 * r
             * e = -n2 * r
             *
             * Czyli mamy po uproszczeniu
             *
             * 1. at - bs = d
             * 2. bt - cs = e
             *
             * s = (ae - bd) / (b^2 - ac)
             * t = (be - cd) / (b^2 - ac)
             */
            Vector3 r = p1 - p2;

            float a = Vector3.Dot(n1, n1);
            float b = Vector3.Dot(n1, n2);
            float c = Vector3.Dot(n2, n2);
            float d = -Vector3.Dot(n1, r);
            float e = -Vector3.Dot(n2, r);

            float mian = b * b - a * c;
            if (Mathf.Abs(mian) < EPS)
            {
                Debug.LogWarning($"Płaszczyzny są równoległe lub prawie równoległe – brak przecięcia. n1={n1}, n2={n2}");
                return null;
            }

            float t = (b * e - c * d) / mian;
            float s = (a * e - b * d) / mian;

            Vector3 point1 = p1 + t * n1;
            Vector3 point2 = p2 + s * n2;

            return new Tuple<Vector3, Vector3>(point1, point2);
        }
        public static List<Vector3> FindLCIntersections(Vector3 A, Vector3 B, Vector3 S, float r)
        {
            List<Vector3> intersections = new List<Vector3>();

            Vector3 u = B - A;            // kierunek prostej
            Vector3 AS = A - S;

            float a = Vector3.Dot(u, u);
            float b = 2f * Vector3.Dot(u, AS);
            float c = Vector3.Dot(AS, AS) - r * r;

            float delta = b * b - 4f * a * c;
            if (delta < 0f)
            {
                return intersections; // brak przecięć
            }

            float sqrtDelta = Mathf.Sqrt(delta);
            float t1 = (-b - sqrtDelta) / (2f * a);
            float t2 = (-b + sqrtDelta) / (2f * a);

            // Punkt 1
            Vector3 P1 = A + t1 * u;
            intersections.Add(P1);

            // Punkt 2 (może być taki sam jak P1 przy delta==0)
            if (delta > 1e-6f)
            {
                Vector3 P2 = A + t2 * u;
                intersections.Add(P2);
            }

            return intersections;
        }

        public static List<Vector3> FindCCIntersections(Vector3 S1, Vector3 A1, Vector3 S2, Vector3 A2)
        {
            List<Vector3> intersections = new List<Vector3>();

            float r1 = (A1 - S1).magnitude;
            float r2 = (A2 - S2).magnitude;

            // Normalna wspólnej płaszczyzny (przez S1, S2, A2)
            Vector3 n = Vector3.Cross(S2 - S1, A2 - S1).normalized;
            if (n.sqrMagnitude < 1e-8f)
            {
                Debug.LogWarning("Punkty współliniowe -> brak jednoznacznej płaszczyzny.");
                return intersections;
            }

            // Budujemy bazę w tej płaszczyźnie
            Vector3 e1 = (S2 - S1).normalized;
            Vector3 e2 = Vector3.Cross(n, e1).normalized;

            // Rzutujemy środki do 2D
            Vector2 S1_2 = Vector2.zero;
            Vector2 S2_2 = new Vector2((S2 - S1).magnitude, 0f);

            // Promienie
            float d = (S2 - S1).magnitude;
            if (d < 1e-6f) return intersections; // te same środki -> nieskończoność albo nic

            // Odległość od S1 do osi przecięcia dwóch kół
            float x = (d * d - r2 * r2 + r1 * r1) / (2f * d);
            float y2 = r1 * r1 - x * x;
            if (y2 < -1e-6f)
            {
                return intersections; // brak przecięć
            }

            float y = Mathf.Sqrt(Mathf.Max(0f, y2));

            // Dwa rozwiązania w 2D
            Vector2 P1_2 = new Vector2(x, y);
            Vector2 P2_2 = new Vector2(x, -y);

            // Rzutowanie z powrotem do 3D
            Vector3 P1 = S1 + P1_2.x * e1 + P1_2.y * e2;
            intersections.Add(P1);

            if (y > 1e-6f)
            {
                Vector3 P2 = S1 + P2_2.x * e1 + P2_2.y * e2;
                intersections.Add(P2);
            }

            return intersections;
        }
        public static bool IsPointOnSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            const float epsilon = 1e-5f;
            float segmentLength = Vector3.Distance(a, b);
            float d1 = Vector3.Distance(a, p);
            float d2 = Vector3.Distance(b, p);

            return (d1 + d2 <= segmentLength + epsilon);
        }


    }
}
