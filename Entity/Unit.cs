using System;
using UnityEngine;
using HypePotato.Core;

namespace HypePotato.Entity
{
    /// <summary>
    /// 전장의 모든 생명체가 공유하는 최상위 비제네릭 베이스 클래스.
    /// 제네릭 컬렉션(List&lt;Unit&gt;)의 기반이 되며 공통 체력 관리를 담당함.
    /// </summary>
    public abstract class Unit : MonoBehaviour, IDamageable
    {
        [Header("Vitals")]
        [SerializeField] private int currentHealth;

        /// <summary>
        /// 피격 처리. 체력을 amount만큼 감소시키고 0 이하이면 Die()를 호출함.
        /// </summary>
        public void TakeDamage(int amount)
        {
            currentHealth -= amount;

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected void SetHealth(int health)
        {
            currentHealth = health;
        }

        protected abstract void Die();
    }

    /// <summary>
    /// CRTP(Curiously Recurring Template Pattern) 중간 계층.
    /// 하위 객체들의 데이터, 이벤트 뿐만 아니라 상태 머신 구동부 전체를 책임짐.
    /// </summary>
    [RequireComponent(typeof(TargetScanner))]
    [RequireComponent(typeof(MovementController))]
    [RequireComponent(typeof(CombatController))]
    public abstract class Unit<TUnit, TData> : Unit, IEntity<TData>
        where TUnit : Unit<TUnit, TData>
        where TData : EntityData
    {
        [Header("Confirmed Stats")]
        [SerializeField] protected TData currentData;

        [Header("Runtime State")]
        [SerializeField] protected string currentStateName;

        [Header("Global Config")]
        [SerializeField] private CombatConfig combatConfig;

        protected IState<TUnit> currentState;

        public TData Data => currentData;
        public CombatConfig CombatConfig => combatConfig;
        public event Action<TUnit> OnDied;

        protected TargetScanner targetScanner;

        /// <summary>
        /// 현재 TargetScanner가 추적 중인 타겟 Transform. 없으면 null.
        /// </summary>
        public Transform Target => targetScanner != null ? targetScanner.CurrentTarget : null;

        /// <summary>
        /// 상태 클래스에서 이벤트 구독을 위해 TargetScanner에 직접 접근할 수 있도록 노출함.
        /// </summary>
        public TargetScanner TargetScanner => targetScanner;

        public MovementController MovementController { get; private set; }
        public CombatController CombatController { get; private set; }

        protected virtual void Awake()
        {
            targetScanner      = GetComponent<TargetScanner>();
            MovementController = GetComponent<MovementController>();
            CombatController   = GetComponent<CombatController>();
        }

        // 템플릿 메서드 패턴용 추상 프로퍼티
        protected abstract IState<TUnit> DefaultState { get; }
        protected abstract IState<TUnit> DeadState { get; }

        /// <summary>
        /// 풀에서 꺼내지거나 새로 스폰될 때 호출됨.
        /// 데이터를 주입받고 초기 상태로 진입함.
        /// </summary>
        public virtual void Initialize(TData data, bool isPreExisting = false)
        {
            currentData = data;
            SetHealth(data.maxHealth);
            ChangeState(GetInitialState(isPreExisting));
        }

        protected virtual IState<TUnit> GetInitialState(bool isPreExisting)
        {
            return DefaultState;
        }

        protected virtual void Update()
        {
            currentState?.Update((TUnit)this);
        }

        /// <summary>
        /// 현재 상태에서 지정 상태로 전환함. 동일 상태로의 전환은 무시함.
        /// </summary>
        public void ChangeState(IState<TUnit> newState)
        {
            if (currentState == newState)
            {
                return;
            }

            currentState?.Exit((TUnit)this);
            currentState = newState;

            if (currentState != null)
            {
                currentState.Enter((TUnit)this);
                currentStateName = currentState.GetType().Name;
            }
        }

        /// <summary>
        /// 사망 연출이 완료된 후 호출하여 OnDied 이벤트를 발행함.
        /// Spawner 등 외부 시스템이 이 이벤트를 수신하여 풀 반납을 처리함.
        /// </summary>
        public void CompleteDeath()
        {
            OnDied?.Invoke((TUnit)this);
        }

        protected override void Die()
        {
            ChangeState(DeadState);
        }
    }
}
