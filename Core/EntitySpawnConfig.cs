using UnityEngine;

namespace HypePotato.Core
{
    /// <summary>
    /// 아군과 적군의 공통 스탯 범위를 정의하는 ScriptableObject의 부모 클래스.
    /// </summary>
    public abstract class EntitySpawnConfig<TData> : ScriptableObject where TData : EntityData, new()
    {
        public GameObject prefab;

        [Header("Stats")]
        public StatRangeInt health;
        public StatRangeInt attack;
        public StatRangeFloat moveSpeed;

        [Header("Combat")]
        public StatRangeFloat attackSpeed;
        public StatRangeFloat attackRange;
        public StatRangeFloat detectionRange;

        /// <summary>
        /// 하위 클래스에서 각자의 TData를 인스턴스화하고 공통 스탯을 채워주는 가상 함수.
        /// </summary>
        public virtual TData RollRandomStats()
        {
            var data = new TData
            {
                maxHealth      = health.Roll(),
                attack         = attack.Roll(),
                moveSpeed      = moveSpeed.Roll(),
                attackSpeed    = attackSpeed.Roll(),
                attackRange    = attackRange.Roll(),
                detectionRange = detectionRange.Roll()
            };
            return data;
        }
    }
}
