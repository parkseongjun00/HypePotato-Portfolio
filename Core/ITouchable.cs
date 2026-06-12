namespace HypePotato.Core
{
    /// <summary>
    /// 터치/클릭 입력을 받을 수 있는 모든 오브젝트가 구현해야 하는 범용 인터페이스.
    /// </summary>
    public interface ITouchable
    {
        void OnReceiveTouch();
    }
}
