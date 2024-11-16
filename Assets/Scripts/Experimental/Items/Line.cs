using Assets.Scripts.Experimental.Utils;
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

        private Vector3 _from;

        private Vector3 _to;

        public bool ColliderEnabled { get; set; } = true;

        private LineRenderer _lineRenderer;

        private IndexedLabel _labelComponent;

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

        public void BindPoints(ExPoint startPoint, ExPoint endPoint)
        {
            if (_isBoundToPoints)
                return;

            _startPoint = startPoint;
            _endPoint = endPoint;

            _startPointLabel = startPoint.FocusedLabel;
            _endPointLabel = endPoint.FocusedLabel;

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

            //if (_startPointLabel == _startPoint.Label && _endPointLabel == _endPoint.Label)
            if (_startPoint.Labels.Contains(_startPointLabel) && _endPoint.Labels.Contains(_endPointLabel))
                return;

            RemoveEdgeProjection();
            _isBoundToPoints = false;

            //_startPointLabel = _startPoint.Label;
            //_endPointLabel = _endPoint.Label;

            //AddEdgeProjection();
        }

        private void RemoveEdgeProjection()
        {
            _mc.RemoveEdgeProjection(_startPointLabel, _endPointLabel);
        }

        private void AddEdgeProjection()
        {
            _mc.AddEdgeProjection(_startPointLabel, _endPointLabel);
        }


        // IDRAWABLE interface

        public WallInfo Plane { get; private set; }

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


        // IRAYCASTABLE interface

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


        // ILABELABLE interface

        private const char DefaultLabelText = ' ';
        private const string LabelTexts = "abcdefghijklmnoprqstuvwxyz";
        private readonly CircularIterator<char> _labelTexts = new CircularIterator<char>($"{DefaultLabelText}{LabelTexts}".ToList());

        public bool EnabledLabels { get; set; } = false;

        public string FocusedLabel
        {
            get
            {
                return _labelComponent?.FocusedLabel.Text
                       ?? string.Empty;
            }
            set
            {
                if (_labelComponent != null)
                    _labelComponent.FocusedLabel.Text = value;
            }
        }

        public List<string> Labels => _labelComponent?.Labels.Select(l => l.Text).ToList()
                                      ?? new List<string>();

        public void AddLabel()
        {
            if (!EnabledLabels)
                return;

            if (_labelComponent == null)
                _labelComponent = gameObject.AddComponent<IndexedLabel>();

            _labelComponent.AddLabel("", new string('\'', Plane.number), "");

            NextText();
        }

        public void RemoveFocusedLabel()
        {
            _labelComponent?.RemoveFocusedLabel();
        }

        public void NextLabel()
        {
            _labelComponent?.NextLabel();
        }

        public void PrevLabel()
        {
            _labelComponent?.PrevLabel();
        }

        public void NextText()
        {
            if (_labelComponent == null)
                return;

            _labelTexts.Next();

            FocusedLabel = _labelTexts.Current.ToString();
        }

        public void PrevText()
        {
            if (_labelComponent == null)
                return;

            _labelTexts.Previous();

            FocusedLabel = _labelTexts.Current.ToString();
        }

    }
}
