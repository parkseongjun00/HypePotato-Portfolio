namespace HypePotato.Entity
{
    /// <summary>
    /// 생명체의 모든 상태 클래스가 구현해야 하는 제네릭 공통 규약.
    /// TEntity에 Ally, Monster, Boss 등 자신의 정확한 타입을 지정하여 캐스팅 없이 사용함.
    /// </summary>
    public interface IState<TEntity>
    {
        void Enter(TEntity entity);
        void Update(TEntity entity);
        void Exit(TEntity entity);
    }
}
