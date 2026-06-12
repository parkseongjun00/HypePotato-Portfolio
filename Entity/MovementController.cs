using UnityEngine;

namespace HypePotato.Entity
{
    /// <summary>
    /// 타겟을 향해 이동하되, 주변 유닛들과의 충돌을 수학적으로 회피(Separation)하는
    /// 이동 전담 컴포넌트.
    /// </summary>
    public class MovementController : MonoBehaviour
    {
        [Header("Movement & Separation Settings")]
        [Tooltip("Separation radius for neighbor avoidance. Tune to match unit size.")]
        [SerializeField] private float separationRadius = 0.5f;

        [Tooltip("Strength of the separation force.")]
        [SerializeField] private float separationWeight = 1.5f;

        [Tooltip("Max number of neighbors to sample per frame for separation.")]
        [SerializeField] private int maxNeighbors = 5;

        private Collider2D[] neighborResults;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            neighborResults = new Collider2D[maxNeighbors];
            spriteRenderer  = GetComponentInChildren<SpriteRenderer>();
        }

        /// <summary>
        /// 목표 좌표를 향해 겹치지 않고(Separation) 이동함.
        /// </summary>
        public void MoveTowards(Vector2 targetPosition, float moveSpeed)
        {
            Vector2 currentPosition = transform.position;

            // 1. 타겟을 향하는 기본 이동 벡터
            Vector2 targetDir = (targetPosition - currentPosition).normalized;

            // 2. 주변 유닛 밀어내기 (Separation) 벡터 계산
            Vector2 separationVector = CalculateSeparationVector(currentPosition);

            // 3. 최종 이동 방향 합성 (목표 방향 + 밀어내기 방향)
            Vector2 moveDir = (targetDir + separationVector * separationWeight).normalized;

            // 4. 이동 적용
            transform.Translate(moveDir * moveSpeed * Time.deltaTime);

            // 5. 시각적 처리 (스프라이트 좌우 반전)
            if (spriteRenderer != null && moveDir.x != 0f)
            {
                spriteRenderer.flipX = moveDir.x < 0f;
            }
        }

        /// <summary>
        /// 제자리에 멈춤.
        /// Translate 방식이므로 현재는 빈 구현이며, Rigidbody/NavMesh로 전환 시 이 지점에서 수정함.
        /// </summary>
        public void Stop() { }

        private Vector2 CalculateSeparationVector(Vector2 currentPosition)
        {
            Vector2 separationDir = Vector2.zero;

            int hitCount = Physics2D.OverlapCircleNonAlloc(currentPosition, separationRadius, neighborResults);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = neighborResults[i];
                if (hit == null || hit.transform == transform)
                {
                    continue;
                }

                Vector2 neighborPos        = hit.transform.position;
                Vector2 dirAwayFromNeighbor = currentPosition - neighborPos;
                float distance              = dirAwayFromNeighbor.magnitude;

                if (distance > 0f)
                {
                    separationDir += (dirAwayFromNeighbor.normalized / distance);
                }
                else
                {
                    // 완전히 겹쳤을 때: 랜덤 방향으로 밀어냄
                    separationDir += new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                }
            }

            return separationDir.normalized;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, separationRadius);
        }
#endif
    }
}
