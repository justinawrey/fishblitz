using System;
using UnityEngine;
using UnityEngine.InputSystem;
using ReactiveUnity;

public enum TradeType
{
    Key,
    Rod
}

public class Store : MonoBehaviour
{
    [SerializeField] private InputActionReference _action;
    [SerializeField] private SpriteRenderer _storeSpriteRenderer;
    [SerializeField] private Sprite _outlineSprite;
    [SerializeField] private Sprite _nonOutlineSprite;
    [SerializeField] private Sprite _soldOutSprite;
    [SerializeField] private TradeType _tradeType = TradeType.Rod;
    [SerializeField] private int _totalInventory = 1;
    [SerializeField] private int _keyCost = 10;
    [SerializeField] private int _rodCost = 1;

    private Inventory _inventory;
    private Reactive<bool> _inRange = new Reactive<bool>(false);

    private void Awake()
    {
        _inventory = GameObject.FindWithTag("Inventory").GetComponent<Inventory>();
        _inRange.OnChange((_, inRange) => SetOutline(inRange));
        SetOutline(false);
    }

    private void SetOutline(bool inRange)
    {
        if (IsSoldOut())
        {
            return;
        }

        _storeSpriteRenderer.sprite = inRange ? _outlineSprite : _nonOutlineSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        _inRange.Value = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _inRange.Value = false;
    }

    private void Update()
    {
        if (IsSoldOut())
        {
            return;
        }

        if (!_inRange.Value)
        {
            return;
        }

        if (!_action.action.WasPressedThisFrame())
        {
            return;
        }

        if (_tradeType == TradeType.Rod)
        {
            MakeRodTrade();
        }
        else if (_tradeType == TradeType.Key)
        {
            MakeKeyTrade();
        }
    }

    // Keys cost 10 money
    private void MakeKeyTrade()
    {
        if (IsSoldOut())
        {
            return;
        }

        if (_inventory.Gold < _keyCost)
        {
            return;
        }

        _inventory.Gold -= _keyCost;
        _inventory.TryAddItem("Key", 1);
        _totalInventory -= 1;

        if (IsSoldOut())
        {
            _storeSpriteRenderer.sprite = _soldOutSprite;
        }
    }

    // Rods cost 1 money
    private void MakeRodTrade()
    {
        if (IsSoldOut())
        {
            return;
        }

        if (_inventory.Gold <= 0)
        {
            return;
        }

        _inventory.Gold -= _rodCost;
        _inventory.TryAddItem("MountedRod", 1);
        _totalInventory -= 1;

        if (IsSoldOut())
        {
            _storeSpriteRenderer.sprite = _soldOutSprite;
        }
    }

    private bool IsSoldOut()
    {
        return _totalInventory <= 0;
    }
}
