using UnityEngine;

namespace HypePotato.Core
{
    /// <summary>
    /// 일반 몹(Monster)을 찍어낼 때 기준이 되는 스탯 범위를 정의한 설정 데이터 애셋.
    /// 공통 스탯(health, attack, moveSpeed, attackSpeed, attackRange, detectionRange)은
    /// EntitySpawnConfig에서 상속받음.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemySpawnConfig", menuName = "HypePotato/Core/Enemy Spawn Config")]
    public class EnemySpawnConfig : EntitySpawnConfig<EnemyData>
    {
        public override EnemyData RollRandomStats()
        {
            EnemyData data = base.RollRandomStats();
            data.name = "이름 없는 몬스터";
            return data;
        }
    }
}
