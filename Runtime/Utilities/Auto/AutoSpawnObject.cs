using SketchEngine.Mono;
using UnityEngine;

namespace SketchEngine.Utilities.Auto
{
    public class AutoSpawnObject : MonoBase
    {
        [SerializeField] GameObject _prefab;
        [SerializeField] float _spawnTime = 4f;
        [SerializeField] bool _spawnAtStart = true;

         GameObject _ins;

         float _timer;

         void Awake()
         {
             _timer = _spawnTime;
             
             if (_spawnAtStart)
             {
                 SpawnTheItem();
             }
         }

         protected override void FixedTick()
         {
             base.FixedTick();
             
             if (!_ins)
             {
                 _timer -= Time.deltaTime;
                 if (_timer <= 0)
                 {
                     _timer = _spawnTime;
                     SpawnTheItem();
                 }
             }
         }

        public void SpawnTheItem()
        {
            if (_ins)
            {
                Destroy(_ins);
            }

            if (_prefab)
            {
                _ins = Instantiate(_prefab, TransformCached.position, TransformCached.rotation);
            }
            else
            {
                Debug.LogWarning("Please add a item to spawn.");
            }
        }
    }
}
