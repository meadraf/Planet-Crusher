using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Launcher._Project.Scripts;
using _Project.Scripts.Planet;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.Launcher
{
    public class BallLauncher : MonoBehaviour
    {
        [SerializeField] private float _launchForce = 10f;
        [SerializeField] private Transform _launchPoint;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private float _maxDragDistance = 5f;
        [SerializeField] private float _trajectoryTimeStep = 0.02f;
        [SerializeField] private float _maxTrajectoryTime = 3f;
        [SerializeField] private LayerMask _collisionMask;
        [SerializeField] private float _ballSpawnDelay = 0.3f; // Delay between ball spawns
        [SerializeField] private int _maxBalls = 10; // Maximum number of balls
        [SerializeField] private TMP_Text _ballsLeftText; // UI Text to display remaining balls

        private Vector2 _startTouchPosition;
        private Vector2 _currentTouchPosition;
        private bool _isDragging = false;
        private Camera _mainCamera;
        private int _ballsLeft; // Counter for remaining balls

        private Ball.Factory _ballFactory;
        private GameplayInput _input;
        private PlanetModel _planetModel;
        private Ball _currentBall;

        [Inject]
        public void Construct(Ball.Factory ballFactory, PlanetModel planetModel)
        {
            _ballFactory = ballFactory;
            _planetModel = planetModel;
        }

        private void Start()
        {
            _ballsLeft = _maxBalls; // Initialize the ball counter
            UpdateBallsLeftText(); // Update the UI text
            SpawnNextBall(); // Spawn the first ball
        }

        private void Awake()
        {
            _input = new GameplayInput();
            _mainCamera = Camera.main;

            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = false;
                _lineRenderer.useWorldSpace = true;
            }
        }

        private void OnEnable()
        {
            _input.Enable();
            _input.Gameplay.Press.performed += OnTouchPerformed;
            _input.Gameplay.Press.canceled += OnTouchCanceled;
            _input.Gameplay.TouchPosition.performed += TouchPosition;
        }

        private void OnDisable()
        {
            _input.Disable();
            _input.Gameplay.Press.performed -= OnTouchPerformed;
            _input.Gameplay.Press.canceled -= OnTouchCanceled;
            _input.Gameplay.TouchPosition.performed -= TouchPosition;
        }

        private void SpawnNextBall()
        {
            if (_ballsLeft <= 0) return; // Don't spawn if no balls are left

            _currentBall = _ballFactory.Create();
            _currentBall.transform.position = _launchPoint.position;

            // Assign random material
            var availableMaterials = _planetModel.UniqueMaterials;
            if (availableMaterials.Count > 0)
            {
                int randomIndex = Random.Range(0, availableMaterials.Count);
                _currentBall.SetMaterial(availableMaterials[randomIndex]);
            }

            // Set line color to match the new ball
            if (_lineRenderer != null)
            {
                Renderer ballRenderer = _currentBall.GetComponent<Renderer>();
                if (ballRenderer != null)
                {
                    var ballColor = ballRenderer.material;
                    _lineRenderer.material = ballColor;
                }
            }

            // Disable physics until launch
            Rigidbody rb = _currentBall.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            
             // Update the UI text
        }

        private void UpdateBallsLeftText()
        {
            if (_ballsLeftText != null)
            {
                _ballsLeftText.text = $"{_ballsLeft}";
            }
        }

        private void OnTouchPerformed(InputAction.CallbackContext context)
        {
            if (!_isDragging && _currentBall != null && _ballsLeft >= 0)
            {
                _isDragging = true;
                _startTouchPosition = GetWorldTouchPosition(_input.Gameplay.TouchPosition.ReadValue<Vector2>());
                _currentTouchPosition = _startTouchPosition;

                if (_lineRenderer != null)
                {
                    _lineRenderer.enabled = true;
                }
                UpdateTrajectory();
            }
        }

        private void OnTouchCanceled(InputAction.CallbackContext context)
        {
            if (_isDragging)
            {
                _isDragging = false;
                LaunchBall();

                if (_lineRenderer != null)
                {
                    _lineRenderer.enabled = false;
                }
            }
        }

        private void TouchPosition(InputAction.CallbackContext context)
        {
            if (_isDragging)
            {
                _currentTouchPosition = GetWorldTouchPosition(context.ReadValue<Vector2>());
                UpdateTrajectory();
            }
        }

        private Vector2 GetWorldTouchPosition(Vector2 screenPosition)
        {
            Ray ray = _mainCamera.ScreenPointToRay(new Vector3(screenPosition.x, screenPosition.y, 0));
            Plane plane = new Plane(Vector3.forward, _launchPoint.position);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 worldPosition = ray.GetPoint(distance);
                return new Vector2(worldPosition.x, worldPosition.y);
            }

            return screenPosition;
        }

        private void UpdateTrajectory()
        {
            if (_lineRenderer == null || _currentBall == null) return;

            Vector2 dragVector = _currentTouchPosition - _startTouchPosition;
            float dragDistance = Mathf.Min(dragVector.magnitude, _maxDragDistance);
            Vector2 dragDirection = dragVector.normalized;

            Vector3 initialVelocity = new Vector3(
                dragDirection.x,
                dragDirection.y,
                -1
            ) * (_launchForce * (dragDistance / _maxDragDistance));

            List<Vector3> trajectoryPoints = new List<Vector3>();
            Vector3 position = _launchPoint.position;
            Vector3 velocity = initialVelocity;
            float timeStep = _trajectoryTimeStep;
            float currentTime = 0f;

            while (currentTime < _maxTrajectoryTime)
            {
                trajectoryPoints.Add(position);

                velocity += Physics.gravity * timeStep;
                Vector3 nextPosition = position + velocity * timeStep;

                RaycastHit hit;
                if (Physics.Linecast(position, nextPosition, out hit, _collisionMask))
                {
                    trajectoryPoints.Add(hit.point);
                    break;
                }

                position = nextPosition;
                currentTime += timeStep;
            }

            _lineRenderer.positionCount = trajectoryPoints.Count;
            _lineRenderer.SetPositions(trajectoryPoints.ToArray());
        }

        private void LaunchBall()
        {
            if (_currentBall == null) return;

            Vector2 dragVector = _currentTouchPosition - _startTouchPosition;
            float dragDistance = Mathf.Min(dragVector.magnitude, _maxDragDistance);
            Vector2 dragDirection = dragVector.normalized;

            Rigidbody ballRigidbody = _currentBall.GetComponent<Rigidbody>();
            ballRigidbody.isKinematic = false;

            Vector3 launchVelocity = new Vector3(
                dragDirection.x,
                dragDirection.y,
                -1
            ) * (_launchForce * (dragDistance / _maxDragDistance));

            ballRigidbody.linearVelocity = launchVelocity;
            _ballsLeft--;
            UpdateBallsLeftText();
            _currentBall = null;

            // Start coroutine to delay next ball spawn
            if (_ballsLeft > 0)
            {
                StartCoroutine(DelayedBallSpawn());
            }
        }

        private IEnumerator DelayedBallSpawn()
        {
            yield return new WaitForSeconds(_ballSpawnDelay); // Wait for the specified delay
            SpawnNextBall(); // Spawn the next ball after the delay
        }
    }
}