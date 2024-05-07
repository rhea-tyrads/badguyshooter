﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class DistanceToggle
    {
        static List<IDistanceToggle> distanceToggles = new List<IDistanceToggle>();
        static int distanceTogglesCount;

        static bool isActive;
        public static bool IsActive => isActive;

        static Vector3 tempDistance;
        static float tempDistanceMagnitude;
        static bool tempIsVisible;

        static Transform playerTransform;

        static Coroutine updateCoroutine;

        public static void Initialise(Transform transform)
        {
            playerTransform = transform;

            distanceToggles = new List<IDistanceToggle>();
            distanceTogglesCount = 0;

            isActive = true;

            // Activate update coroutine
            updateCoroutine = Tween.InvokeCoroutine(UpdateCoroutine());
        }

        static IEnumerator UpdateCoroutine()
        {
            while(true)
            {
                if (isActive)
                {
                    for (var i = 0; i < distanceTogglesCount; i++)
                    {
                        if (!distanceToggles[i].IsShowing)
                            continue;

                        tempIsVisible = distanceToggles[i].IsVisible;

                        tempDistance = playerTransform.position - distanceToggles[i].DistancePointPosition;
                        tempDistance.y = 0;

                        tempDistanceMagnitude = tempDistance.magnitude;

                        if (!tempIsVisible && tempDistanceMagnitude <= distanceToggles[i].ShowingDistance)
                        {
                            distanceToggles[i].PlayerEnteredZone();
                        }
                        else if (tempIsVisible && tempDistanceMagnitude > distanceToggles[i].ShowingDistance)
                        {
                            distanceToggles[i].PlayerLeavedZone();
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
            distanceToggles.Add(distanceToggle);
            distanceTogglesCount++;
        }

        public static void RemoveObject(IDistanceToggle distanceToggle)
        {
            distanceToggles.Remove(distanceToggle);
            distanceTogglesCount--;
        }

        public static bool IsInRange(IDistanceToggle distanceToggle)
        {
            tempDistance = playerTransform.position - distanceToggle.DistancePointPosition;
            tempDistance.y = 0;

            tempDistanceMagnitude = tempDistance.magnitude;

            return tempDistanceMagnitude <= distanceToggle.ShowingDistance;
        }

        public static void Enable()
        {
            isActive = true;
        }

        public static void Disable()
        {
            isActive = false;
        }

        public static void Unload()
        {
            if (updateCoroutine != null)
                Tween.StopCustomCoroutine(updateCoroutine);

            isActive = false;
        }
    }
}