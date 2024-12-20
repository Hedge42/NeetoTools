﻿using Neeto;
using UnityEngine;

namespace Neeto
{
    /// <summary>
    /// Stores data about collision-instances
    /// </summary>
    public struct CollisionInfo
    {
        /// <summary> the main component of the collider that had the OnCollision/OnTrigger method </summary>
        public MonoBehaviour target { get; private set; }

        /// <summary> the collider that came in as a parameter in the OnCollision/OnTrigger method </summary>
        public Collider other { get; private set; }

        /// <summary> the collider that had the OnCollision/OnTrigger method </summary>
        public Collider sender { get; private set; }

        /// <summary> not generated by triggers </summary>
        public Collision collision { get; private set; }


        public CollisionType type { get; private set; }


        public CollisionInfo(MonoBehaviour _target, Collider _sender, Collision _collision, CollisionType _type)
        {
            collision = _collision;
            target = _target;
            sender = _sender;
            other = collision.collider;
            type = _type;
        }

        public CollisionInfo(MonoBehaviour _target, Collider _sender, Collider _receiver, CollisionType _t)
        {
            target = _target;
            other = _receiver;
            sender = _sender;
            type = _t;
            collision = null;
        }

        public Vector3 FindOverlap()
        {
            // Get the positions of the colliders
            Vector3 pos1 = sender.transform.position;
            Vector3 pos2 = other.transform.position;

            // Get the extents of the colliders (half their size)
            Vector3 extents1 = sender.transform.localScale / 2f;
            Vector3 extents2 = other.transform.localScale / 2f;

            // Calculate the minimum and maximum points of the colliders
            Vector3 min1 = pos1 - extents1;
            Vector3 max1 = pos1 + extents1;
            Vector3 min2 = pos2 - extents2;
            Vector3 max2 = pos2 + extents2;

            // Find the overlapping intervals on each axis
            float overlapX = FindOverlap(min1.x, max1.x, min2.x, max2.x);
            float overlapY = FindOverlap(min1.y, max1.y, min2.y, max2.y);
            float overlapZ = FindOverlap(min1.z, max1.z, min2.z, max2.z);

            // Find the axis with the smallest overlap (the separating axis)
            float minOverlap = Mathf.Min(Mathf.Abs(overlapX), Mathf.Abs(overlapY), Mathf.Abs(overlapZ));
            Vector3 separatingAxis = Vector3.zero;

            if (Mathf.Approximately(minOverlap, Mathf.Abs(overlapX)))
            {
                separatingAxis = Vector3.right * Mathf.Sign(overlapX);
            }
            else if (Mathf.Approximately(minOverlap, Mathf.Abs(overlapY)))
            {
                separatingAxis = Vector3.up * Mathf.Sign(overlapY);
            }
            else if (Mathf.Approximately(minOverlap, Mathf.Abs(overlapZ)))
            {
                separatingAxis = Vector3.forward * Mathf.Sign(overlapZ);
            }

            // Calculate the collision point as the average of the contact points on the separating axis
            Vector3 contact1 = GetContactPoint(min1, max1, separatingAxis);
            Vector3 contact2 = GetContactPoint(min2, max2, -separatingAxis);
            Vector3 collisionPoint = (contact1 + contact2) / 2f;

            return collisionPoint;
        }

        // Helper function to find the overlapping interval on a single axis
        private float FindOverlap(float min1, float max1, float min2, float max2)
        {
            if (max1 < min2 || max2 < min1)
            {
                return 0f; // no overlap
            }

            float overlap = Mathf.Min(max1, max2) - Mathf.Max(min1, min2);
            return Mathf.Sign(overlap) * overlap;
        }
        // Helper function to find the contact point on a collider given a separating axis
        private Vector3 GetContactPoint(Vector3 min, Vector3 max, Vector3 separatingAxis)
        {
            float distance1 = Vector3.Dot(min - sender.transform.position, separatingAxis);
            float distance2 = Vector3.Dot(max - sender.transform.position, separatingAxis);
            float t = distance1 / (distance1 - distance2);
            return Vector3.Lerp(min, max, t);
        }
    }
}