using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Launcher._Project.Scripts;
using _Project.Scripts.Planet;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace _Project.Scripts.Launcher
{
    //TODO: Split this class into MVC
    public class BallLauncher : MonoBehaviour
    {
        [SerializeField] private float _launchForce = 10f;
        [SerializeField] private Transform _launchPoint;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private float _maxDragDistance = 5f;
        [SerializeField] private float _trajectoryTimeStep = 0.02f;
        [SerializeField] private float _maxTrajectoryTime = 3f;
        [SerializeField] private LayerMask _collisionMask;
        [SerializeField] private float _ballSpawnDelay = 0.3f;
        [SerializeField] private int _maxBalls = 10;
        [SerializeField] private TMP_Text _ballsLeftText;

        private Vector2 _startTouchPosition;
        private Vector2 _currentTouchPosition;
        private bool _isDragging = false;
        private Camera _mainCamera;
        private int _ballsLeft;

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
            _ballsLeft = _maxBalls;
            UpdateBallsLeftText();
            SpawnNextBall();
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
            if (_ballsLeft <= 0) return;

            _currentBall = _ballFactory.Create();
            _currentBall.transform.position = _launchPoint.position;

            var availableMaterials = _planetModel.UniqueMaterials;
            if (availableMaterials.Count > 0)
            {
                var randomIndex = Random.Range(0, availableMaterials.Count);
                _currentBall.SetMaterial(availableMaterials[randomIndex]);
            }

            if (_lineRenderer != null)
            {
                var ballRenderer = _currentBall.GetComponent<Renderer>();
                if (ballRenderer != null)
                {
                    var ballColor = ballRenderer.material;
                    _lineRenderer.material = ballColor;
                }
            }

            var rb = _currentBall.GetComponent<Rigidbody>();
            rb.isKinematic = true;
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
            var ray = _mainCamera.ScreenPointToRay(new Vector3(screenPosition.x, screenPosition.y, 0));
            var plane = new Plane(Vector3.forward, _launchPoint.position);

            if (!plane.Raycast(ray, out var distance)) return screenPosition;
            var worldPosition = ray.GetPoint(distance);
            return new Vector2(worldPosition.x, worldPosition.y);
        }

        private void UpdateTrajectory()
        {
            if (_lineRenderer == null || _currentBall == null) return;

            var dragVector = _currentTouchPosition - _startTouchPosition;
            var dragDistance = Mathf.Min(dragVector.magnitude, _maxDragDistance);
            var dragDirection = dragVector.normalized;

            var initialVelocity = new Vector3(
                dragDirection.x,
                dragDirection.y,
                -1
            ) * (_launchForce * (dragDistance / _maxDragDistance));

            var trajectoryPoints = new List<Vector3>();
            var position = _launchPoint.position;
            var velocity = initialVelocity;
            var timeStep = _trajectoryTimeStep;
            var currentTime = 0f;

            while (currentTime < _maxTrajectoryTime)
            {
                trajectoryPoints.Add(position);

                velocity += Physics.gravity * timeStep;
                var nextPosition = position + velocity * timeStep;

                if (Physics.Linecast(position, nextPosition, out var hit, _collisionMask))
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

            var dragVector = _currentTouchPosition - _startTouchPosition;
            var dragDistance = Mathf.Min(dragVector.magnitude, _maxDragDistance);
            var dragDirection = dragVector.normalized;

            var ballRigidbody = _currentBall.GetComponent<Rigidbody>();
            ballRigidbody.isKinematic = false;

            var launchVelocity = new Vector3(
                dragDirection.x,
                dragDirection.y,
                -1
            ) * (_launchForce * (dragDistance / _maxDragDistance));

            ballRigidbody.linearVelocity = launchVelocity;
            _ballsLeft--;
            UpdateBallsLeftText();
            _currentBall = null;


            if (_ballsLeft > 0)
            {
                StartCoroutine(DelayedBallSpawn());
            }
        }

        private IEnumerator DelayedBallSpawn()
        {
            yield return new WaitForSeconds(_ballSpawnDelay);
            SpawnNextBall();
        }
    }
}