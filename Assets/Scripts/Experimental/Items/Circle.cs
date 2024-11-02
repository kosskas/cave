using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Experimental.Utils;
using UnityEngine;
using UnityEngine.Analytics;

namespace Assets.Scripts.Experimental.Items
{
    public class Circle : MonoBehaviour, IDrawable, IRaycastable
    {
        private static readonly Color ColorNormal = Color.black;
        private static readonly Color ColorFocused = Color.red;

        private static readonly float Width = 0.002f;

        private static readonly int PositionsCount = 100;

        private Vector3 _center;

        private float _radius;

        private Vector3 _radiusEnd;

        public bool ColliderEnabled { get; set; } = true;

        public WallInfo Plane { get; private set; }

        private LineRenderer _circleRenderer;

        private RingCollider _ringCollider;


        void Start()
        {
            _circleRenderer = gameObject.AddComponent<LineRenderer>();
            _circleRenderer.material = new Material(Shader.Find("Unlit/Color"))
            {
                color = ColorNormal
            };
            _circleRenderer.positionCount = PositionsCount + 1;

            _circleRenderer.startWidth = Width;
            _circleRenderer.endWidth = Width;

            _ringCollider = gameObject.AddComponent<RingCollider>();
            UpdateRingCollider();
        }

        void Update()
        {
            for (int step = 0; step <= PositionsCount; step++)
            {
                float progress = (float)step / PositionsCount;
                float radian = 2 * Mathf.PI * progress;

                float x = Mathf.Cos(radian) * _radius;
                float y = Mathf.Sin(radian) * _radius;

                Vector3 currPos = _center + gameObject.transform.forward * x + gameObject.transform.up * y;
                _circleRenderer.SetPosition(step, currPos);
            }

            Vector3 newPosition = _center;

            gameObject.transform.position = newPosition;

            UpdateRingCollider();
        }

        private void UpdateRingCollider()
        {
            _ringCollider.Center = _center;
            _ringCollider.Radius = _radius;
            _ringCollider.Width = Width;
            _ringCollider.ColliderEnabled = ColliderEnabled;
        }

        public void Draw(WallInfo plane, params Vector3[] positions)
        {
            if (plane != default(WallInfo))
            {
                Plane = plane;
                gameObject.transform.rotation = plane.gameObject.transform.rotation;
            }

            _center = (positions.ElementAtOrDefault(0) == default(Vector3)) ? _center : positions[0];
            _radiusEnd = (positions.ElementAtOrDefault(1) == default(Vector3)) ? _radiusEnd : positions[1];
            _radius = Vector3.Distance(_radiusEnd, _center);
        }

        public void OnHoverAction(Action<GameObject> action)
        {
            action(gameObject);
        }

        public void OnHoverEnter()
        {
            _circleRenderer.material.color = ColorFocused;
        }

        public void OnHoverExit()
        {
            _circleRenderer.material.color = ColorNormal;
        }
    }
}
