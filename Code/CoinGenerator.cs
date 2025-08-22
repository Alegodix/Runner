using UnityEngine;

public class CoinGenerator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private int amoutOfCoins;
    [SerializeField] private GameObject coinPrefab;

    [SerializeField] private int minCoins;
    [SerializeField] private int maxCoins;
    [SerializeField] private float chanceToSpawn;

    [SerializeField] private SpriteRenderer[] coinImg;
    void Start()
    {

        for (int i = 0; i < coinImg.Length; i++) { 
            coinImg[i].sprite = null;
        }

        amoutOfCoins = Random.Range(minCoins, maxCoins);
        int additionalOffset = amoutOfCoins / 2;

        for (int i = 0; i < amoutOfCoins; i++)
        {
            Vector3 offset = new Vector2(i - additionalOffset, 0);
            Instantiate(coinPrefab, transform.position + offset, Quaternion.identity, transform);
        }   
    }
}
