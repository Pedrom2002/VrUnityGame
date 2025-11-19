using UnityEngine;

namespace VRAimLab.Gameplay
{
    /// Movement patterns for moving targets
    public enum MovementPattern
    {
        Linear,     // Straight line movement
        Circular,   // Circle around a point
        Zigzag,     // Zigzag pattern
        Random,     // Random direction changes
        Figure8,    // Figure-8 pattern
        Bounce      // Bounce between points
    }

    /// Moving Target - extends base Target with movement capabilities
    /// Supports 6 different movement patterns
    public class MovingTarget : Target
    {
        #region Inspector Fields
        [Header("Movement Settings")]
        [SerializeField] private MovementPattern movementPattern = MovementPattern.Linear;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float patternScale = 3f; // Scale of movement pattern
        [SerializeField] private bool randomizePattern = false;

        [Header("Linear Movement")]
        [SerializeField] private Vector3 moveDirection = Vector3.right;

        [Header("Circular Movement")]
        [SerializeField] private Vector3 circleCenter;
        [SerializeField] private float circleRadius = 2f;
        [SerializeField] private bool clockwise = true;

        [Header("Zigzag Movement")]
        [SerializeField] private float zigzagAmplitude = 2f;
        [SerializeField] private float zigzagFrequency = 2f;

        [Header("Random Movement")]
        [SerializeField] private float directionChangeInterval = 1f;
        [SerializeField] private Vector3 movementBounds = new Vector3(5f, 3f, 5f);

        [Header("Ground Constraints")]
        [SerializeField] private float minHeight = 0.5f; // Minimum Y position to prevent going underground
        [SerializeField] private bool enforceGroundLimit = true;

        [Header("Bounce Movement")]
        [SerializeField] private Transform[] bouncePoints;
        #endregion

        #region Private Fields
        private Vector3 startPosition;
        private float movementTimer;
        private float directionChangeTimer;
        private Vector3 currentDirection;
        private int currentBounceIndex = 0;
        private float circleAngle = 0f;
        #endregion

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();

            if (randomizePattern)
            {
                movementPattern = (MovementPattern)Random.Range(0, System.Enum.GetValues(typeof(MovementPattern)).Length);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InitializeMovement();
        }

        protected override void Update()
        {
            base.Update();

            if (!isActive) return;

            UpdateMovement();
        }
        #endregion

        #region Initialization
        /// Initialize movement parameters
        private void InitializeMovement()
        {
            startPosition = transform.position;
            movementTimer = 0f;
            directionChangeTimer = 0f;
            currentDirection = moveDirection.normalized;
            currentBounceIndex = 0;
            circleAngle = 0f;

            // Set circle center to start position if not set
            if (circleCenter == Vector3.zero)
            {
                circleCenter = startPosition;
            }

            // Generate random direction for random movement
            if (movementPattern == MovementPattern.Random)
            {
                GenerateRandomDirection();
            }
        }

        /// Reset target and movement
        public override void ResetTarget()
        {
            base.ResetTarget();
            InitializeMovement();
        }
        #endregion

        #region Movement Update
        /// Update target movement based on pattern
        private void UpdateMovement()
        {
            movementTimer += Time.deltaTime;

            switch (movementPattern)
            {
                case MovementPattern.Linear:
                    UpdateLinearMovement();
                    break;

                case MovementPattern.Circular:
                    UpdateCircularMovement();
                    break;

                case MovementPattern.Zigzag:
                    UpdateZigzagMovement();
                    break;

                case MovementPattern.Random:
                    UpdateRandomMovement();
                    break;

                case MovementPattern.Figure8:
                    UpdateFigure8Movement();
                    break;

                case MovementPattern.Bounce:
                    UpdateBounceMovement();
                    break;
            }
        }
        #endregion

        #region Movement Patterns
        /// Linear movement in one direction
        private void UpdateLinearMovement()
        {
            transform.Translate(currentDirection * moveSpeed * Time.deltaTime, Space.World);
            transform.position = ClampToGround(transform.position);
        }

        /// Circular movement around a center point
        private void UpdateCircularMovement()
        {
            float angleSpeed = moveSpeed / circleRadius;
            circleAngle += angleSpeed * Time.deltaTime * (clockwise ? 1f : -1f);

            float x = circleCenter.x + Mathf.Cos(circleAngle) * circleRadius;
            float z = circleCenter.z + Mathf.Sin(circleAngle) * circleRadius;
            float y = circleCenter.y;

            Vector3 newPosition = new Vector3(x, y, z);
            transform.position = ClampToGround(newPosition);
        }

        /// Zigzag movement pattern
        private void UpdateZigzagMovement()
        {
            float zigzagOffset = Mathf.Sin(movementTimer * zigzagFrequency) * zigzagAmplitude;

            Vector3 forwardMovement = currentDirection * moveSpeed * Time.deltaTime;
            Vector3 sideMovement = Vector3.Cross(currentDirection, Vector3.up).normalized * zigzagOffset * Time.deltaTime;

            transform.Translate(forwardMovement + sideMovement, Space.World);
            transform.position = ClampToGround(transform.position);
        }

