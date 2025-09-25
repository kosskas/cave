using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Experimental.Items
{
    public class Axis : MonoBehaviour, IDrawable
    {
        private readonly Color _COLOR = Color.black;

        public float Width { get; set; } = 0.005f;

        public WallInfo Plane { get; private set; }

        public Vector3 From { get; private set; }

        public Vector3 To { get; private set; }

        private GameObject _cylinder;

        private MeshRenderer _meshRenderer;


        void Awake()
        {
            _cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _cylinder.transform.SetParent(transform, worldPositionStays: false);

            _meshRenderer = _cylinder.GetComponent<MeshRenderer>();
            if (_meshRenderer != null)
            {
                var mat = new Material(Shader.Find("Unlit/Color"))
                {
                    color = _COLOR
                };
                _meshRenderer.material = mat;
                _meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        void Update()
        {
            // Jeśli From/To nieustawione, nic nie rób
            Vector3 dir = To - From;
            float len = dir.magnitude;
            if (len < 1e-6f) return;

            // Pozycja = środek odcinka
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
        }


        public void Draw(WallInfo plane, params Vector3[] positions)
        {
            From = positions[0];
            To = positions[1];

            Plane = plane;
        }
    }
}
