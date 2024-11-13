﻿using Assets.Scripts.Experimental.Utils;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Experimental.Items
{
    public class Line : MonoBehaviour, IDrawable, IRaycastable, ILabelable
    {
        private static readonly Color ColorNormal = Color.black;

        private static readonly Color ColorFocused = Color.red;

        public float Width { get; set; } = 0.005f;

        private CircularIterator<char> _labels;

        private Vector3 _from;

        private Vector3 _to;

        public bool ColliderEnabled { get; set; } = true;

        public WallInfo Plane { get; private set; }

        public string Label { get; private set; }

        private LineRenderer _lineRenderer;

        private BoxCollider _boxCollider;
        private MeshBuilder _mc;

        private ExPoint _startPoint;
        private ExPoint _endPoint;
        private string _startPointLabel;
        private string _endPointLabel;
        private bool _isBoundToPoints = false;

        void Start()
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.material = new Material(Shader.Find("Unlit/Color"))
            {
                color = ColorNormal
            };
            _lineRenderer.positionCount = 2;

            _lineRenderer.startWidth = Width;
            _lineRenderer.endWidth = Width;

            _boxCollider = gameObject.AddComponent<BoxCollider>();
            _boxCollider.isTrigger = true;

            _mc = (MeshBuilder)FindObjectOfType(typeof(MeshBuilder));

            _labels = new CircularIterator<char>("abcdefghijklmnoprqstuvwxyz".ToList(), ' ');
        }

        void Update()
        {
            _lineRenderer.startWidth = Width;
            _lineRenderer.endWidth = Width;
            _lineRenderer.SetPositions(new[] { _from, _to });

            Vector3 newPosition = _from;
            Quaternion newRotation = Quaternion.LookRotation((_to - _from).normalized, gameObject.transform.up) * Quaternion.Euler(0, -90, 0);
            Vector3 newColliderSize = new Vector3(Vector3.Distance(_from, _to), Width * 3, Width * 3);
            Vector3 newColliderCenter = new Vector3(Vector3.Distance(_from, _to) * 0.5f, 0, 0);

            gameObject.transform.position = newPosition;
            gameObject.transform.rotation = newRotation;
            _boxCollider.size = newColliderSize;
            _boxCollider.center = newColliderCenter;
            _boxCollider.enabled = ColliderEnabled;

            if (_isBoundToPoints)
                RefreshEdgeProjection();
        }

        void OnDestroy()
        {
            if (_isBoundToPoints)
                RemoveEdgeProjection();
        }

        public void Draw(WallInfo plane, params Vector3[] positions)
        {
            if (plane != default(WallInfo))
            {
                Plane = plane;
                gameObject.transform.rotation = plane.gameObject.transform.rotation;
            }

            _from = (positions.ElementAtOrDefault(0) == default(Vector3)) ? _from : positions[0];
            _to = (positions.ElementAtOrDefault(1) == default(Vector3)) ? _to : positions[1];
        }

        public void OnHoverEnter()
        {
            _lineRenderer.material.color = ColorFocused;
        }

        public void OnHoverExit()
        {
            _lineRenderer.material.color = ColorNormal;
        }

        public void OnHoverAction(Action<GameObject> action)
        {
            action(gameObject);
        }

        public void NextLabel()
        {
            var labelComponent = gameObject.GetComponent<IndexedLabel>();
            if (labelComponent == null)
            {
                return;
            }

            _labels.Next();

            labelComponent.Text = _labels.Current.ToString();
            Label = _labels.Current.ToString();
        }

        public void PrevLabel()
        {
            var labelComponent = gameObject.GetComponent<IndexedLabel>();
            if (labelComponent == null)
            {
                return;
            }

            _labels.Previous();

            labelComponent.Text = _labels.Current.ToString();
            Label = _labels.Current.ToString();
        }

        public void BindPoints(ExPoint startPoint, ExPoint endPoint)
        {
            if (_isBoundToPoints)
                return;

            _startPoint = startPoint;
            _endPoint = endPoint;

            _startPointLabel = startPoint.Label;
            _endPointLabel = endPoint.Label;

            AddEdgeProjection();

            _isBoundToPoints = true;
        }

        private void RefreshEdgeProjection()
        {
            if (_startPoint == null || _endPoint == null)
            {
                RemoveEdgeProjection();
                _isBoundToPoints = false;
                return;
            }

            if (_startPointLabel == _startPoint.Label && _endPointLabel == _endPoint.Label)
                return;

            RemoveEdgeProjection();

            _startPointLabel = _startPoint.Label;
            _endPointLabel = _endPoint.Label;

            AddEdgeProjection();
        }

        private void RemoveEdgeProjection()
        {
            _mc.RemoveEdgeProjection(_startPointLabel, _endPointLabel);
        }

        private void AddEdgeProjection()
        {
            _mc.AddEdgeProjection(_startPointLabel, _endPointLabel);
        }
    }
}
