namespace HypePotato.Core
{
    /// <summary>
    /// 아군과 적군이 공통으로 가지는 런타임 데이터 베이스 클래스.
    /// </summary>
    [System.Serializable]
    public class EntityData
    {
        public string name;
        public int maxHealth;
        public int attack;
        public float moveSpeed;
        public float attackSpeed;    // 공격 속도 (높을수록 빠름, 하입 버프 배율 적용 기준)
        public float attackRange;    // 공격 사거리
        public float detectionRange; // 타겟 탐지 반경
    }
}
