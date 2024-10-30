using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Experimental.Items
{
    public class Point : MonoBehaviour, IDrawable, IRaycastable, ILabelable
    {
        private static readonly Color ColorNormal = Color.black;

        private static readonly Color ColorFocused = Color.red;

        private static readonly float Size = 0.025f;

        private static readonly char[] Labels = " ABCDEFGHIJKLMNOPRQSTUVWXYZ".ToCharArray();
        private int _labelIdx = 0;

        public Vector3 Position { get; private set; }

        public WallInfo Plane { get; private set; }

        private GameObject _pointObject;

        private Renderer _pointRenderer;

        private BoxCollider _boxCollider;
        private MeshBuilder _mc;

        void Start()
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
            _boxCollider.size = new Vector3(Size, Size, Size);
            _boxCollider.center = Vector3.zero;
            _boxCollider.isTrigger = true;

            _mc = (MeshBuilder)FindObjectOfType(typeof(MeshBuilder));
        }

        void Update()
        {
            gameObject.transform.position = Position;
        }

        public void Draw(WallInfo plane, params Vector3[] positions)
        {
            if (plane != default(WallInfo))
            {
                Plane = plane;
                gameObject.transform.rotation = plane.gameObject.transform.rotation;
            }

            Position = (positions.ElementAtOrDefault(0) == default(Vector3)) ? Position : positions[0];
        }

        public void OnHoverAction(Action<GameObject> action)
        {
            action(gameObject);
        }

        public void OnHoverEnter()
        {
            _pointRenderer.material.color = ColorFocused;
        }

        public void OnHoverExit()
        {
            _pointRenderer.material.color = ColorNormal;
        }

        public void NextLabel()
        {
            var label = gameObject.GetComponent<IndexedLabel>();
            if (label == null)
            {
                return;
            }

            while (true)
            {
                _labelIdx = (_labelIdx + 1 == Labels.Length) ? 0 : _labelIdx + 1;
                var newText = Labels[_labelIdx].ToString();

                if (!_mc.CheckIfAlreadyExist(Plane, newText))
                {
                    _mc.RemovePointProjection(Plane, label.Text);
                    label.Text = newText;
                    if (newText != " ")
                    {
                        _mc.AddPointProjection(Plane, newText, gameObject);
                    }

                    break;
                }
            }
        }

        public void PrevLabel()
        {
            var label = gameObject.GetComponent<IndexedLabel>();
            if (label == null)
            {
                return;
            }

            while (true)
            {
                _labelIdx = (_labelIdx == 0) ? Labels.Length - 1 : _labelIdx - 1;
                var newText = Labels[_labelIdx].ToString();

                if (!_mc.CheckIfAlreadyExist(Plane, newText))
                {
                    _mc.RemovePointProjection(Plane, label.Text);
                    label.Text = newText;
                    if (newText != " ")
                    {
                        _mc.AddPointProjection(Plane, newText, gameObject);
                    }

                    break;
                }
            }
        }

    }
}
