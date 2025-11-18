using System.Collections.Generic;
using UnityEngine;

namespace VRAimLab.Gameplay
{
    /// <summary>
    /// Object pool for a specific prefab
    /// </summary>
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    /// <summary>
    /// Generic object pooler - optimizes performance by reusing GameObjects
    /// Prevents expensive Instantiate/Destroy calls during gameplay
    /// Essential for VR 90fps target
    /// </summary>
    public class ObjectPooler : MonoBehaviour
    {
        #region Singleton
        public static ObjectPooler Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        #endregion

        #region Inspector Fields
        [Header("Pool Settings")]
        [SerializeField] private List<Pool> pools = new List<Pool>();
        [SerializeField] private bool expandPoolIfNeeded = true;
        [SerializeField] private Transform poolParent; // Optional parent for organization
        #endregion

        #region Private Fields
        private Dictionary<string, Queue<GameObject>> poolDictionary;
        private Dictionary<string, GameObject> prefabDictionary; // For expanding pools
        #endregion

        #region Initialization
        private void Start()
        {
            InitializePools();
        }

        /// <summary>
        /// Initialize all pools at start
        /// </summary>
        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            prefabDictionary = new Dictionary<string, GameObject>();

            foreach (Pool pool in pools)
            {
                if (pool.prefab == null)
                {
                    Debug.LogWarning($"[ObjectPooler] Pool '{pool.tag}' has null prefab!");
                    continue;
                }

                Queue<GameObject> objectPool = new Queue<GameObject>();

                // Create initial pool
                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = CreatePooledObject(pool.prefab, pool.tag, i);
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(pool.tag, objectPool);
                prefabDictionary.Add(pool.tag, pool.prefab);

                Debug.Log($"[ObjectPooler] Created pool '{pool.tag}' with {pool.size} objects");
            }
        }

        /// <summary>
        /// Create a pooled object instance
        /// </summary>
        private GameObject CreatePooledObject(GameObject prefab, string poolTag, int index = -1)
        {
            GameObject obj = Instantiate(prefab);

            // Use provided index or try to get count from existing pool
            int objectIndex = index >= 0 ? index :
                             (poolDictionary.ContainsKey(poolTag) ? poolDictionary[poolTag].Count : 0);

            obj.name = $"{poolTag}_{objectIndex}";
            obj.SetActive(false);

            // Organize under pool parent
            if (poolParent != null)
            {
                obj.transform.SetParent(poolParent);
            }
            else
            {
                obj.transform.SetParent(transform);
            }

            return obj;
        }
        #endregion

