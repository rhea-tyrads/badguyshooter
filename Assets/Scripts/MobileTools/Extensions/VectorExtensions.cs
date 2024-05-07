using UnityEngine;

namespace MobileTools.Extensions
{
    public static class VectorExtensions
    {

        public static float GetAngle(Vector2 dir)
        {
            dir = dir.normalized;
            var n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (n < 0) n += 360f;
            return n;
        }

        public static Vector2 GetVector(float angle)
        {
            var angleRad = angle * (Mathf.PI / 180f);
            return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;
        }
        public static Vector3 GetVector3(float angle)
        {
            var angleRad = angle * (Mathf.PI / 180f);
            return new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad)).normalized;
        }
    }
}