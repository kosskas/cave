﻿using System;
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

        private static readonly float Width = 0.005f;

        private static readonly char[] Labels = " abcdefghijklmnoprqstuvwxyz".ToCharArray();
        private int _labelIdx = 0;

        private Vector3 _from;

        private Vector3 _to;

        private LineRenderer _lineRenderer;

        private BoxCollider _boxCollider;


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
        }

        void Update()
        {
            _lineRenderer.SetPositions(new[] { _from, _to });

            Vector3 newPosition = _from;
            Quaternion newRotation = Quaternion.LookRotation((_to - _from).normalized, Vector3.up) * Quaternion.Euler(0, -90, 0);
            Vector3 newColliderSize = new Vector3(Vector3.Distance(_from, _to), Width * 3, Width * 3);
            Vector3 newColliderCenter = new Vector3(Vector3.Distance(_from, _to) * 0.5f, 0, 0);

            gameObject.transform.position = newPosition;
            gameObject.transform.rotation = newRotation;
            _boxCollider.size = newColliderSize;
            _boxCollider.center = newColliderCenter;
        }

        public void Draw(params Vector3[] positions)
        {
            _from = (positions.ElementAtOrDefault(0) == default(Vector3)) ? _from : positions[0];
            _to = (positions.ElementAtOrDefault(1) == default(Vector3)) ? _to : positions[1];
        }

        public void Erase()
        {
            throw new NotImplementedException();
        }

        public void OnHoverEnter()
        {
            _lineRenderer.material.color = Color.red;
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
            _labelIdx = (_labelIdx + 1 == Labels.Length) ? 0 : _labelIdx + 1;

            var label = gameObject.GetComponent<IndexedLabel>();
            if (label != null)
            {
                label.Text = Labels[_labelIdx].ToString();
            }
        }

        public void PrevLabel()
        {
            _labelIdx = (_labelIdx == 0) ? Labels.Length - 1 : _labelIdx - 1;

            var label = gameObject.GetComponent<IndexedLabel>();
            if (label != null)
            {
                label.Text = Labels[_labelIdx].ToString();
            }
        }
    }
}