        #region Pool Management
        /// <summary>
        /// Spawn object from pool at position and rotation
        /// </summary>
        /// <param name="tag">Pool tag</param>
        /// <param name="position">Spawn position</param>
        /// <param name="rotation">Spawn rotation</param>
        /// <returns>Spawned GameObject</returns>
        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Pool with tag '{tag}' doesn't exist!");
                return null;
            }

            GameObject objectToSpawn = null;

            // Check if any objects are available
            if (poolDictionary[tag].Count == 0)
            {
                if (expandPoolIfNeeded)
                {
                    // Create new object if pool is empty and expansion is allowed
                    objectToSpawn = CreatePooledObject(prefabDictionary[tag], tag);
                    Debug.Log($"[ObjectPooler] Expanded pool '{tag}' (new size: {GetPoolSize(tag) + 1})");
                }
                else
                {
                    Debug.LogWarning($"[ObjectPooler] Pool '{tag}' is empty and expansion is disabled!");
                    return null;
                }
            }
            else
            {
                objectToSpawn = poolDictionary[tag].Dequeue();
            }

            // Setup object
            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            // Call IPooledObject interface if implemented
            IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
            if (pooledObj != null)
            {
                pooledObj.OnObjectSpawn();
            }

            return objectToSpawn;
        }

        /// <summary>
        /// Return object to pool
        /// </summary>
        /// <param name="tag">Pool tag</param>
        /// <param name="obj">Object to return</param>
        public void ReturnToPool(string tag, GameObject obj)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Pool with tag '{tag}' doesn't exist!");
                Destroy(obj);
                return;
            }

            obj.SetActive(false);

            // Organize under pool parent
            if (poolParent != null)
            {
                obj.transform.SetParent(poolParent);
            }
            else
            {
                obj.transform.SetParent(transform);
            }

            poolDictionary[tag].Enqueue(obj);
        }

        /// <summary>
        /// Automatically return object to pool (disables GameObject)
        /// Object will be automatically returned based on its name
        /// </summary>
        public void ReturnToPool(GameObject obj)
        {
            // Try to find which pool this object belongs to based on name
            string poolTag = GetPoolTagFromName(obj.name);

            if (poolTag != null)
            {
                ReturnToPool(poolTag, obj);
            }
            else
            {
                Debug.LogWarning($"[ObjectPooler] Could not determine pool for object '{obj.name}'");
                Destroy(obj);
            }
        }

        /// <summary>
        /// Get pool tag from object name
        /// </summary>
        private string GetPoolTagFromName(string objectName)
        {
            foreach (string tag in poolDictionary.Keys)
            {
                if (objectName.StartsWith(tag))
                {
                    return tag;
                }
            }
            return null;
        }
        #endregion

        #region Pool Queries
        /// <summary>
        /// Check if pool exists
        /// </summary>
        public bool PoolExists(string tag)
        {
            return poolDictionary.ContainsKey(tag);
        }

        /// <summary>
        /// Get current available count in pool
        /// </summary>
        public int GetAvailableCount(string tag)
        {
            if (!poolDictionary.ContainsKey(tag))
                return 0;

            return poolDictionary[tag].Count;
        }

        /// <summary>
        /// Get total pool size (includes active objects)
        /// </summary>
        public int GetPoolSize(string tag)
        {
            if (!poolDictionary.ContainsKey(tag))
                return 0;

            // Count all objects with this tag in hierarchy
            int count = 0;
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith(tag))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Get count of active objects from pool
        /// </summary>
        public int GetActiveCount(string tag)
        {
            return GetPoolSize(tag) - GetAvailableCount(tag);
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Return all active objects from a pool
        /// </summary>
        public void ReturnAllToPool(string tag)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Pool with tag '{tag}' doesn't exist!");
                return;
            }

            // Find all active objects with this tag
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int returnedCount = 0;

            foreach (GameObject obj in allObjects)
            {
                if (obj.name.StartsWith(tag) && obj.activeInHierarchy)
                {
                    ReturnToPool(tag, obj);
                    returnedCount++;
                }
            }

            Debug.Log($"[ObjectPooler] Returned {returnedCount} objects to pool '{tag}'");
        }

        /// <summary>
        /// Clear and rebuild a pool
        /// </summary>
        public void RebuildPool(string tag, int newSize)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Pool with tag '{tag}' doesn't exist!");
                return;
            }

            // Clear existing pool
            ReturnAllToPool(tag);
            Queue<GameObject> pool = poolDictionary[tag];

            // Destroy excess objects
            while (pool.Count > newSize)
            {
                GameObject obj = pool.Dequeue();
                Destroy(obj);
            }

            // Create new objects if needed
            while (pool.Count < newSize)
            {
                GameObject obj = CreatePooledObject(prefabDictionary[tag], tag);
                pool.Enqueue(obj);
            }

            Debug.Log($"[ObjectPooler] Rebuilt pool '{tag}' with {newSize} objects");
        }

        /// <summary>
        /// Warm up pool by activating/deactivating all objects
        /// Helps prevent first-frame hitches
        /// </summary>
        public void WarmUpPool(string tag)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Pool with tag '{tag}' doesn't exist!");
                return;
            }

            List<GameObject> tempList = new List<GameObject>(poolDictionary[tag]);

            foreach (GameObject obj in tempList)
            {
                obj.SetActive(true);
                obj.SetActive(false);
            }

            Debug.Log($"[ObjectPooler] Warmed up pool '{tag}'");
        }

        /// <summary>
        /// Warm up all pools
        /// </summary>
        public void WarmUpAllPools()
        {
            foreach (string tag in poolDictionary.Keys)
            {
                WarmUpPool(tag);
            }
        }
        #endregion

        #region Debug
        /// <summary>
        /// Print pool statistics
        /// </summary>
        public void PrintPoolStats()
        {
            Debug.Log("===== Object Pool Statistics =====");
            foreach (string tag in poolDictionary.Keys)
            {
                int total = GetPoolSize(tag);
                int available = GetAvailableCount(tag);
                int active = GetActiveCount(tag);
                Debug.Log($"Pool '{tag}': Total={total}, Available={available}, Active={active}");
            }
            Debug.Log("==================================");
        }
        #endregion
    }

    /// <summary>
    /// Interface for pooled objects to implement
    /// Allows objects to reset themselves when spawned from pool
    /// </summary>
    public interface IPooledObject
    {
        void OnObjectSpawn();
    }
}
