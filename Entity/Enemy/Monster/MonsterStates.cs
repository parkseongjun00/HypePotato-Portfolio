using UnityEngine;
using HypePotato.Core;

namespace HypePotato.Entity.Enemy.States
{
    // Monster의 모든 상태 클래스를 한 파일에 모아 파편화를 방지합니다.

    public class MonsterStateIdle : IState<Monster>
    {
        private Monster cachedMonster;

        private void OnTargetAcquired(Transform _)
        {
            if (cachedMonster != null)
            {
                cachedMonster.ChangeState(cachedMonster.ChaseState);
            }
        }

        public void Enter(Monster monster)
        {
            cachedMonster = monster;
            monster.MovementController.Stop();

            // 타겟이 없었다가 생기면 추적 상태로 전환 — 폴링 대신 이벤트로 감지
            monster.TargetScanner.OnTargetAcquired += OnTargetAcquired;

            Debug.Log($"[Monster] {monster.Data.name} 대기 중.");
        }

        // 타겟 감지는 TargetScanner 이벤트가 담당하므로 Update는 완전히 비워둠.
        public void Update(Monster monster) { }

        public void Exit(Monster monster)
        {
            monster.TargetScanner.OnTargetAcquired -= OnTargetAcquired;
            cachedMonster = null;
        }
    }

    public class MonsterStateChase : IState<Monster>
    {
        private Monster cachedMonster;

        private void OnTargetLost()
        {
            if (cachedMonster != null)
            {
                cachedMonster.ChangeState(cachedMonster.IdleState);
            }
        }

        public void Enter(Monster monster)
        {
            cachedMonster = monster;
            // 추적 중 타겟이 사라지면 대기 복귀 — 이벤트 기반
            monster.TargetScanner.OnTargetLost += OnTargetLost;

            Debug.Log($"[Monster] {monster.Data.name} 추적 시작. 타겟: {monster.Target?.name}");
        }

        public void Update(Monster monster)
        {
            if (monster.Target == null)
            {
                return;
            }

            MoveOrEnterCombat(monster);
        }

        private void MoveOrEnterCombat(Monster monster)
        {
            // sqrMagnitude로 제곱근 연산 없이 거리 비교
            float sqrDist  = ((Vector2)monster.transform.position - (Vector2)monster.Target.position).sqrMagnitude;
            float sqrRange = monster.Data.attackRange * monster.Data.attackRange;

            if (sqrDist <= sqrRange)
            {
                // 사거리 이내 진입: 전투 상태로 전환
                monster.ChangeState(monster.CombatState);
            }
            else
            {
                // 사거리 밖: 계속 추적
                monster.MovementController.MoveTowards(monster.Target.position, monster.Data.moveSpeed);
            }
        }

        public void Exit(Monster monster)
        {
            monster.TargetScanner.OnTargetLost -= OnTargetLost;
            cachedMonster = null;
        }
    }

    public class MonsterStateCombat : IState<Monster>
    {
        public void Enter(Monster monster)
        {
            monster.MovementController.Stop();
            Debug.Log($"[Monster] {monster.Data.name} 공격 개시!");
        }

        public void Update(Monster monster)
        {
            if (monster.Target == null)
            {
                monster.ChangeState(monster.IdleState);
                return;
            }

            ProcessCombat(monster);
        }

        private void ProcessCombat(Monster monster)
        {
            float sqrDist = ((Vector2)monster.transform.position - (Vector2)monster.Target.position).sqrMagnitude;

            // 사거리 + 이탈 여유치를 초과하면 다시 추적으로 복귀
            float exitRange    = monster.Data.attackRange + monster.CombatConfig.hitRangeLeeway;
            float sqrExitRange = exitRange * exitRange;

            if (sqrDist > sqrExitRange)
            {
                monster.ChangeState(monster.ChaseState);
                return;
            }

            if (!monster.CombatController.IsAttacking)
            {
                Transform target = monster.Target;
                monster.CombatController.StartAttack(
                    target:      target,
                    attackSpeed: monster.Data.attackSpeed,
                    attackRange: monster.Data.attackRange,
                    windUpRatio: 0.3f,
                    leeway:      monster.CombatConfig.hitRangeLeeway,
                    onHit: () =>
                    {
                        if (target != null && target.TryGetComponent<IDamageable>(out var damageable))
                        {
                            damageable.TakeDamage(monster.Data.attack);
                        }
                    }
                );
            }
        }

        public void Exit(Monster monster)
        {
            monster.CombatController.CancelAttack();
        }
    }

    public class MonsterStateDead : IState<Monster>
    {
        public void Enter(Monster monster)
        {
            monster.MovementController.Stop();
            monster.CombatController.CancelAttack();
            Debug.Log($"[Monster] {monster.Data.name} 사망.");
            monster.CompleteDeath();
        }

        public void Update(Monster monster) { }

        public void Exit(Monster monster) { }
    }
}
