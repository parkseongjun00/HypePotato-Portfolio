using UnityEngine;
using HypePotato.Core;
using HypePotato.Entity.Enemy.States;

namespace HypePotato.Entity.Enemy
{
    /// <summary>
    /// 일반 몹 컴포넌트.
    /// 상태 머신 로직은 부모 Unit<T,T>에 위임하고, 고유 로직에 집중함.
    /// </summary>
    public class Monster : Enemy<Monster, EnemyData>
    {
        #region State Machine
        public IState<Monster> IdleState { get; private set; } = new MonsterStateIdle();
        public IState<Monster> ChaseState { get; private set; } = new MonsterStateChase();
        public IState<Monster> CombatState { get; private set; } = new MonsterStateCombat();
        public IState<Monster> StateDead { get; private set; } = new MonsterStateDead();
        #endregion

        #region Unit Implementation
        protected override IState<Monster> DefaultState => IdleState;
        protected override IState<Monster> DeadState => StateDead;
        #endregion
    }
}
