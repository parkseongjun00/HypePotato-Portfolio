using System;
using System.Collections;
using UnityEngine;
using HypePotato.Core;

namespace HypePotato.Entity
{
    /// <summary>
    /// 롤(LoL) 방식의 선딜레이/후딜레이가 포함된 공격 사이클을 관리하는 컴포넌트.
    /// 아군, 적군 상관없이 사용하여 통일된 공격 감각을 제공함.
    /// </summary>
    public class CombatController : MonoBehaviour
    {
        private Coroutine attackRoutine;

        /// <summary>
        /// 현재 공격 사이클(선딜 + 후딜)이 진행 중인지 여부.
        /// </summary>
        public bool IsAttacking { get; private set; }

        /// <summary>
        /// 공격 사이클을 시작함.
        /// </summary>
        public void StartAttack(Transform target, float attackSpeed, float attackRange, float windUpRatio, float leeway, Action onHit, Action onMiss = null)
        {
            if (IsAttacking)
            {
                return;
            }
            attackRoutine = StartCoroutine(AttackCycle(target, attackSpeed, attackRange, windUpRatio, leeway, onHit, onMiss));
        }

        /// <summary>
        /// 타겟이 죽거나 상태가 강제 변경될 때 공격 사이클을 취소함.
        /// </summary>
        public void CancelAttack()
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }
            IsAttacking = false;
        }

        private IEnumerator AttackCycle(Transform target, float attackSpeed, float attackRange, float windUpRatio, float leeway, Action onHit, Action onMiss)
        {
            IsAttacking = true;

            // 쿨타임 = 1 / 공격속도 (초당 공격 횟수)
            float totalCooldown = 1.0f / attackSpeed;
            float windUpTime    = totalCooldown * windUpRatio;          // 선딜레이
            float windDownTime  = totalCooldown * (1f - windUpRatio);   // 후딜레이

            // 1. 선딜레이 (Wind-up 대기)
            yield return new WaitForSeconds(windUpTime);

            // 2. 타격 판정 — 선딜 종료 시점에 타겟이 사거리 내에 있는지 재검증
            if (target != null && target.gameObject.activeInHierarchy)
            {
                float sqrDistance  = (transform.position - target.position).sqrMagnitude;
                float allowedRange = attackRange + leeway;

                if (sqrDistance <= allowedRange * allowedRange)
                {
                    // 적중! (데미지는 콜백에서 처리)
                    onHit?.Invoke();
                }
                else
                {
                    // 빗나감 — 타겟이 선딜 중에 사거리를 이탈함
                    onMiss?.Invoke();
                    Debug.Log($"[CombatController] {gameObject.name}의 공격 빗나감 (타겟이 범위 이탈)");
                }
            }

            // 3. 후딜레이 (Wind-down 대기) — 공격 후 자세를 추스르는 시간
            yield return new WaitForSeconds(windDownTime);

            IsAttacking = false; // 사이클 완료, 다음 행동 가능
        }
    }
}
