using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Experimental.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Experimental.Items
{
    public class ExPoint : MonoBehaviour, IDrawable, IRaycastable, ILabelable
    {
        private Color ColorNormal = ReconstructionInfo.NORMAL;

        private Color ColorFocused = ReconstructionInfo.FOCUSED;

        private static readonly float Size = 0.025f;

        private static readonly float ColliderSize = Size * 4;

        public Vector3 Position { get; private set; }

        private GameObject _pointObject;

        private Renderer _pointRenderer;

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

        void Awake()
        {
            _pointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _pointObject.transform.SetParent(gameObject.transform);
            _pointObject.transform.localScale = new Vector3(Size, Size, Size);

            _pointRenderer = _pointObject.GetComponent<Renderer>();
            _pointRenderer.material = new Material(Shader.Find("Unlit/Color"))
            {
                color = ColorNormal
            };
            _pointRenderer.shadowCastingMode = ShadowCastingMode.Off;

            Destroy(_pointObject.GetComponent<SphereCollider>());

            _boxCollider = gameObject.AddComponent<BoxCollider>();
            _boxCollider.size = new Vector3(ColliderSize, ColliderSize, ColliderSize);
            _boxCollider.center = Vector3.zero;
            _boxCollider.isTrigger = true;

            _mc = (MeshBuilder)FindObjectOfType(typeof(MeshBuilder));
        }

        void Update()
        {
            gameObject.transform.position = Position;
            _pointObject.transform.position = Position;
        }

        void OnDestroy()
        {
            Labels?.ForEach(label =>
            {
                Mc?.RemovePointProjection(Plane, label);
            });
        }

        private GameObject AddProjectionLine(string labelText)
        {
            var projectionLineObj = new GameObject(labelText);
            projectionLineObj.transform.SetParent(gameObject.transform);
            projectionLineObj.transform.SetPositionAndRotation(gameObject.transform.position, gameObject.transform.rotation);

            projectionLineObj
                .AddComponent<LineSegment>()
                .SetStyle(ReconstructionInfo.PROJECTION_LINE_COLOR, ReconstructionInfo.PROJECTION_LINE_WIDTH);

            return projectionLineObj;
        }

        private void RemoveProjectionLine(string labelText)
        {
            GameObject projectionLineObjToRemove = null;

            foreach (Transform child in gameObject.transform)
            {
                if (child.name == labelText)
                    projectionLineObjToRemove = child.gameObject;
            }

            if (projectionLineObjToRemove != null)
                Destroy(projectionLineObjToRemove);
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

            Position = (positions.ElementAtOrDefault(0) == default(Vector3)) ? Position : positions[0];
            gameObject.transform.position = Position;
        }


        // IRAYCASTABLE interface

        public void OnHoverAction(Action<GameObject> action)
        {
            action(gameObject);
        }

        public void OnHoverEnter()
        {
            _pointRenderer.material.color = ColorFocused;

            if (_labelComponent != null)
                _labelComponent.FocusedLabelColor = ColorFocused;
        }

        public void OnHoverExit()
        {
            _pointRenderer.material.color = ColorNormal;

            if (_labelComponent != null)
                _labelComponent.FocusedLabelColor = ColorNormal;
        }


        // ILABELABLE interface

        private const char DefaultLabelText = ' ';
        private const string LabelTexts = "ABCDEFGHIJKLMNOPRQSTUVWXYZ123456789";
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
            
            _labelComponent.AddLabel(DefaultLabelText.ToString(), new string('\'', Plane.number), "");
            
            NextText();

            if (FocusedLabel.Equals(DefaultLabelText.ToString()))
                RemoveFocusedLabel();
        }

        public void AddLabel(string labelText)
        {
            AddLabel();
            SetToText(labelText);
        }

        public void RemoveFocusedLabel()
        {
            if (_labelComponent == null)
                return;

            Mc.RemovePointProjection(Plane, FocusedLabel);
            RemoveProjectionLine(FocusedLabel);
            _labelComponent.RemoveFocusedLabel();
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

            var found = _labelTexts.NextWhile(current => Mc.CheckIfAlreadyExist(Plane, current.ToString()) || current == DefaultLabelText);

            if (found)
                UpdateText();
        }

        public void PrevText()
        {
            if (_labelComponent == null)
                return;

            var found = _labelTexts.PreviousWhile(current => Mc.CheckIfAlreadyExist(Plane, current.ToString()) || current == DefaultLabelText);

            if (found)
                UpdateText();
        }

        private void SetToText(string text)
        {
            if (_labelComponent == null)
                return;

            var found = _labelTexts.NextWhile(current => Mc.CheckIfAlreadyExist(Plane, current.ToString()) || current.ToString() != text);

            if (found)
                UpdateText();
        }

        private void UpdateText()
        {
            var oldLabelText = FocusedLabel;
            var newLabelText = _labelTexts.Current.ToString();

            Mc.RemovePointProjection(Plane, oldLabelText);
            RemoveProjectionLine(oldLabelText);

            FocusedLabel = newLabelText;

            if (_labelTexts.Current.Equals(DefaultLabelText))
                return;

            var projectionObj = AddProjectionLine(newLabelText);
            Mc.AddPointProjection(Plane, newLabelText, this.gameObject);
        }

        public void SetLabelColor(Color textColor)
        {
            ColorNormal = textColor;
            _pointRenderer.material.color = textColor;

            if (_labelComponent != null)
                _labelComponent.FocusedLabelColor = textColor;
        }
    }
}
