using UnityEngine;

namespace TopDownEngine.Common.Scripts.BoogieScripts
{
    public class MagicBeam : MonoBehaviour
    {
        public Vector3 offsetStart;
        public Vector3 offsetEnd;
        public GameObject beamStart;
        public GameObject beamEnd;
        public GameObject beam;
        public LineRenderer line;

        [Header("Adjustable Variables")]
        public float beamEndOffset = 1f;
        public float textureScrollSpeed = 8f;
        public float textureLengthScale = 3;
 
        public void Hide()
        {
            _isShow = false;
            beamStart.SetActive(false);
            beamEnd.SetActive(false);
            beam.SetActive(false);
          //  gameObject.SetActive(false);
        }

 

      public  bool _isShow;
        Transform _target;
 

        public void Show(Transform target)
        {
            _target = target;
            _isShow = true;
            beamStart.SetActive(true);
            beamEnd.SetActive(true);
            beam.SetActive(true);
        }

        void Update()
        {
            if (!_isShow) return;
            var dir = _target.position - transform.position;
            DrawBeam(transform.position, dir);
        }


        void DrawBeam(Vector3 start, Vector3 dir)
        {
            start += Vector3.up * offsetStart.y + dir.normalized * offsetStart.z;
            line.positionCount = 2;
            line.SetPosition(0, start );
            beamStart.transform.position = start;
            var end = _target.position + Vector3.up * offsetEnd.y + dir.normalized * offsetEnd.z;
            beamEnd.transform.position = end;
            line.SetPosition(1, end);

            beamStart.transform.LookAt(end);
            beamEnd.transform.LookAt(start);

            var distance = Vector3.Distance(start, end);
            line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
            line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
        }
    }
}