namespace HypePotato.Core
{
    /// <summary>
    /// 아군 유닛(Ally)의 런타임 확정 스탯 데이터 클래스.
    /// EntityData(name, maxHealth, attack, moveSpeed 등)를 상속받음.
    /// </summary>
    [System.Serializable]
    public class AllyData : EntityData
    {
        // 아군 전용 스탯 필드 (게임 핵심 메커닉 관련 항목은 비공개).
    }
}
