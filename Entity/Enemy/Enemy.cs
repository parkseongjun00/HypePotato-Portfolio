using UnityEngine;
using HypePotato.Core;

namespace HypePotato.Entity.Enemy
{
    /// <summary>
    /// Monster와 Boss가 공유하는 제네릭 중간 클래스.
    /// 적군이라면 공통적으로 가지는 로직을 통합 관리함.
    /// </summary>
    public abstract class Enemy<TEnemy, TData> : Unit<TEnemy, TData>
        where TEnemy : Enemy<TEnemy, TData>
        where TData : EnemyData
    {
        // 현재는 비어 있지만 추후 드랍 테이블 등 적군 전용 공통 로직이 들어갈 수 있습니다.
        
        public override void Initialize(TData data, bool isPreExisting = false)
        {
            base.Initialize(data, isPreExisting);
            // 아군을 찾기 위한 스캐너 가동
            targetScanner.StartScanning(LayerMask.GetMask("Ally"), Data.detectionRange);
        }
    }
}
