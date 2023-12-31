using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using ReactiveUnity;
using UnityEngine.Tilemaps;

public class PlacedMountedRod : MonoBehaviour, ICursorInteractableObject
{
    [Header("Sprite Options")]
    [SerializeField] private Sprite _onSprite;
    [SerializeField] private Sprite _offSprite;
    [SerializeField] private Sprite _onSelectedSprite;
    [SerializeField] private Sprite _offSelectedSprite;

    [Header("Behavioural Options")]
    [SerializeField] private float _minChangeInterval = 3;
    [SerializeField] private float _maxChangeInterval = 10;

    [Header("Shake Options")]
    [SerializeField] private float _shakeDuration = 1;
    [SerializeField] private float _shakeStrength = 1;
    [SerializeField] private int _shakeVibrato = 10;
    [SerializeField] private float _shakeRandomness = 90;
    private Inventory _inventory;
    private SpriteRenderer _spriteRenderer;
    private Reactive<bool> _fishOn = new Reactive<bool>(false);
    private Reactive<bool> _selected = new Reactive<bool>(false);
    private FishBar _fishBar;
    private Coroutine _changeStateRoutine;
    private ActiveGridCell _activeGridCell;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _fishOn.OnChange((_, curr) => ChangeSprite(curr, _selected.Value));
        _fishOn.OnChange((_, curr) => Shake());
        _selected.OnChange((_, selected) => ChangeSprite(_fishOn.Value, selected));
        ChangeSprite(_fishOn.Value, _selected.Value);
    }

    private void Shake()
    {
        transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, _shakeRandomness);
    }

    private void Start()
    {
        _activeGridCell = GameObject.FindWithTag("ActiveGridCell").GetComponent<ActiveGridCell>();
        _inventory = GameObject.FindWithTag("InventoryContainer").GetComponent<Inventory>();
        _fishBar = GameObject.FindWithTag("Player").GetComponentInChildren<FishBar>(true);
        _changeStateRoutine = StartCoroutine(ChangeStateRoutine());
    }

    private IEnumerator ChangeStateRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_minChangeInterval, _maxChangeInterval));
            _fishOn.Value = !_fishOn.Value;
        }
    }

    private void ChangeSprite(bool fishOn, bool selected)
    {
        if (fishOn)
        {
            _spriteRenderer.sprite = selected ? _onSelectedSprite : _onSprite;
        }
        else
        {
            _spriteRenderer.sprite = selected ? _offSelectedSprite : _offSprite;
        }
    }

    private void Update()
    {
        List<Collider2D> _results = new List<Collider2D>();
        Physics2D.OverlapBox(_activeGridCell.GetActiveCursorLocation() + new Vector3(0.5f, 0.5f, 0f), new Vector2(1, 1), 0, new ContactFilter2D().NoFilter(), _results);

        bool _found = false;
        foreach (var _result in _results)
        {
            var _rod = _result.gameObject.GetComponent<PlacedMountedRod>();
            if (_rod == this)
            {
                _found = true;
                _selected.Value = true;
            }
        }

        if (!_found)
        {
            _selected.Value = false;
        }
    }

    public void CursorAction(TileData tileData, Vector3 cursorLocation)
    {
        if (!_fishOn.Value)
        {
            Shake();
            return;
        }

        StopCoroutine(_changeStateRoutine);
        _inventory.AddItem("MountedRod", 1);
        _fishBar.Play();
        Destroy(gameObject);
    }
}