using OysterUtils;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UIElements;

public class ChangeLevel : MonoBehaviour
{
    [SerializeField] private string _toScene;
    [SerializeField] private Vector3 _sceneSpawnLocation;

    private void OnTriggerEnter2D(Collider2D other)
    {
        SmoothSceneManager.LoadScene(_toScene);
        PlayerData.Instance.SceneSpawnPosition = _sceneSpawnLocation;
    }
}
