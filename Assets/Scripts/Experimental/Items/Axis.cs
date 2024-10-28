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


        private Vector3 _from;

        private Vector3 _to;

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
            _lineRenderer.SetPositions(new[] { _from, _to });

            Vector3 newPosition = _from;
            Quaternion newRotation = Quaternion.LookRotation((_to - _from).normalized, Vector3.up) * Quaternion.Euler(0, -90, 0);

            gameObject.transform.position = newPosition;
            gameObject.transform.rotation = newRotation;
        }


        public void Draw(params Vector3[] positions)
        {
            _from = positions[0];
            _to = positions[1];
        }

        public void Erase()
        {
            throw new NotImplementedException();
        }
    }
}
