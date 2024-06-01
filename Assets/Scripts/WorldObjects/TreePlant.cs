using System;
using System.Collections;
using DG.Tweening;
using ReactiveUnity;
using UnityEngine;

public enum TreeStates { SummerAdult, FallAdult, DeadAdult, Stump };
public abstract class TreePlant : MonoBehaviour, IInteractable, IUseableWithAxe
{

    [Header("Tree Base State")]
    [SerializeField] protected Material _standardLit;
    [SerializeField] protected Material _windyTree;
    [SerializeField] protected Sprite _summerAdult;
    [SerializeField] protected Sprite _fallAdult;
    [SerializeField] protected Sprite _deadAdult;
    [SerializeField] protected Reactive<TreeStates> _treeState = new Reactive<TreeStates>(TreeStates.SummerAdult);

    [Header("Fallen Tree")]
    [SerializeField] protected GameObject _E_fallenTree;
    [SerializeField] protected GameObject _W_fallenTree;
    [SerializeField] protected GameObject _stump;

    [Header("Shake Properties")]
    [SerializeField] float _shakeDuration = 0.5f;
    [SerializeField] float _shakeStrength = 0.05f;
    [SerializeField] int _shakeVibrato = 10;
    [SerializeField] float _shakeRandomness = 90f;

    protected SpriteRenderer _spriteRenderer;
    private PlayerMovementController _playerMovementController;
    protected Action _unsubscribe;

    protected const int _HITS_TO_FALL = 5;
    protected int _hitCount = 0;

    protected virtual void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playerMovementController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementController>();
        OnStateChange();
    }
    protected virtual void OnEnable()
    {
        _unsubscribe = _treeState.OnChange((prev, curr) => OnStateChange());
    }
    protected virtual void OnDisable()
    {
        _unsubscribe();
    }

    protected virtual void OnStateChange()
    {
        switch (_treeState.Value)
        {
            case TreeStates.SummerAdult:
                _spriteRenderer.material = _windyTree;
                _spriteRenderer.sprite = _summerAdult;
                break;
            case TreeStates.FallAdult:
                _spriteRenderer.material = _windyTree;
                _spriteRenderer.sprite = _fallAdult;
                break;
            case TreeStates.DeadAdult:
                _spriteRenderer.material = _standardLit;
                _spriteRenderer.sprite = _deadAdult;
                break;
            default:
                Debug.LogError("TreePlant state machine defaulted");
                break;
        }
    }

    public void OnUseAxe()
    {
        if (_hitCount < _HITS_TO_FALL - 1)
        {
            _hitCount++;
            transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, _shakeRandomness);
            return;
        }
        StartCoroutine(FallTree());
    }

    /// <summary>
    /// Spawns a stump and a falling tree. Deletes the standing tree (this object instance).
    /// </summary>
    IEnumerator FallTree()
    {
        // Wait for axing animation to finish
        yield return new WaitUntil(() => _playerMovementController.PlayerState.Value != PlayerStates.Axing);

        // Determine which tree prefab to select
        bool _fallsEast = WillTreeFallEast();
        GameObject _treeToFall = _fallsEast ? _E_fallenTree : _W_fallenTree;

        // Falls a hand-picked distance to the side of the stump
        Vector3 _fallenTreePosition = _fallsEast ? new Vector3(6f, 1f, 0) : new Vector3(-0.5f, 1f, 0);

        // Instantiating
        GameObject _larchStump = UnityEngine.Object.Instantiate(_stump,
                                                                transform.position,
                                                                Quaternion.identity,
                                                                GameObject.FindGameObjectWithTag("Impermanent").transform);

        GameObject _fallenTree = UnityEngine.Object.Instantiate(_treeToFall,
                                                                transform.position + _fallenTreePosition, // falls some distance to the side of stump
                                                                Quaternion.identity,
                                                                GameObject.FindGameObjectWithTag("Impermanent").transform);

        // Want falling tree to appear infront of stump.
        // FallingTree.cs re-enables sprite sorting after falling
        _fallenTree.GetComponentInChildren<StaticSpriteSorting>().enabled = false;
        _fallenTree.GetComponentInChildren<SpriteRenderer>().sortingOrder = _spriteRenderer.sortingOrder + 1;
        Destroy(gameObject);
    }

    /// <summary>
    /// Determines tree fall direction
    /// </summary>
    private bool WillTreeFallEast()
    {
        switch (_playerMovementController.FacingDirection.Value)
        {
            case FacingDirections.West:
                return false;
            case FacingDirections.East:
                return true;
            case FacingDirections.North:
            case FacingDirections.South:
                return UnityEngine.Random.value < 0.5;
            default:
                Debug.LogError("DoesTreeFallEast defaulted.");
                return false;
        }
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        return false;
    }
}