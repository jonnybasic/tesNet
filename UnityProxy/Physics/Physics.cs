using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class Physics
    {
        public static Collider[] OverlapSphere(Vector3 position, float explosionRadius)
        {
            throw new NotImplementedException();
        }

        public static bool SphereCast(Ray ray, float sphereCastRadius, out RaycastHit hit, float touchRange)
        {
            throw new NotImplementedException();
        }

        public static void IgnoreCollision(Collider collider1, Collider collider2)
        {
            throw new NotImplementedException();
        }

        public static bool Raycast(Ray ray, float maxDistance)
        {
            throw new NotImplementedException();
        }

        public static bool Raycast(Ray ray, out RaycastHit hit, float maxDistance)
        {
            throw new NotImplementedException();
        }

        public static bool Raycast(Vector3 position, Vector3 direction, float maxDistance)
        {
            throw new NotImplementedException();
        }

        public static bool Raycast(Vector3 position, Vector3 direction, out RaycastHit hit, float v)
        {
            throw new NotImplementedException();
        }
    }

    [Flags]
    public enum CollisionFlags : short
    {
        Below
    }

    public class Collision : GameObject
    { }

    public class Collider : GameObject
    {
        public bool isTrigger;
    }

    public class CapsuleCollider : Collider
    {
    }

    public class BoxCollider : Collider
    {
        public Bounds bounds;
        public Vector3 center;
    }

    public class SphereCollider : Collider
    {
        public float radius;
    }

    public class MeshCollider : Collider
    {
        public Mesh sharedMesh;
        public bool convex;
    }

    public class TerrainCollider : Collider
    {
    }

    public class Rigidbody : Collider
    {
        public bool useGravity;
        public bool isKinematic;
    }

    public class CharacterController : GameObject
    {
        public float height;
        public Vector2 velocity;
        public float radius;
    }

    public class Ray : GameObject
    {
        public Ray(Vector3 aimPosition, Vector3 aimDirection)
        {
        }
    }

    public struct ControllerColliderHit
    {
        public readonly Transform transform;
        public readonly Collider collider;
        public readonly Vector3 point;
        public readonly Vector3 moveDirection;
        public readonly Vector3 normal;
    }

    public struct RaycastHit
    {
        public readonly Transform transform;
        public readonly Collider collider;
        public readonly Vector3 point;
        public readonly Vector3 normal;
        public readonly int distance;
    }
}
