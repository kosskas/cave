using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Experimental.Utils
{
    public class RingCollider : MonoBehaviour
    {
        private const int Segments = 100;

        private List<BoxCollider> colliders = new List<BoxCollider>();

        public float Width { get; set; } = 0.005f;

        public Vector3 Center { get; set; } = Vector3.zero;

        public float Radius { get; set; } = 0f;

        public bool ColliderEnabled { get; set; } = true;


        void Start()
        {
            var boxColliders = new GameObject("boxColliders");
            boxColliders.transform.SetParent(transform);
            boxColliders.transform.position = transform.position;
            boxColliders.transform.rotation = transform.rotation;

            for (var ithSegment = 0; ithSegment < Segments; ithSegment++)
            {
                var boxColliderBox = new GameObject($"collider_{ithSegment}");
                boxColliderBox.transform.SetParent(boxColliders.transform);
                boxColliderBox.transform.position = transform.position;
                boxColliderBox.transform.rotation = transform.rotation;

                var boxCollider = boxColliderBox.AddComponent<BoxCollider>();
                boxCollider.center = Vector3.zero;
                boxCollider.isTrigger = true;

                boxColliderBox.AddComponent<RaycastableComponent>();

                colliders.Add(boxCollider);
            }

            Rebuild();
        }

        void Update()
        {
            Rebuild();
        }

        private void Rebuild()
        {
            for (var ithSegment = 0; ithSegment < Segments; ithSegment++)
            {
                var startRadian = 2 * Mathf.PI * ((float)ithSegment / Segments);
                var startX = Mathf.Cos(startRadian) * Radius;
                var startY = Mathf.Sin(startRadian) * Radius;
                var startPos = Center + gameObject.transform.forward * startX + gameObject.transform.up * startY;

                var stopRadian = 2 * Mathf.PI * ((float)(ithSegment + 1) / Segments);
                var stopX = Mathf.Cos(stopRadian) * Radius;
                var stopY = Mathf.Sin(stopRadian) * Radius;
                var stopPos = Center + gameObject.transform.forward * stopX + gameObject.transform.up * stopY;

                var centerPos = startPos + (stopPos - startPos) * 0.5f;
                var size = new Vector3(Vector3.Distance(startPos, stopPos), Width * 3, Width * 3);

                colliders[ithSegment].gameObject.transform.position = centerPos;
                colliders[ithSegment].gameObject.transform.rotation = Quaternion.LookRotation((startPos - stopPos).normalized, gameObject.transform.up) * Quaternion.Euler(0, -90, 0);
                colliders[ithSegment].size = size;
                colliders[ithSegment].enabled = ColliderEnabled;
            }
        }


        private class RaycastableComponent : MonoBehaviour, IRaycastable
        {
            public void OnHoverAction(Action<GameObject> action)
            {
                gameObject.transform.parent?.parent?.GetComponent<IRaycastable>()?.OnHoverAction(action);
            }

            public void OnHoverEnter()
            {
                gameObject.transform.parent?.parent?.GetComponent<IRaycastable>()?.OnHoverEnter();
            }

            public void OnHoverExit()
            {
                gameObject.transform.parent?.parent?.GetComponent<IRaycastable>()?.OnHoverExit();
            }
        }
    }
}
