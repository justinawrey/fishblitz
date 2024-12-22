using UnityEngine;

[CreateAssetMenu(fileName = "NewBinoculars", menuName = "Items/Binoculars")]
public class Binoculars : Inventory.ItemType, PlayerInteractionManager.ITool
{
    private BirdingGame _birdingGame;
    private PlayerMovementController _playerMovementController;

    private void Awake() {
        _playerMovementController = GameObject.FindWithTag("Player").GetComponent<PlayerMovementController>();
        _birdingGame = GameObject.FindWithTag("Player").GetComponentInChildren<BirdingGame>(true);
    }

    public bool UseToolOnWorldObject(PlayerInteractionManager.IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        return false;
    }

    public bool UseToolOnInteractableTileMap(string tilemapLayerName, Vector3Int cursorLocation)
    {
        return false;
    }

    public void SwingTool()
    {
        Debug.Log("Binoculars swung");
        if (_playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Birding) {
            Debug.Log("Binoculars returned");
            return;
        }

        _playerMovementController.PlayerState.Value = PlayerMovementController.PlayerStates.Birding;
        _birdingGame.Play();
    }

    public void PlayToolHitSound()
    {
        // no sound
    }
}
