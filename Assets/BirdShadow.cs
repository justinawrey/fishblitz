using UnityEngine;

public class BirdShadow : MonoBehaviour
{
    [SerializeField] private float _idleYPosition = -0.03125f;
    [SerializeField] private float _flyingYPosition = -1.25f;
    [SerializeField] private float _soaringYPosition = -6.25f;
    [SerializeField] private float _idleOpacity = 0.3f;
    [SerializeField] private float _soaringOpacity = 0.03f;
    [SerializeField] private float _shadowTransitionSpeed = 1f;

    private float _targetYPosition;
    private bool _isTransitioning = false;
    private SpriteRenderer _renderer;
    private BirdBrain _bird;
    private IBirdState _currentState = null;
    private IBirdState _previousState = null;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _bird = transform.parent.GetComponent<BirdBrain>();
    }

    private void Update()
    {
        _renderer.sortingOrder = _bird.Renderer.sortingOrder - 1; // shadow below bird

        if (_isTransitioning)
        {
            MoveShadowToTarget();
            InterpolateShadowOpacity();
        }

        _currentState = _bird.BirdState;
        if (_previousState != _currentState)
        {
            _previousState = _currentState;
            UpdateShadowTarget();
        }
    }


    private void MoveShadowToTarget()
    {
        float _delta = _shadowTransitionSpeed * Time.deltaTime;
        float _currentY = transform.localPosition.y;
        float _distanceToTarget = Mathf.Abs(_targetYPosition - _currentY);

        // snap to target
        if (_distanceToTarget <= _delta)
        {
            transform.localPosition = new Vector2(transform.localPosition.x, _targetYPosition);
            _isTransitioning = false;
            return;
        }

        // move delta to target
        transform.localPosition = new Vector2
        (
            transform.localPosition.x,
            _currentY < _targetYPosition ? _currentY + _delta : _currentY - _delta
        );
    }

    private void InterpolateShadowOpacity()
    {
        float _interpolateValue = Normalize(transform.localPosition.y, _idleYPosition, _soaringYPosition);
        _renderer.color = new Color
        (
            _renderer.color.r,
            _renderer.color.g,
            _renderer.color.b,
            Mathf.Clamp01(Mathf.Lerp(_idleOpacity, _soaringOpacity, _interpolateValue))
        );
    }

    private void UpdateShadowTarget()
    {
        // hidden
        if (_currentState is ShelteredState)
        {
            _isTransitioning = false;
            _renderer.enabled = false;
            return;
        }

        _renderer.enabled = true;

        // standing
        if (_currentState is PerchedState || _currentState is GroundedState)
        {
            SetTarget(_idleYPosition);
            return;
        }

        // flying
        if (_currentState is FlyingState || _currentState is LandingState || _currentState is FleeingState)
        {
            SetTarget(_flyingYPosition);
            return;
        }

        // soaring
        if (_currentState is SoaringState || _currentState is SoaringLandingState)
        {
            SetTarget(_soaringYPosition);
            return;
        }

        Debug.LogError("Unexpected state for bird shadow.");
    }

    private void SetTarget(float targetY)
    {
        _targetYPosition = targetY;
        _isTransitioning = true;
    }

    private static float Normalize(float value, float min, float max)
    {
        return (max == min) ? 0f : (value - min) / (max - min);
    }
}