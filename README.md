# HypePotato — 포트폴리오 코드 샘플

> **장르**: 전략 디펜스 (모바일 타겟)  
> **엔진**: Unity 2D (C#)  
> **상태**: 1인 개발, 출시 목표 진행 중

---

## 게임 개요

전장 곳곳에 숨어 있는 아군 유닛들을 터치로 찾아내어 영입시키고,  
영입된 유닛들이 밀려오는 적을 자동으로 상대하는 방어전 게임입니다.

---

## 이 저장소에 대해

> **원본 프로젝트는 비공개입니다.**  
> 이 저장소는 아키텍처 역량 시연을 위해 선별한 코드 샘플만 담고 있습니다.

### 제외된 파일과 이유

아래 파일들은 게임의 핵심 IP를 포함하므로 공개하지 않습니다.

| 제외 / 추상화 파일 | 사유 |
|---|---|
| `Ally.cs` (✅ 부분 공개) | 클래스 선언과 CRTP 구조는 코드에 포함. `HandleHypedAction` 내에 구현 / `OnReceiveTouch` 내부 로직만 비공개. |
| `AllyData.cs` (✅ 부분 공개) | 상속 구조만 포함. Hype 시스템 관련 필드는 비공개. |
| `AllyStates.cs` | Hidden/Idle/Combat 전환 로직이 게임 플레이 정체성과 직결됨 |
| `AllySpawnConfig.cs` | 아군 유닛 밸런스 설계 데이터 |
| `AllySpawner.cs` | 위 파일들에 직접 의존 |
| `Boss.cs` / `BossStates.cs` / `BossData.cs` / `BossSpawnConfig.cs` / `BossSpawner.cs` | 보스 기미 로직 미구현 상태 — 아키텍처는 Monster와 동일하므로 중복 포함 실익 없음 |

---

## 파일 구조

```
_Portfolio/
├── Core/
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

캐스팅 없이 타입 안전한 이벤트와 상태 전환을 구현합니다.

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

`Instantiate` 남발을 방지합니다. Queue로 비활성 오브젝트를 재사용하며,  
새로운 유닛 타입 추가 시 **한 줄** 상속으로 확장됩니다.

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

템플릿 SO 없이 **평균값 ± 오차 범위** 방식으로 매 스폰마다 고유한 개체를 생성합니다.  
하위 클래스는 `RollRandomStats()`를 override하여 전용 필드만 추가합니다.

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

### 4. 이벤트 기반 제네릭 FSM

`IState<TEntity>` 제네릭 인터페이스로 **캐스팅 없이** 각 유닛 타입에 맞는 상태를 구현합니다.  
타겟 탐지는 폴링 대신 `TargetScanner` 이벤트를 구독하여 Update 부하를 제거합니다.

```csharp
// IState<Monster> — Monster를 직접 받으므로 캐스팅 불필요
public class MonsterStateIdle : IState<Monster>
{
    private void OnTargetAcquired(Transform _)
    {
        _cachedMonster.ChangeState(_cachedMonster.ChaseState);
    }

    public void Enter(Monster monster)
    {
        // 폴링(Update 매 프레임 체크) 대신 이벤트 구독
        monster.TargetScanner.OnTargetAcquired += OnTargetAcquired;
    }

    public void Exit(Monster monster)
    {
        // 반드시 구독 해제 — 메모리 누수 방지
        monster.TargetScanner.OnTargetAcquired -= OnTargetAcquired;
    }
}
```

---

## 의존 관계 요약

```
InputManager ──── ITouchable ──────────────────────────── Ally (미공개)
                                                                │
EntitySpawner<T,T> ── EntitySpawnConfig<T> ── StatRange        │
        │                                                       │
        ▼                                                       ▼
  Unit (IDamageable)                                      Unit<T,T> (FSM)
        │                                                       │
        └── Enemy<T,T> ── Monster ── MonsterStates (IState<Monster>)
                       └── Boss   (미공개 — 로직 미구현)

                         TargetScanner ◄── CombatConfig (SO)
                         MovementController
                         CombatController
```
