using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace Assets.Scripts.Experimental.Items
{
    public class Axis : MonoBehaviour, IDrawable
    {
        private readonly Color _COLOR = Color.black;

        private const float _WIDTH = 0.01f;

        public WallInfo Plane { get; private set; }

        public Vector3 From { get; private set; }

        public Vector3 To { get; private set; }

        private LineRenderer _lineRenderer;
        

        void Start()
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.material = new Material(Shader.Find("Unlit/Color"))
            {
                color = _COLOR
            };
            _lineRenderer.positionCount = 2;

            _lineRenderer.startWidth = _WIDTH;
            _lineRenderer.endWidth = _WIDTH;
        }

        void Update()
        {
            _lineRenderer.SetPositions(new[] { From, To });

            Vector3 newPosition = From;
            Quaternion newRotation = Quaternion.LookRotation((To - From).normalized, Vector3.up) * Quaternion.Euler(0, -90, 0);

            gameObject.transform.position = newPosition;
            gameObject.transform.rotation = newRotation;
        }


        public void Draw(WallInfo plane, params Vector3[] positions)
        {
            From = positions[0];
            To = positions[1];

            Plane = plane;
        }
    }
}
