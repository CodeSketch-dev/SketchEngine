using UnityEngine;

namespace SketchEngine.Patterns.Pool
{
    [System.Serializable]
    public class PoolPrefabConfig : ScriptableObject
    {
        [SerializeField] GameObject _prefab;
        [SerializeField] bool _persistAcrossScenes;
        [SerializeField] bool _deactiveOnRelease = true;

        [Space]

        [SerializeField] int _poolPrewarm = 0;
        [SerializeField] int _poolCapacity = 10;
        [SerializeField] int _poolCapacityMax = 1000;

        public GameObject Prefab => _prefab;
        public bool PersistAcrossScenes => _persistAcrossScenes;

        public bool DeactiveOnRelease => _deactiveOnRelease;
        
        public int PoolPrewarm => _poolPrewarm;
        public int PoolCapacity => _poolCapacity;
        public int PoolCapacityMax => _poolCapacityMax;
    }
}
