using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class DistanceToggle
    {
        static List<IDistanceToggle> _distanceToggles = new();
        static int _distanceTogglesCount;

        static bool _isActive;
        public static bool IsActive => _isActive;

        static Vector3 _tempDistance;
        static float _tempDistanceMagnitude;
        static bool _tempIsVisible;

        static Transform _playerTransform;

        static Coroutine _updateCoroutine;

        public static void Initialise(Transform transform)
        {
            _playerTransform = transform;

            _distanceToggles = new List<IDistanceToggle>();
            _distanceTogglesCount = 0;

            _isActive = true;

            // Activate update coroutine
            _updateCoroutine = Tween.InvokeCoroutine(UpdateCoroutine());
        }

        static IEnumerator UpdateCoroutine()
        {
            while(true)
            {
                if (_isActive)
                {
                    for (int i = 0; i < _distanceTogglesCount; i++)
                    {
                        if (!_distanceToggles[i].IsShowing)
                            continue;

                        _tempIsVisible = _distanceToggles[i].IsVisible;

                        _tempDistance = _playerTransform.position - _distanceToggles[i].DistancePointPosition;
                        _tempDistance.y = 0;

                        _tempDistanceMagnitude = _tempDistance.magnitude;

                        if (!_tempIsVisible && _tempDistanceMagnitude <= _distanceToggles[i].ShowingDistance)
                        {
                            _distanceToggles[i].PlayerEnteredZone();
                        }
                        else if (_tempIsVisible && _tempDistanceMagnitude > _distanceToggles[i].ShowingDistance)
                        {
                            _distanceToggles[i].PlayerLeavedZone();
                        }
                    }
                }

                yield return null;
                yield return null;
                yield return null;
                yield return null;
                yield return null;
            }
        }

        public static void AddObject(IDistanceToggle distanceToggle)
        {
            _distanceToggles.Add(distanceToggle);
            _distanceTogglesCount++;
        }

        public static void RemoveObject(IDistanceToggle distanceToggle)
        {
            _distanceToggles.Remove(distanceToggle);
            _distanceTogglesCount--;
        }

        public static bool IsInRange(IDistanceToggle distanceToggle)
        {
            _tempDistance = _playerTransform.position - distanceToggle.DistancePointPosition;
            _tempDistance.y = 0;

            _tempDistanceMagnitude = _tempDistance.magnitude;

            return _tempDistanceMagnitude <= distanceToggle.ShowingDistance;
        }

        public static void Enable()
        {
            _isActive = true;
        }

        public static void Disable()
        {
            _isActive = false;
        }

        public static void Unload()
        {
            if (_updateCoroutine != null)
                Tween.StopCustomCoroutine(_updateCoroutine);

            _isActive = false;
        }
    }
}