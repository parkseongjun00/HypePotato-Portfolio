using System.Collections;
using UnityEngine;
using HypePotato.Core;

namespace HypePotato.Entity.Enemy
{
    /// <summary>
    /// 일반 몹 웨이브 스폰 관리자.
    /// </summary>
    public class MonsterSpawner : EntitySpawner<Monster, EnemyData>
    {
        #region Inspector Fields
        [Header("Wave Settings")]
        [Tooltip("Interval in seconds between each wave.")]
        [SerializeField] private float waveInterval = 5f;

        [Tooltip("Number of monsters spawned per wave.")]
        [SerializeField] private int monstersPerWave = 3;

        [Header("Spawn Points")]
        [Tooltip("Outer edge spawn point transforms. Randomly selected each wave.")]
        [SerializeField] private Transform[] spawnPoints;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("[MonsterSpawner] 스폰 포인트가 지정되지 않았습니다.");
                return;
            }

            StartCoroutine(WaveRoutine());
        }
        #endregion

        #region Wave Logic
        private IEnumerator WaveRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(waveInterval);
                SpawnWave();
            }
        }

        private void SpawnWave()
        {
            for (int i = 0; i < monstersPerWave; i++)
            {
                Transform point  = spawnPoints[Random.Range(0, spawnPoints.Length)];
                Monster monster  = Spawn(point.position);

                // CRTP 적용으로 monster.OnDied가 Action<Monster>이므로 캐스팅 없이 바로 구독 가능
                monster.OnDied += HandleMonsterDied;
            }

            Debug.Log($"[MonsterSpawner] 웨이브 스폰 완료. ({monstersPerWave}마리)");
        }

        private void HandleMonsterDied(Monster monster)
        {
            DespawnMonster(monster);
        }

        private void DespawnMonster(Monster monster)
        {
            if (monster == null)
            {
                return;
            }

            monster.OnDied -= HandleMonsterDied;
            Despawn(monster);
        }
        #endregion
    }
}
