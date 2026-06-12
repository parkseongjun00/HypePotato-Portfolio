namespace HypePotato.Core
{
    /// <summary>
    /// 아군·적군 구분 없이 피격 처리를 위한 공통 규약.
    /// 교전 로직 작성 시, 상대가 어떤 Unit 타입인지 알 필요 없이
    /// 이 인터페이스만 참조하여 TakeDamage()를 호출함.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int amount);
    }
}
