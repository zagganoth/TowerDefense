using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinCollector : MonoBehaviour
{
    [SerializeField] Text uiText;
    private int coinCount;
    // Start is called before the first frame update
    void Start()
    {
        coinCount = 0;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            coinCount++;
            updateCoinCount();
        }
    }
    void updateCoinCount()
    {
        uiText.text = "Coins: " + coinCount;
    }
    public int getCoins()
    {
        return coinCount;
    }
    public void spendCoins(int amount)
    {
        if(amount <= coinCount)
        {
            coinCount -= amount;
            updateCoinCount();
        }

    }
}
