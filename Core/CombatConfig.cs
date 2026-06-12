using UnityEngine;

namespace HypePotato.Core
{
    /// <summary>
    /// 전역 전투 설정 및 최적화 수치를 중앙에서 관리하는 에셋.
    /// 에디터에서 값을 변경하면 이 에셋을 참조하는 모든 유닛에 즉시 반영됨.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatConfig", menuName = "HypePotato/Config/CombatConfig")]
    public class CombatConfig : ScriptableObject
    {
        [Header("Target Scanner Settings")]
        [Tooltip("Scan interval in seconds. Lower values respond faster but increase CPU load.")]
        public float scanInterval = 0.25f;

        [Tooltip("Max array size for target scan queries.")]
        public int maxTargets = 10;

        [Header("Combat Rules")]
        [Tooltip("Hit leeway distance. Allows a hit to register slightly outside attack range.")]
        public float hitRangeLeeway = 0.3f;
    }
}
