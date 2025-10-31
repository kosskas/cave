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
             * 2. -n2 (r - s*n2 + t*n1) = 0
             *
             * 1. n1*n1 * t - n1*n2 * s = -n1 * r
             * 2. -n1*n2 * t + n2*n2 * s = n2 * r
             *
             * Niech:
             * a = n1 * n1
             * b = n1 * n2
             * c = n2 * n2
             * d = n1 * r
             * e = n2 * r
             *
             * Czyli mamy po uproszczeniu
             *
             * 1.-bs + at = -d
             * 2. cs - bt = e
             *
             * s = (ae - bd) / (ac - b^2)
             * t = (be - cd) / (ac - b^2)
             */
            Vector3 r = p1 - p2;

            float a = Vector3.Dot(n1, n1);
            float b = Vector3.Dot(n1, n2);
            float c = Vector3.Dot(n2, n2);
            float d = Vector3.Dot(n1, r);
            float e = Vector3.Dot(n2, r);

            float mian = a * c - b * b;
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

        public static List<Vector3> FindCCIntersections(Vector3 S1, Vector3 A1, Vector3 S2, Vector3 A2, Vector3 n)
        {
            List<Vector3> intersections = new List<Vector3>();

            float r1 = (A1 - S1).magnitude;
            float r2 = (A2 - S2).magnitude;

            Vector3 d = S2 - S1;
            float dist = d.magnitude;

            // Brak przecięcia
            if (dist > r1 + r2 || dist < Mathf.Abs(r1 - r2) || dist < 1e-6f)
                return intersections;

            // Odległość od S1 do punktu "P" wzdłuż linii łączącej środki
            float a = (r1 * r1 - r2 * r2 + dist * dist) / (2 * dist);
            float h = Mathf.Sqrt(r1 * r1 - a * a);

            // Punkt P - środek linii łączącej punkty przecięcia
            Vector3 P = S1 + a * d.normalized;

            // Wektor prostopadły do d w płaszczyźnie okręgu
            Vector3 dPerp = Vector3.Cross(n.normalized, d.normalized).normalized;

            // Dwa punkty przecięcia
            Vector3 i1 = P + h * dPerp;
            Vector3 i2 = P - h * dPerp;

            intersections.Add(i1);
            intersections.Add(i2);
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
        public static Vector3 FindLinePlaneIntersections(Vector3 l1_p, Vector3 l1_n, Vector3 l2_p, Vector3 l2_n)
        {
            return Vector3.zero;
        }

    }
}
