using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    public GameObject bulletTamplate;
    public Transform firePoint;

    public int ammoCount = 10;
    private int currentAmmoCount;

    public float timeBetweenShots = 1f;
    private float shotCooldownRemaining = 0;

    public GameObject reloadingText;
    public Slider reloadProgress;
    public float reloadTime = 2f;
    private float reloadCooldownRemaining = 0;

    public bool isAutomatic = false;
    private bool isShooting = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentAmmoCount = ammoCount;
    }

    void Update()
    {
        if (shotCooldownRemaining > 0)
        {
            shotCooldownRemaining -= Time.deltaTime;
        }

        if (reloadCooldownRemaining > 0)
        {
            reloadCooldownRemaining -= Time.deltaTime;
            reloadProgress.value = (reloadTime - reloadCooldownRemaining) / reloadTime;
            if (reloadCooldownRemaining <= 0)
            {
                endReload();
            }
        }

        if (isShooting)
        {
            fireBullet();
            if (!isAutomatic)
            {
                isShooting = false;
            }
        }

    }

    private void startReload()
    {
        reloadCooldownRemaining = reloadTime;
        reloadingText.SetActive(true);
        reloadProgress.gameObject.SetActive(true);
    }

    public void endReload()
    {
        reloadingText.SetActive(false);
        reloadProgress.gameObject.SetActive(false);
        currentAmmoCount = ammoCount;
    }

    public void shoot()
    {
        isShooting = true;
    }

    public void stopShooting()
    {
        isShooting = false;
    }

    private void fireBullet()
    {
        if (currentAmmoCount > 0 && shotCooldownRemaining <= 0)
        {
            GameObject bullet = Instantiate(bulletTamplate, firePoint.position, firePoint.rotation);
            Rigidbody rigidbody = bullet.GetComponent<Rigidbody>();
            rigidbody.AddForce(firePoint.up * 5000f);
            shotCooldownRemaining = timeBetweenShots;
            currentAmmoCount -= 1;

            if (currentAmmoCount == 0)
            {
                startReload();
            }
        }
    }
}
