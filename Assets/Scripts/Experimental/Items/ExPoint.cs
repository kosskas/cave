using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Experimental.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Experimental.Items
{
    public class ExPoint : MonoBehaviour, IDrawable, IRaycastable, ILabelable
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
                _pointRenderer.material.color = value;

                if (_labelComponent != null)
                    _labelComponent.FocusedLabelColor = value;
            }
        }

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
                color = _color
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
            _pointRenderer.material.color = ReconstructionInfo.FOCUSED; ;

            if (_labelComponent != null)
                _labelComponent.FocusedLabelColor = ReconstructionInfo.FOCUSED; ;
        }

        public void OnHoverExit()
        {
            _pointRenderer.material.color = _color;

            if (_labelComponent != null)
                _labelComponent.FocusedLabelColor = _color;
        }


        // ILABELABLE interface

        private const string DefaultLabelText = " ";

        private static readonly List<string> LabelTexts = " ABCDEFGHIJKLMNOPRQSTUVWXYZ123456789".Select(c => c.ToString())
            .Concat(new[] { "I","II","III","IV","V","VI","VII","VIII","IX","X" })
            .ToList();

        private readonly CircularIterator<string> _labelTexts = new CircularIterator<string>(LabelTexts);

        public bool EnabledLabels { get; set; } = false;

        public string FocusedLabel
        {
            get
            {
                return _labelComponent?.FocusedLabel?.Text
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
            
            _labelComponent.AddLabel(DefaultLabelText.ToString(), new string('\'', Plane.numberExp), "");
            
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

            _labelComponent.RemoveFocusedLabel();
            if (string.IsNullOrEmpty(FocusedLabel))
            {
                Mc.RemoveProjectionLine(this.gameObject);
            }
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
            _labelTexts.Begin();
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

            FocusedLabel = newLabelText;

            if (_labelTexts.Current.Equals(DefaultLabelText))
                return;

            Mc.AddPointProjection(Plane, newLabelText, this.gameObject);
        }

    }
}
