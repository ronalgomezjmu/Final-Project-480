using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public GameObject scoreManagerObject;
    public GameObject XROriginObject;
    public GameObject SMGPrefab;
    public GameObject ShotGunPrefab;
    public Transform gunSpawnPoint;
    private ScoreManager scoreManager;
    private PlayerDamageEffects healthManager;

    void Start()
    {
        scoreManager = scoreManagerObject.GetComponent<ScoreManager>();
        healthManager = XROriginObject.GetComponent<PlayerDamageEffects>();
    }

    public void buySMG()
    {
        bool purchased = scoreManager.MakePurchase(50);
        if (purchased)
        {
            Instantiate(SMGPrefab, gunSpawnPoint.position, gunSpawnPoint.rotation);
        }
    }

    public void buyHeal()
    {
        bool purchased = scoreManager.MakePurchase(20);
        if (purchased)
        {
            healthManager.Heal();
        }
    }

    public void buyShotGun()
    {
        bool purchased = scoreManager.MakePurchase(50);
        if (purchased)
        {
            Instantiate(ShotGunPrefab, gunSpawnPoint.position, gunSpawnPoint.rotation);
        }
    }
}
