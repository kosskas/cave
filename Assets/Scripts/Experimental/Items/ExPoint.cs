using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Experimental.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Experimental.Items
{
    public class ExPoint : MonoBehaviour, IDrawable, IRaycastable, ILabelable
    {
        private static readonly Color ColorNormal = Color.black;

        private static readonly Color ColorFocused = Color.red;

        private static readonly float Size = 0.025f;

        private CircularIterator<char> _labels;

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

            _labels = new CircularIterator<char>("ABCDEFGHIJKLMNOPRQSTUVWXYZ123456789".ToList(), ' ');
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
            var labelComponent = gameObject.GetComponent<IndexedLabel>();
            if (labelComponent == null)
            {
                return;
            }

            _labels.NextWhile(current => _mc.CheckIfAlreadyExist(Plane, current.ToString()));

            _mc.RemovePointProjection(Plane, labelComponent.Text);

            labelComponent.Text = _labels.Current.ToString();

            if (!_labels.CurrentIsDefault)
            {
                AddProjectionLine();
                _mc.AddPointProjection(Plane, _labels.Current.ToString(), gameObject);
            }
            else
            {
                RemoveProjectionLine();
            }
        }

        public void PrevLabel()
        {
            var labelComponent = gameObject.GetComponent<IndexedLabel>();
            if (labelComponent == null)
            {
                return;
            }

            _labels.PreviousWhile(current => _mc.CheckIfAlreadyExist(Plane, current.ToString()));

            _mc.RemovePointProjection(Plane, labelComponent.Text);

            labelComponent.Text = _labels.Current.ToString();

            if (!_labels.CurrentIsDefault)
            {
                AddProjectionLine();
                _mc.AddPointProjection(Plane, _labels.Current.ToString(), gameObject);
            }
            else
            {
                RemoveProjectionLine();
            }
        }

        private void AddProjectionLine()
        {
            var ls = gameObject.GetComponent<LineSegment>();
            if (ls == null)
            {
                gameObject
                    .AddComponent<LineSegment>()
                    .SetStyle(ReconstructionInfo.PROJECTION_LINE_COLOR, ReconstructionInfo.PROJECTION_LINE_WIDTH);
            }
        }

        private void RemoveProjectionLine()
        {
            var ls = gameObject.GetComponent<LineSegment>();
            if (ls != null)
            {
                Destroy(gameObject.GetComponent<LineSegment>());
            }
        }
    }
}