        /// Random movement with direction changes
        private void UpdateRandomMovement()
        {
            directionChangeTimer += Time.deltaTime;

            if (directionChangeTimer >= directionChangeInterval)
            {
                GenerateRandomDirection();
                directionChangeTimer = 0f;
            }

            Vector3 newPosition = transform.position + currentDirection * moveSpeed * Time.deltaTime;

            // Keep within bounds
            newPosition.x = Mathf.Clamp(newPosition.x, startPosition.x - movementBounds.x, startPosition.x + movementBounds.x);
            newPosition.y = Mathf.Clamp(newPosition.y, startPosition.y - movementBounds.y, startPosition.y + movementBounds.y);
            newPosition.z = Mathf.Clamp(newPosition.z, startPosition.z - movementBounds.z, startPosition.z + movementBounds.z);

            // Apply ground constraint
            newPosition = ClampToGround(newPosition);

            transform.position = newPosition;

            // Bounce off bounds
            if (Mathf.Abs(newPosition.x - startPosition.x) >= movementBounds.x)
                currentDirection.x *= -1f;
            if (Mathf.Abs(newPosition.y - startPosition.y) >= movementBounds.y)
                currentDirection.y *= -1f;
            if (Mathf.Abs(newPosition.z - startPosition.z) >= movementBounds.z)
                currentDirection.z *= -1f;
        }

        /// Figure-8 movement pattern
        private void UpdateFigure8Movement()
        {
            float t = movementTimer * moveSpeed * 0.5f;

            float x = patternScale * Mathf.Sin(t);
            float y = 0f; // Relative offset from start position
            float z = patternScale * Mathf.Sin(t * 2f) * 0.5f;

            Vector3 newPosition = startPosition + new Vector3(x, y, z);
            transform.position = ClampToGround(newPosition);
        }

        /// Bounce between predefined points
        private void UpdateBounceMovement()
        {
            if (bouncePoints == null || bouncePoints.Length < 2)
            {
                // Fallback to linear if no bounce points
                UpdateLinearMovement();
                return;
            }

            Transform targetPoint = bouncePoints[currentBounceIndex];
            Vector3 direction = (targetPoint.position - transform.position).normalized;

            Vector3 newPosition = transform.position + direction * moveSpeed * Time.deltaTime;
            transform.position = ClampToGround(newPosition);

            // Check if reached target point
            if (Vector3.Distance(transform.position, targetPoint.position) < 0.5f)
            {
                currentBounceIndex = (currentBounceIndex + 1) % bouncePoints.Length;
            }
        }
        #endregion

        #region Helper Methods
        /// Clamp position to prevent going underground
        private Vector3 ClampToGround(Vector3 position)
        {
            if (enforceGroundLimit && position.y < minHeight)
            {
                position.y = minHeight;
            }
            return position;
        }

        /// Generate a new random direction
        private void GenerateRandomDirection()
        {
            currentDirection = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-0.5f, 0.5f),
                Random.Range(-1f, 1f)
            ).normalized;
        }

        /// Change movement pattern at runtime
        public void SetMovementPattern(MovementPattern pattern)
        {
            movementPattern = pattern;
            InitializeMovement();
        }

        /// Change movement speed at runtime
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }

        /// Set circle center for circular movement
        public void SetCircleCenter(Vector3 center)
        {
            circleCenter = center;
        }

        /// Set bounce points for bounce movement
        public void SetBouncePoints(Transform[] points)
        {
            bouncePoints = points;
            currentBounceIndex = 0;
        }
        #endregion

        #region Editor Helpers
#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Draw movement pattern preview
            Gizmos.color = Color.cyan;

            switch (movementPattern)
            {
                case MovementPattern.Linear:
                    Vector3 start = Application.isPlaying ? startPosition : transform.position;
                    Gizmos.DrawRay(start, moveDirection.normalized * 5f);
                    break;

                case MovementPattern.Circular:
                    Vector3 center = circleCenter == Vector3.zero ? transform.position : circleCenter;
                    DrawCircle(center, circleRadius, 32);
                    break;

                case MovementPattern.Random:
                    Vector3 boundsStart = Application.isPlaying ? startPosition : transform.position;
                    Gizmos.DrawWireCube(boundsStart, movementBounds * 2f);
                    break;

                case MovementPattern.Bounce:
                    if (bouncePoints != null && bouncePoints.Length > 1)
                    {
                        for (int i = 0; i < bouncePoints.Length; i++)
                        {
                            if (bouncePoints[i] != null)
                            {
                                int nextIndex = (i + 1) % bouncePoints.Length;
                                if (bouncePoints[nextIndex] != null)
                                {
                                    Gizmos.DrawLine(bouncePoints[i].position, bouncePoints[nextIndex].position);
                                }
                                Gizmos.DrawWireSphere(bouncePoints[i].position, 0.3f);
                            }
                        }
                    }
                    break;
            }
        }

        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
#endif
        #endregion
    }
}
