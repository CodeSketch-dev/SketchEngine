using UnityEngine;

namespace SketchEngine.Utilities.Auto
{
    public class AutoSpawnObject : MonoBehaviour
    {
        [SerializeField] GameObject _prefab;
        [SerializeField] float _spawnTime = 4f;
        [SerializeField] bool _spawnAtStart = true;

        GameObject _spawned;

        float _timer;

        void Awake()
        {
            _timer = _spawnTime;

            if (_spawnAtStart)
            {
                Spawn();
            }
        }

        void FixedUpdate()
        {
            if (!_spawned)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0)
                {
                    _timer = _spawnTime;
                    Spawn();
                }
            }
        }

        public void Spawn()
        {
            if (_spawned)
            {
                Destroy(_spawned);
            }

            if (_prefab)
            {
                _spawned = Instantiate(_prefab, transform.position, transform.rotation);
            }
        }
    }
}
