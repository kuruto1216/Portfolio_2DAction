using UnityEngine;

public class ItemManager : MonoBehaviour
{
    GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.Find("GameSystems").GetComponent<GameManager>();
    }

    public void GetItem()
    {
        gameManager.AddScore();
        Destroy(this.gameObject);
    }
}
