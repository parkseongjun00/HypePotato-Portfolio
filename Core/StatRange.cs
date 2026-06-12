using UnityEngine;

namespace HypePotato.Core
{
    /// <summary>
    /// 실수형(Float) 스탯의 기준값과 오차 범위를 저장하고, 랜덤 산출 연산을 수행하는 구조체.
    /// </summary>
    [System.Serializable]
    public struct StatRangeFloat
    {
        public float baseValue;
        public float variance;

        /// <summary>
        /// 설정된 기준값과 오차를 바탕으로 최솟값이 보장된 랜덤 값을 계산하여 반환함.
        /// </summary>
        public float Roll()
        {
            return Mathf.Max(0f, Random.Range(baseValue - variance, baseValue + variance));
        }
    }

    /// <summary>
    /// 정수형(Int) 스탯의 기준값과 오차 범위를 저장하고, 랜덤 산출 연산을 수행하는 구조체.
    /// </summary>
    [System.Serializable]
    public struct StatRangeInt
    {
        public int baseValue;
        public int variance;

        /// <summary>
        /// 설정된 기준값과 오차를 바탕으로 최솟값이 보장된 랜덤 값을 계산하여 반환함.
        /// </summary>
        public int Roll()
        {
            // 정수형 Random.Range는 최댓값이 exclusive이므로 +1 보정함.
            return Mathf.Max(0, Random.Range(baseValue - variance, baseValue + variance + 1));
        }
    }
}
