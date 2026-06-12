namespace HypePotato.Core
{
    /// <summary>
    /// EntitySpawner에서 스폰되는 객체들이 반드시 가져야 하는 초기화 규약을 정의.
    /// </summary>
    public interface IEntity<TData> where TData : EntityData
    {
        void Initialize(TData data, bool isPreExisting = false);
    }
}
