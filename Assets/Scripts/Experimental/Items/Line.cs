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

        private float ColliderWidth => Width * 4;

        public Vector3 StartPosition { get; private set; }

        public Vector3 EndPosition { get; private set; }

        public bool ColliderEnabled { get; set; } = true;

        private LineRenderer _lineRenderer;

        private IndexedLabel _labelComponent;

        private BoxCollider _boxCollider;

        private MeshBuilder _mc;

        private MeshBuilder Mc
        {
            get
            {
                if (_mc == null)
                    _mc = (MeshBuilder)FindObjectOfType(typeof(MeshBuilder));

                return _mc;
            }
        }

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
            _lineRenderer.SetPositions(new[] { StartPosition, EndPosition });

            Vector3 newPosition = StartPosition;
            Quaternion newRotation = Quaternion.LookRotation((EndPosition - StartPosition).normalized, gameObject.transform.up) * Quaternion.Euler(0, -90, 0);
            Vector3 newColliderSize = new Vector3(Vector3.Distance(StartPosition, EndPosition), ColliderWidth, ColliderWidth);
            Vector3 newColliderCenter = new Vector3(Vector3.Distance(StartPosition, EndPosition) * 0.5f, 0, 0);

            gameObject.transform.position = newPosition;
            gameObject.transform.rotation = newRotation;
            _boxCollider.size = newColliderSize;
            _boxCollider.center = newColliderCenter;
            _boxCollider.enabled = ColliderEnabled;

            if (_isBoundToPoints)
                WatchBinding();
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

        public void BindPoints(ExPoint startPoint, string startPointLabel, ExPoint endPoint, string endPointLabel)
        {
            if (_isBoundToPoints)
                return;

            _startPoint = startPoint;
            _endPoint = endPoint;

            _startPointLabel = startPointLabel;
            _endPointLabel = endPointLabel;

            AddEdgeProjection();

            _isBoundToPoints = true;
        }

        public List<string> GetLabelsOfBoundPoints()
        {
            return (_isBoundToPoints)
                ? new List<string> { _startPointLabel, _endPointLabel }
                : new List<string>();
        }

        private void WatchBinding()
        {
            if (
                _startPoint != null &&
                _endPoint != null &&
                _startPoint.Labels.Contains(_startPointLabel) &&
                _endPoint.Labels.Contains(_endPointLabel)
                )
                return;

            RemoveEdgeProjection();
            Destroy(gameObject);
        }

        private void RemoveEdgeProjection()
        {
            Mc?.RemoveEdgeProjection(_startPointLabel, _endPointLabel);
        }

        private void AddEdgeProjection()
        {
            Mc.AddEdgeProjection(_startPointLabel, _endPointLabel);
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

            StartPosition = (positions.ElementAtOrDefault(0) == default(Vector3)) ? StartPosition : positions[0];
            EndPosition = (positions.ElementAtOrDefault(1) == default(Vector3)) ? EndPosition : positions[1];
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

        public void AddLabel(string labelText)
        {
            AddLabel();
            SetToText(labelText);
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

        private void SetToText(string text)
        {
            if (_labelComponent == null)
                return;

            _labelTexts.NextWhile(current => current.ToString() != text);
        }

    }
}
