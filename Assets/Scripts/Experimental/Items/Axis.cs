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

            // Obrót z osi Y na kierunek
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, dir);

            // 1) Root wskazuje na From i ma obrót wzdłuż odcinka
            transform.SetPositionAndRotation(From, rot);

            // 2) Dziecko (cylinder + collider) ustawiamy lokalnie:
            //    - oś Y rodzica jest wzdłuż odcinka, więc połowa długości to przesunięcie do środka
            _cylinder.transform.localPosition = new Vector3(0f, len * 0.5f, 0f);
            _cylinder.transform.localRotation = Quaternion.identity; // światowy obrót = rot (dziedziczony z rodzica)

            // 3) Skala cylindra: wysokość modelu bazowego 2 -> y = L/2
            _cylinder.transform.localScale = new Vector3(Width, len * 0.5f, Width);

            // 4) BoxCollider na dziecku: środek w lokalnym (0,0,0) dziecka = środek odcinka w świecie
            _boxCollider.center = Vector3.zero;
            _boxCollider.size = new Vector3(ColliderWidth, len, ColliderWidth);
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
