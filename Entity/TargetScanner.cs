using System;
using System.Collections;
using UnityEngine;

namespace HypePotato.Entity
{
    /// <summary>
    /// 물리 연산(OverlapCircleNonAlloc)을 활용해 타겟을 탐지하는 독립 컴포넌트.
    /// 스캔 타이밍을 분산시켜 대규모 유닛 환경에서 CPU 스파이크를 방지함.
    /// </summary>
    public class TargetScanner : MonoBehaviour
    {
        [Header("Global Configuration")]
        [SerializeField] private Core.CombatConfig combatConfig;

        private Collider2D[] scanResults;
        private LayerMask targetLayerMask;
        private float scanRadius;
        private Coroutine scanCoroutine;

        /// <summary>
        /// 새로운 타겟이 탐지되었을 때 발행됨.
        /// </summary>
        public event Action<Transform> OnTargetAcquired;

        /// <summary>
        /// 기존 타겟이 사라졌을 때 발행됨.
        /// </summary>
        public event Action OnTargetLost;

        /// <summary>
        /// 현재 추적 중인 타겟 Transform. 없으면 null.
        /// </summary>
        public Transform CurrentTarget { get; private set; }

        private void Awake()
        {
            if (combatConfig == null)
            {
                Debug.LogError($"[TargetScanner] {gameObject.name}에 CombatConfig가 할당되지 않았습니다! 인스펙터를 확인하세요.");
            }
            scanResults = new Collider2D[combatConfig.maxTargets];
        }

        /// <summary>
        /// 스캔 파라미터를 초기화하고 스캔 코루틴을 시작함.
        /// </summary>
        public void StartScanning(LayerMask mask, float radius)
        {
            targetLayerMask = mask;
            scanRadius      = radius;

            if (scanCoroutine != null)
            {
                StopCoroutine(scanCoroutine);
            }
            scanCoroutine = StartCoroutine(ScanRoutine());
        }

        /// <summary>
        /// 스캔 코루틴을 중지하고 현재 타겟을 초기화함.
        /// </summary>
        public void StopScanning()
        {
            if (scanCoroutine != null)
            {
                StopCoroutine(scanCoroutine);
                scanCoroutine = null;
            }
            CurrentTarget = null;
        }

        private IEnumerator ScanRoutine()
        {
            // 초기 스캔 타이밍 분산 (Staggering)
            // 모든 개체가 한 프레임에 스캔 연산을 하지 않도록 랜덤 딜레이 부여
            yield return new WaitForSeconds(Random.Range(0f, combatConfig.scanInterval));

            while (true)
            {
                PerformScan();
                yield return new WaitForSeconds(combatConfig.scanInterval);
            }
        }

        private void PerformScan()
        {
            Transform previousTarget = CurrentTarget;

            int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, scanRadius, scanResults, targetLayerMask);

            if (hitCount == 0)
            {
                CurrentTarget = null;

                if (previousTarget != null)
                {
                    OnTargetLost?.Invoke();
                }
                return;
            }

            Transform closestTarget = null;
            float minSqrDistance    = float.MaxValue;
            Vector2 currentPosition = transform.position;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = scanResults[i];
                if (hit == null || hit.transform == transform)
                {
                    continue;
                }

                // SqrMagnitude를 사용하여 제곱근(Distance) 연산 비용 절감
                float sqrDistance = (currentPosition - (Vector2)hit.transform.position).sqrMagnitude;
                if (sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    closestTarget  = hit.transform;
                }
            }

            CurrentTarget = closestTarget;

            if (previousTarget == null && CurrentTarget != null)
            {
                OnTargetAcquired?.Invoke(CurrentTarget);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, scanRadius);
        }
#endif
    }
}
