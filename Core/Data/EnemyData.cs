namespace HypePotato.Core
{
    /// <summary>
    /// Monster의 런타임 확정 스탯 데이터 클래스.
    /// EntityData(name, maxHealth, attack, moveSpeed, attackSpeed, attackRange, detectionRange)를 상속받음.
    /// 적군 전용 필드(드랍 테이블, XP 등)가 생기면 여기에 추가함.
    /// </summary>
    [System.Serializable]
    public class EnemyData : EntityData
    {
    }
}
