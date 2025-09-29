using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Experimental.Items
{
    public class Axis : MonoBehaviour, IDrawable, IRaycastable, IColorable
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
                _meshRenderer.material.color = value;
            }
        }

        public float Width { get; set; } = ReconstructionInfo.LINE_2D_WIDTH;

        private float ColliderWidth => Width * 4;

        public Vector3 From { get; private set; }

        public Vector3 To { get; private set; }

        private GameObject _cylinder;

        private MeshRenderer _meshRenderer;

        private BoxCollider _boxCollider;


        void Awake()
        {
            _cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _cylinder.transform.SetParent(gameObject.transform, worldPositionStays: false);

            _meshRenderer = _cylinder.GetComponent<MeshRenderer>();
            _meshRenderer.material = new Material(Shader.Find("Unlit/Color"))
            {
                color = _color
            };
            _meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

            Destroy(_cylinder.GetComponent<CapsuleCollider>());

            _boxCollider = gameObject.AddComponent<BoxCollider>();
            _boxCollider.isTrigger = true;
        }

        void Update()
        {
            // Jeśli From/To nieustawione, nic nie rób
            Vector3 dir = To - From;
            float len = dir.magnitude;

            // włącz/wyłącz collider dla zdegenerowanej długości
            bool valid = len >= 1e-6f;
            if (_boxCollider != null) _boxCollider.enabled = valid;
            if (!valid) return;

            // pivot w środku odcinka
            Vector3 mid = (From + To) * 0.5f;
            transform.position = mid;

            // Obrót: z osi Y na kierunek odcinka
            transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);

            // Skalowanie cylindra (lokalne):
            // Cylinder ma wysokość 2 → skala.y = L/2
            // Grubość = Width (średnica)
            _cylinder.transform.localPosition = Vector3.zero;
            _cylinder.transform.localRotation = Quaternion.identity;
            _cylinder.transform.localScale = new Vector3(Width, len * 0.5f, Width);

            // BoxCollider pokrywający cylinder:
            // - wysokość (oś Y) = len
            // - grubość (x, z) = ColliderWidth
            _boxCollider.size = new Vector3(ColliderWidth, len, ColliderWidth);
            _boxCollider.center = Vector3.zero; // pivot w środku, więc zero
        }


        // IDRAWABLE interface

        public WallInfo Plane { get; private set; }

        public void Draw(WallInfo plane, params Vector3[] positions)
        {
            From = positions[0];
            To = positions[1];

            Plane = plane;
        }


        // IRAYCASTABLE interface

        public void OnHoverEnter()
        {
            _meshRenderer.material.color = ReconstructionInfo.FOCUSED;
        }

        public void OnHoverAction(Action<GameObject> action)
        {
            action(gameObject);
        }

        public void OnHoverExit()
        {
            _meshRenderer.material.color = _color;
        }
    }
}
