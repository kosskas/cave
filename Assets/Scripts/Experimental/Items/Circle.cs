using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Experimental.Utils;
using UnityEngine;

namespace Assets.Scripts.Experimental.Items
{
    public class Circle : MonoBehaviour, IDrawable, IRaycastable, IAnalyzable, IColorable
    {
        private Color _color = ReconstructionInfo.NORMAL;

        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                _circleRenderer.material.SetColor("_color", value);
            }
        }

        public float Width = ReconstructionInfo.CIRCLE_2D_WIDTH;

        private float ColliderWidth => Width * 4;

        private static readonly int PositionsCount = 60;

        private Vector3 _center;

        public Vector3 StartPosition => _center;

        private float _radius;

        private Vector3 _radiusEnd;

        public Vector3 EndPosition => _radiusEnd;

        public bool ColliderEnabled { get; set; } = true;

        public WallInfo Plane { get; private set; }

        private LineRenderer _circleRenderer;

        private RingCollider _ringCollider;


        void Awake()
        {
            _circleRenderer = gameObject.AddComponent<LineRenderer>();
            _circleRenderer.material = new Material(Shader.Find("Unlit/Unlit_line"));
            _circleRenderer.material.SetColor("_color", _color);
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
            _ringCollider.Width = ColliderWidth;
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
            if (_circleRenderer != null)
                _circleRenderer.material.SetColor("_color", ReconstructionInfo.FOCUSED);
        }

        public void OnHoverExit()
        {
            if (_circleRenderer != null)
                _circleRenderer.material.SetColor("_color", _color);
        }

        // IAnalyzable interface
        public List<Vector3> FindCrossingPoints(IAnalyzable obj)
        {
            Line crossLineObj = null;
            Circle crossCircleObj = null;
            if (obj is Line)
            {
                crossLineObj = (Line)obj;
                Vector3 A = crossLineObj.StartPosition;
                Vector3 B = crossLineObj.EndPosition;
                Vector3 S = this.StartPosition;
                float r = this._radius;

                List<Vector3> intersections = DescriptiveMathLib.FindLCIntersections(A, B, S, r)
                    .Where(p => DescriptiveMathLib.IsPointOnSegment(p, A, B))
                    .ToList();

                return intersections;

            }
            if (obj is Circle)
            {
                crossCircleObj = (Circle)obj;
                Vector3 S1 = this.StartPosition;
                Vector3 A1 = this.EndPosition;
                Vector3 S2 = crossCircleObj.StartPosition;
                Vector3 B2 = crossCircleObj.EndPosition;

                return DescriptiveMathLib.FindCCIntersections(S1, A1, S2, B2);
            }
            return null;
        }
        public IAnalyzable GetElement()
        {
            return this;
        }
    }
}
