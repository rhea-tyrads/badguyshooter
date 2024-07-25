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

        MeshFilter _meshFilter;
        MeshRenderer _meshRenderer;
        Mesh _mesh;

        List<Vector3> _vertices = new();
        List<int> _triangles = new();

        Transform _followTransform;

        float _radius;

        void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            _mesh = new Mesh();
            _meshFilter.mesh = _mesh;
        }

        public void Init(Transform followTransform)
        {
            transform.SetParent(null);
            this._followTransform = followTransform;
        }

        public void SetRadius(float radius)
        {
            if (radius == 0)
            {
                Debug.LogError("Aiming radius can't be 0!");
            }

            this._radius = Mathf.Clamp(radius, 1, float.MaxValue);
            GenerateMesh();
        }

        public void UpdatePosition()
        {
            transform.position = _followTransform.position;
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
        }

        public void Show()
        {
            _meshRenderer.enabled = true;
        }

        public void Hide()
        {
            _meshRenderer.enabled = false;
        }

        void GenerateMesh()
        {
            _mesh = new Mesh();
            _mesh.name = "Generated Mesh";
            _meshFilter.mesh = _mesh;

            var stepAngle = 360f / detalisation;

            var stripeAngle = 180f * stripeLength / (Mathf.PI * _radius);
            var stripeSectorsAmount = Mathf.FloorToInt(stripeAngle / stepAngle);

            var gapAngle = 180f * gapLength / (Mathf.PI * _radius);
            var gapSectorsAmount = Mathf.FloorToInt(gapAngle / stepAngle);

            _vertices.Clear();
            _triangles.Clear();
            _mesh.Clear();

            float currentAngle = 0;

            while (currentAngle < 360f)
            {
                for (var i = 0; i < stripeSectorsAmount && currentAngle < 360f; i++)
                {
                    _vertices.Add(GetPoint(_radius, Mathf.Deg2Rad * currentAngle, Vector3.zero));
                    _vertices.Add(GetPoint(_radius + width, Mathf.Deg2Rad * currentAngle, Vector3.zero));
                    _vertices.Add(GetPoint(_radius, Mathf.Deg2Rad * (currentAngle + stepAngle), Vector3.zero));

                    _vertices.Add(GetPoint(_radius + width, Mathf.Deg2Rad * currentAngle, Vector3.zero));
                    _vertices.Add(GetPoint(_radius + width, Mathf.Deg2Rad * (currentAngle + stepAngle), Vector3.zero));
                    _vertices.Add(GetPoint(_radius, Mathf.Deg2Rad * (currentAngle + stepAngle), Vector3.zero));

                    var trisCount = _triangles.Count;

                    _triangles.Add(trisCount + 2);
                    _triangles.Add(trisCount + 1);
                    _triangles.Add(trisCount);

                    _triangles.Add(trisCount + 5);
                    _triangles.Add(trisCount + 4);
                    _triangles.Add(trisCount + 3);

                    currentAngle += stepAngle;
                }

                for (var i = 0; i < gapSectorsAmount && currentAngle < 360f; i++)
                {
                    currentAngle += stepAngle;
                }
            }

            _mesh.vertices = _vertices.ToArray();
            _mesh.triangles = _triangles.ToArray();
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