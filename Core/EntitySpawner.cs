using UnityEngine;
using System.Collections.Generic;

namespace HypePotato.Core
{
    /// <summary>
    /// 풀링(Pooling) 메커니즘과 스폰 데이터 주입을 하나로 통합한 마스터 클래스.
    /// 유니티 엔진과 직렬화하기 위해, 실제 사용할 때는 구상(Concrete) 클래스로 상속받아 인스펙터에 등록.
    /// </summary>
    public abstract class EntitySpawner<TEntity, TData> : MonoBehaviour
        where TEntity : MonoBehaviour, IEntity<TData>
        where TData : EntityData, new()
    {
        [Tooltip("Spawn config ScriptableObject used to generate randomized entity data.")]
        [SerializeField] private EntitySpawnConfig<TData> spawnConfig;

        private readonly Queue<TEntity> pool = new Queue<TEntity>();

        /// <summary>
        /// 완전히 무작위의 새로운 데이터를 굴려 스폰함. (신규 개체)
        /// </summary>
        public TEntity Spawn(Vector3 position)
        {
            // spawnConfig에서 방금 굴린 데이터이므로 isPreExisting = false
            return SpawnInternal(position, spawnConfig.RollRandomStats(), false);
        }

        /// <summary>
        /// 외부에 이미 존재하는 데이터를 주입하여 스폰함. (기존 개체)
        /// </summary>
        public TEntity Spawn(Vector3 position, TData specificData)
        {
            // 외부에서 주입해준 확정 데이터이므로 isPreExisting = true
            return SpawnInternal(position, specificData, true);
        }

        /// <summary>
        /// 내부적으로 스폰 로직을 전담하는 공통 함수. (코드 중복 방지)
        /// </summary>
        private TEntity SpawnInternal(Vector3 position, TData data, bool isPreExisting)
        {
            TEntity entity = GetFromQueue();

            entity.Initialize(data, isPreExisting);
            entity.transform.position = position;

            return entity;
        }

        /// <summary>
        /// 사용이 끝난 개체를 비활성화하여 풀에 반납함.
        /// </summary>
        public void Despawn(TEntity entity)
        {
            entity.gameObject.SetActive(false);
            pool.Enqueue(entity);
        }

        private TEntity GetFromQueue()
        {
            if (pool.Count > 0)
            {
                TEntity obj = pool.Dequeue();
                obj.gameObject.SetActive(true);
                return obj;
            }

            GameObject newObj = Instantiate(spawnConfig.prefab, transform);
            TEntity component = newObj.GetComponent<TEntity>();

            return component;
        }
    }
}
