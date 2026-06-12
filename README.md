# HypePotato — 포트폴리오 코드 샘플

> **장르**: 전략 디펜스 (모바일 타겟)  
> **엔진**: Unity 2D (C#)  
> **상태**: 1인 개발, 출시 목표 진행 중

---

## 게임 개요

전장 곳곳에 숨어 있는 아군 유닛(게임에서는 감자, 고구마 등으로 표현)들을 찾아내어 터치로 아군으로 영입하고,  
영입된 유닛들이 밀려오는 적을 자동으로 상대하는 레이드 게임입니다.

---

## 이 저장소에 대해

> **원본 프로젝트는 비공개입니다.**  
> 이 저장소는 선별한 코드 샘플만 담고 있습니다.

---

## 파일 구조

```
_Portfolio/
├── Core/
│   ├── Config
│   ├── Data
│   ├── Interfaces
│   ├── EntityData.cs          — 공통 베이스 데이터 클래스
│   ├── AllyData.cs            — 아군 데이터 (EntityData 상속, 필드 일부 비공개)
│   ├── EnemyData.cs           — 적군 데이터 (EntityData 상속)
│   ├── IEntity.cs             — 스폰 초기화 규약 인터페이스
│   ├── ITouchable.cs          — 터치 수신 인터페이스
│   ├── IDamageable.cs         — 피격 처리 인터페이스
│   ├── StatRange.cs           — 절차적 스탯 생성 구조체 (Float/Int)
│   ├── EntitySpawnConfig.cs   — 스폰 설정 SO 추상 베이스
│   ├── EnemySpawnConfig.cs    — 일반 몹 스폰 설정 SO
│   ├── EntitySpawner.cs       — Queue 기반 풀링 + 스폰 통합 클래스
│   ├── CombatConfig.cs        — 전역 전투 수치 SO (스캔 주기, 판정 여유 등)
│   └── InputManager.cs        — New Input System 기반 통합 입력 매니저
└── Entity/
    ├── IState.cs              — 제네릭 FSM 상태 인터페이스
    ├── Unit.cs                — 비제네릭 베이스 + CRTP 중간 계층
    ├── Ally.cs                — 아군 컴포넌트 (CRTP 구조 시연, 일부 로직 비공개)
    ├── CombatController.cs    — 선딜/후딜 공격 사이클 컴포넌트
    ├── MovementController.cs  — Separation 기반 충돌 회피 이동
    ├── TargetScanner.cs       — 분산 스캔 타겟 탐지 컴포넌트
    └── Enemy/
        ├── Enemy.cs           — 적군 공통 제네릭 중간 계층
        ├── MonsterSpawner.cs  — 웨이브 스폰 매니저
        └── Monster/
            ├── Monster.cs     — 일반 몹 컴포넌트
            └── MonsterStates.cs — Idle / Chase / Combat / Dead FSM 구현
```

---

## 핵심 기술 포인트

### 1. CRTP 기반 다단계 제네릭 계층
 
유닛 종류가 늘어날수록 '공통으로 처리해야 하는 부분'과 '이 유닛만 신경 써야 하는 부분'이 점점 헷갈리기 시작했습니다. 그래서 공통 로직은 최대한 상위 계층(`Unit<TUnit, TData>`)으로 위임하고, 각 유닛은 자기한테 정말 필요한 부분에만 집중할 수 있도록 구조를 잡았습니다. 공통 로직을 한 곳에 모아두면 수정할 때도 한 군데만 고치면 되고, 새로운 유닛을 추가하는 입장에서도 "무엇을 신경 써야 하는지?"에 대한 고민을 최대한 줄이고자 했습니다.
 
```
Unit (비제네릭 — IDamageable, List<Unit> 기반)
└── Unit<TUnit, TData> (CRTP 중간 계층 — FSM 구동부, OnDied 이벤트)
    ├── Ally                 ← IState<Ally>, OnHyped: Action<Ally>
    └── Enemy<TEnemy, TData>
        ├── Monster             ← MonsterStates.cs 참조
        └── Boss (스탯)
```
 
**Ally에서 보이는 CRTP 포인트:**
 
```csharp
// 자기 자신을 타입 인수로 전달 (Curiously Recurring Template Pattern)
// → Unit<T,T> 안에서 Action<TUnit>(직접 구체 타입)으로 OnDied가 조성됨
public class Ally : Unit<Ally, AllyData>, ITouchable
{
    // IState<Ally> — 캐스팅 없이 Ally를 직접 받는 상태 인터페이스
    public IState<Ally> HiddenState { get; private set; }
    public IState<Ally> IdleState   { get; private set; }
    public IState<Ally> CombatState { get; private set; }
    public IState<Ally> StateDead   { get; private set; }
 
    // 스폰 맥락에 따라 초기 상태를 결정하는 템플릿 메서드 오버라이드
    protected override IState<Ally> GetInitialState(bool isPreExisting)
    {
        return isPreExisting ? IdleState : HiddenState;
    }
 
    // Hype 이벤트 — Unit<Ally, AllyData>를 상속했으므로 Action<Ally>로 구성됨
    public event Action<Ally> OnHyped;
}
```
 
**Monster에서 보이는 CRTP 포인트:**
 
```csharp
// CRTP 덕분에 OnDied가 Action<Monster>로 정확히 타입 매핑됨
// — 캐스팅 없이 Monster를 직접 받을 수 있음
monster.OnDied += HandleMonsterDied;
 
private void HandleMonsterDied(Monster monster)
{
    Despawn(monster); // EntitySpawner<Monster, EnemyData>
}
```
 
---
 
### 2. 오브젝트 풀링 (`EntitySpawner<TEntity, TData>`)
 
레이드 게임 특성상 씬 내에 유닛이 많이 존재합니다. 이전 프로젝트에서의 경험을 생각했을 때, `Instantiate`/`Destroy`를 개별적으로 호출하면 GC 부하에 따른 성능 저하가 예상되기에 비활성화된 오브젝트를 Queue에 쌓아두고 재사용하는 풀링 구조를 적용했습니다. 또한 새로운 유닛 타입을 추가할 때 별도 풀링 로직을 구현할 필요 없이 한 줄 상속으로 끝나도록 만들었습니다.
 
```csharp
// 적군 추가: 한 줄로 완성
public class MonsterSpawner : EntitySpawner<Monster, EnemyData> { }
 
// 내부: 풀에 여분이 있으면 재사용, 없으면 그때만 Instantiate
private TEntity GetFromQueue()
{
    if (pool.Count > 0)
    {
        TEntity obj = pool.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }
    return Instantiate(spawnConfig.prefab).GetComponent<TEntity>();
}
```
 
---
 
### 3. 절차적 스탯 생성 (`StatRange`)
 
주요 게임 철학 중에 유닛들이 개별적으로 고유한 정체성을 가져서 플레이어가 개별 유닛들에게 애정을 가질 수 있는 환경을 원했습니다. 이에 따른 노력 중 하나로, 유닛들이 개별적인 스탯을 가져 정체성이 더욱 두드러지길 원하면서도 평균 내 오차 범위를 가져 밸런스가 파괴되지 않기를 바랐습니다. 유닛의 고유 스탯을 가리키고 오차 범위 내 값을 생성하는 구조체를 정의하여 이를 구현했습니다.
 
```csharp
// 인스펙터: baseValue=100, variance=20 → HP가 80~120 사이 랜덤 결정
public int Roll()
{
    return Mathf.Max(0, Random.Range(baseValue - variance, baseValue + variance + 1));
}
 
// 적군 스폰 설정 예시 — RollRandomStats() 오버라이드 체인
public class EnemySpawnConfig : EntitySpawnConfig<EnemyData>
{
    public override EnemyData RollRandomStats()
    {
        EnemyData data = base.RollRandomStats(); // 공통 스탯 먼저
        data.name = "이름 없는 몬스터";          // 전용 필드 추가
        return data;
    }
}
```

---

## 의존 관계 요약

```
InputManager ──── ITouchable ───────────────────────────────── Ally 
                                                                │
EntitySpawner<T,T> ── EntitySpawnConfig<T> ── StatRange         │
        │                                                       │
        ▼                                                       ▼
  Unit (IDamageable)                                      Unit<T,T> (FSM)
        │                                                       │
        └── Enemy<T,T> ── Monster ── MonsterStates (IState<Monster>)
                       └── Boss

                         TargetScanner ◄── CombatConfig (SO)
                         MovementController
                         CombatController
```
