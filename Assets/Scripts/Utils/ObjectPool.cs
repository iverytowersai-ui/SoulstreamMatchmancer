using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool to reuse GameObjects instead of Instantiate/Destroy.
/// Used for tiles, VFX particles, and score popups.
/// </summary>
public class ObjectPool
{
    #region Fields
    private Queue<GameObject> pool = new Queue<GameObject>();
    private GameObject prefab;
    private Transform parent;
    #endregion

    #region Constructor
    public ObjectPool(GameObject prefab, Transform parent, int initialSize)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab, parent);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
    #endregion

    #region Public Methods
    /// <summary>Get an object from the pool. Creates a new one if pool is empty.</summary>
    public GameObject Get()
    {
        GameObject obj;
        if (pool.Count > 0)
            obj = pool.Dequeue();
        else
            obj = Object.Instantiate(prefab, parent);

        obj.SetActive(true);
        return obj;
    }

    /// <summary>Return an object to the pool for reuse.</summary>
    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    /// <summary>Returns how many objects are currently available in the pool.</summary>
    public int AvailableCount => pool.Count;
    #endregion
}
