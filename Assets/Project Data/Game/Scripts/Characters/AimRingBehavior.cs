using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class AimRingBehavior : MonoBehaviour
    {
        [SerializeField] float width;
        [SerializeField] int detalisation;
        [SerializeField] float stripeLength;
        [SerializeField] float gapLength;

        [Space(5f)]
        [SerializeField] float rotationSpeed;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        Mesh mesh;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        Transform followTransform;

        float radius;

        void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();

            mesh = new Mesh();
            meshFilter.mesh = mesh;
        }

        public void Init(Transform followTransform)
        {
            transform.SetParent(null);
            this.followTransform = followTransform;
        }

        public void SetRadius(float radius)
        {
            if (radius == 0)
            {
                Debug.LogError("Aiming radius can't be 0!");
            }

            this.radius = Mathf.Clamp(radius, 1, float.MaxValue);
            GenerateMesh();
        }

        public void UpdatePosition()
        {
            transform.position = followTransform.position;
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
        }

        public void Show()
        {
            meshRenderer.enabled = true;
        }

        public void Hide()
        {
            meshRenderer.enabled = false;
        }

        void GenerateMesh()
        {
            mesh = new Mesh();
            mesh.name = "Generated Mesh";
            meshFilter.mesh = mesh;

            var stepAngle = 360f / detalisation;

            var stripeAngle = 180f * stripeLength / (Mathf.PI * radius);
            var stripeSectorsAmount = Mathf.FloorToInt(stripeAngle / stepAngle);

            var gapAngle = 180f * gapLength / (Mathf.PI * radius);
            var gapSectorsAmount = Mathf.FloorToInt(gapAngle / stepAngle);

            vertices.Clear();
            triangles.Clear();
            mesh.Clear();

            float currentAngle = 0;

            while (currentAngle < 360f)
            {
                for (var i = 0; i < stripeSectorsAmount && currentAngle < 360f; i++)
                {
                    vertices.Add(GetPoint(radius, Mathf.Deg2Rad * currentAngle, Vector3.zero));
                    vertices.Add(GetPoint(radius + width, Mathf.Deg2Rad * currentAngle, Vector3.zero));
                    vertices.Add(GetPoint(radius, Mathf.Deg2Rad * (currentAngle + stepAngle), Vector3.zero));

                    vertices.Add(GetPoint(radius + width, Mathf.Deg2Rad * currentAngle, Vector3.zero));
                    vertices.Add(GetPoint(radius + width, Mathf.Deg2Rad * (currentAngle + stepAngle), Vector3.zero));
                    vertices.Add(GetPoint(radius, Mathf.Deg2Rad * (currentAngle + stepAngle), Vector3.zero));

                    var trisCount = triangles.Count;

                    triangles.Add(trisCount + 2);
                    triangles.Add(trisCount + 1);
                    triangles.Add(trisCount);

                    triangles.Add(trisCount + 5);
                    triangles.Add(trisCount + 4);
                    triangles.Add(trisCount + 3);

                    currentAngle += stepAngle;
                }

                for (var i = 0; i < gapSectorsAmount && currentAngle < 360f; i++)
                {
                    currentAngle += stepAngle;
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
        }

        Vector3 GetPoint(float radius, float angle, Vector3 center)
        {
            return new Vector3(Mathf.Cos(angle) * radius + center.x, center.y, Mathf.Sin(angle) * radius + center.z);
        }

        public void OnPlayerDestroyed()
        {
            Destroy(gameObject);
        }
    }
}