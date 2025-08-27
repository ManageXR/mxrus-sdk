using System;

using UnityEngine;

namespace MXRUS.SDK {
    internal static class GizmosX {
        public static void DrawConcentricCirclesXZ(Vector3 center, float radius, int perimeterSegments, int count) {
            if (count == 0)
                throw new ArgumentException("Cannot be 0", nameof(count));

            float distanceStep = radius / count;

            for (int i = 1; i <= count; i++) {
                DrawCircleXZ(center, distanceStep * i, perimeterSegments);
            }
        }

        public static void DrawCircleXZ(Vector3 center, float radius, int perimeterSegments) {
            float angleStep = 360f / perimeterSegments;
            Vector3 prevPoint = center + new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)) * radius;

            for (int i = 1; i <= perimeterSegments; i++) {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }

        public static void DrawSectorXZ(Vector3 center, Vector3 direction, float angle, int arcSegments, float radius) {
            float dirAngle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            float halfAngle = angle * 0.5f;
            float angleStep = angle / arcSegments;

            // First spoke
            float startAngle = dirAngle - halfAngle;
            Vector3 prevPoint = center + new Vector3(Mathf.Cos(startAngle * Mathf.Deg2Rad), 0, Mathf.Sin(startAngle * Mathf.Deg2Rad)) * radius;
            Gizmos.DrawLine(center, prevPoint);

            // Arc edges and remaining spokes
            for (int i = 1; i <= arcSegments; i++) {
                float currentAngle = startAngle + angleStep * i;
                Vector3 nextPoint = center + new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad), 0, Mathf.Sin(currentAngle * Mathf.Deg2Rad)) * radius;

                Gizmos.DrawLine(prevPoint, nextPoint);
                Gizmos.DrawLine(center, nextPoint);

                prevPoint = nextPoint;
            }
        }
    }
}
