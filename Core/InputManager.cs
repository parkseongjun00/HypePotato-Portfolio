using UnityEngine;
using UnityEngine.InputSystem;

namespace HypePotato.Core
{
    /// <summary>
    /// 게임 내 모든 터치, 클릭, 드래그 입력을 중앙에서 감지하고 처리하는 통합 매니저.
    /// 마우스와 모바일 터치를 통합 지원하며 싱글톤으로 제공됨.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        #region Inspector Fields
        [Header("Drag Settings")]
        [Tooltip("Minimum pointer travel distance in pixels to be recognized as a drag. Prevents tap events from firing during drag.")]
        [SerializeField] private float dragThreshold = 8f;
        #endregion

        #region Public Properties
        /// <summary>
        /// 현재 드래그가 진행 중인지 여부.
        /// </summary>
        public bool IsDragging { get; private set; }

        /// <summary>
        /// 이번 프레임의 드래그 이동 델타값. (스크린 좌표계 픽셀 기준)
        /// </summary>
        public Vector2 DragDelta { get; private set; }
        #endregion

        #region Private Fields
        private Camera mainCamera;
        private Vector2 pressStartScreenPos;
        private Vector2 prevFrameScreenPos;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Pointer.current == null)
            {
                return;
            }

            HandleInput();
        }
        #endregion

        #region Input Processing
        private void HandleInput()
        {
            Vector2 currentScreenPos = Pointer.current.position.ReadValue();
            DragDelta = Vector2.zero;

            // 누름 시작: 입력 위치 초기화
            if (Pointer.current.press.wasPressedThisFrame)
            {
                pressStartScreenPos = currentScreenPos;
                prevFrameScreenPos  = currentScreenPos;
                IsDragging          = false;
            }

            // 누름 유지 중: 임계값 체크 후 드래그 진행
            if (Pointer.current.press.isPressed)
            {
                if (!IsDragging)
                {
                    float movedPixels = Vector2.Distance(currentScreenPos, pressStartScreenPos);
                    if (movedPixels >= dragThreshold)
                    {
                        IsDragging         = true;
                        prevFrameScreenPos = currentScreenPos; // 스냅 방지
                    }
                }

                if (IsDragging)
                {
                    DragDelta          = currentScreenPos - prevFrameScreenPos;
                    prevFrameScreenPos = currentScreenPos;
                }
            }

            // 누름 해제: 드래그가 아니었을 때만 탭(Tap) 이벤트 레이캐스트 처리
            if (Pointer.current.press.wasReleasedThisFrame)
            {
                if (!IsDragging)
                {
                    HandleTouchcast(pressStartScreenPos);
                }
                IsDragging = false;
            }
        }

        private void HandleTouchcast(Vector2 screenPosition)
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null)
            {
                return;
            }

            Vector2 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.TryGetComponent(out ITouchable touchable))
                {
                    touchable.OnReceiveTouch();
                }
            }
        }
        #endregion
    }
}
