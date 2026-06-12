using System;
using UnityEngine;
using HypePotato.Core;

namespace HypePotato.Entity
{
    /// <summary>
    /// 게임 씬에 스폰되는 아군 유닛 컴포넌트.
    ///
    /// CRTP(Curiously Recurring Template Pattern)를 활용하여 Unit&lt;Ally, AllyData&gt;를 상속받음.
    /// 덕분에 OnDied 등 부모 이벤트가 Action&lt;Ally&gt;로 정확히 타입 매핑되어, 외부에서
    /// 캐스팅 없이 Ally를 직접 받을 수 있음.
    ///
    /// 상태 머신 구동부는 부모 Unit&lt;T,T&gt;에 위임하고, 아군 고유 로직에만 집중함.
    /// ITouchable을 추가 구현하여 터치 입력에 직접 반응함.
    /// </summary>
    public class Ally : Unit<Ally, AllyData>, ITouchable
    {
        #region Inspector Fields
        [SerializeField] private float currentHypeTimer = 0f;
        #endregion

        #region Properties
        /// <summary>
        /// 현재 하입 버프가 활성화된 상태인지 여부.
        /// </summary>
        public bool IsHyped => currentHypeTimer > 0f;
        #endregion

        #region State Machine
        // AllyStates.cs는 게임 핵심 메커닉(Hype 시스템)을 포함하므로 이 저장소에서 제외됨.
        // 구조 시연을 위해 IState<Ally> 선언만 표시함.
        public IState<Ally> HiddenState { get; private set; } /* = new StateHidden(); */
        public IState<Ally> IdleState   { get; private set; } /* = new StateIdle();   */
        public IState<Ally> CombatState { get; private set; } /* = new StateCombat(); */
        public IState<Ally> StateDead   { get; private set; } /* = new StateDead();   */
        #endregion

        #region Unit<T,T> 추상 멤버 구현
        protected override IState<Ally> DefaultState => IdleState;
        protected override IState<Ally> DeadState    => StateDead;

        /// <summary>
        /// 스폰 맥락에 따라 초기 상태를 결정하는 템플릿 메서드 오버라이드.
        /// 기존 데이터로 복원된 경우 Idle, 신규 스폰이면 Hidden 상태로 진입함.
        /// </summary>
        protected override IState<Ally> GetInitialState(bool isPreExisting)
        {
            return isPreExisting ? IdleState : HiddenState;
        }
        #endregion

        #region Events
        /// <summary>
        /// 하입 조건이 충족되었을 때 발행됨.
        /// Hidden 상태에서는 영입 완료 신호로, Combat 상태에서는 버프 트리거로 사용됨.
        /// </summary>
        public event Action<Ally> OnHyped;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            OnHyped += HandleHypedAction;
        }

        private void OnDisable()
        {
            OnHyped -= HandleHypedAction;
        }

        protected override void Update()
        {
            base.Update(); // 부모의 currentState.Update() 호출
            UpdateHypeTimer();
        }

        private void UpdateHypeTimer()
        {
            if (currentHypeTimer > 0f)
            {
                currentHypeTimer -= Time.deltaTime;
                if (currentHypeTimer <= 0f)
                {
                    Debug.Log($"[Ally] {Data.name} 하입 버프 종료.");
                }
            }
        }
        #endregion

        #region IEntity<AllyData> 구현
        /// <summary>
        /// 풀에서 꺼내지거나 새로 스폰될 때 호출됨.
        /// 부모 초기화 후 적군 탐지 스캐너를 가동함.
        /// </summary>
        public override void Initialize(AllyData data, bool isPreExisting = false)
        {
            base.Initialize(data, isPreExisting);
            // 적군을 찾기 위한 스캐너 가동
            targetScanner.StartScanning(LayerMask.GetMask("Enemy"), Data.detectionRange);
        }
        #endregion

        #region Private Handlers
        private void HandleHypedAction(Ally ally)
        {
            // 현재 상태에 따라 상태 전환 또는 버프를 활성화함.
            // 게임 핵심 메커닉으로 상세 구현은 비공개.
        }
        #endregion

        #region ITouchable 구현
        /// <summary>
        /// 플레이어가 이 유닛을 터치(클릭)했을 때 호출됨.
        /// 터치 누적 조건 충족 시 OnHyped 이벤트를 발행함.
        /// 상세 조건 로직은 게임 핵심 메커닉으로 비공개.
        /// </summary>
        public void OnReceiveTouch()
        {
            // 터치 횟수 누적 및 임계값 판정.
            // 게임 핵심 메커닉으로 상세 로직은 비공개.
            OnHyped?.Invoke(this);
        }
        #endregion
    }
}
