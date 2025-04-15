using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public GameObject scoreManagerObject;
    private ScoreManager scoreManager;

    void Start()
    {
        scoreManager = scoreManagerObject.GetComponent<ScoreManager>();
    }

    public void tryPurchase(int cost)
    {
        Debug.Log("CLICK!!!!!!!!!!!!!!!!!!");
        bool purchased = scoreManager.MakePurchase(cost);
        Debug.Log(purchased);
    }


}
