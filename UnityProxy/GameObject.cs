using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public class Object
    {
        public string name;

        public int GetInstanceID()
        {
            throw new NotImplementedException();
        }

        public static void Destroy(GameObject go)
        {
            throw new NotImplementedException();
        }

        public static void DestroyImmediate(GameObject o, bool b)
        {
            throw new NotImplementedException();
        }

        public static implicit operator bool(Object o)
            => o != null;

        public static T Instantiate<T>(T asset) where T : Object
        {
            throw new NotImplementedException();
        }
    }

    [Flags]
    public enum HideFlags : short
    {
        None
    }

    public class GameObject : Object
    {
        public bool enabled;

        public Transform transform;

        public GameObject gameObject
        { get => this; }

        public string tag;
        public int layer;
        public HideFlags hideFlags;

        public GameObject()
        { }

        public GameObject(string name)
        { }

        public bool CompareTag(string other)
        {
            throw new NotImplementedException();
        }

        public void DontDestroyOnLoad(GameObject gameObject)
        {
            throw new NotImplementedException();
        }

        public object AddComponent(Type t)
        {
            throw new NotImplementedException();
        }

        public T AddComponent<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public T GetComponent<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public T[] GetComponentsInChildren<T>()
        {
            throw new NotImplementedException();
        }

        //static T[] FindObjectsOfType<T>() where T : class
        //{
        //    throw new NotImplementedException();
        //}

        public T[] GetComponents<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public static GameObject FindGameObjectWithTag(string tag)
        {
            throw new NotImplementedException();
        }

        public static T FindObjectOfType<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public static T[] FindObjectsOfType<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public static implicit operator bool(GameObject go)
            => go != null;

        public void SetActive(bool v)
        {
            throw new NotImplementedException();
        }

        public static GameObject CreatePrimitive(PrimitiveType cylinder)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Component : GameObject
    { }

    public class Transform : GameObject, IList<Transform>
    {
        public Vector3 position;
        public Vector3 localPosition;
        public Quaternion rotation;
        public Vector3 localScale;

        public Vector3 eulerAngles;
        public Vector3 localEulerAngles;
        public readonly Vector3 forward;
        public readonly Vector3 right;

        public Transform parent;

        public int childCount;

        public Transform GetChild(int index)
        {
            throw new NotImplementedException();
        }

        public Transform Find(string key)
        {
            throw new NotImplementedException();
        }

        public void Rotate(Vector3 axis, float angle)
        {
            throw new NotImplementedException();
        }

        public void Rotate(float x, float y, float z)
        {
            throw new NotImplementedException();
        }

        public Transform this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Vector3 TransformDirection(Vector3 direction)
        {
            throw new NotImplementedException();
        }

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(Transform item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(Transform item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Transform[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Transform> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(Transform item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, Transform item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Transform item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void SetParent(Transform transform)
        {
            throw new NotImplementedException();
        }

        public void Translate(Vector3 vector3, Space space)
        {
            throw new NotImplementedException();
        }

        public void LookAt(Vector3 target, Vector3 up)
        {
            throw new NotImplementedException();
        }

        public void LookAt(Transform target, Vector3 up)
        {
            throw new NotImplementedException();
        }

        public bool IsChildOf(Transform transform)
        {
            throw new NotImplementedException();
        }
    }
}
