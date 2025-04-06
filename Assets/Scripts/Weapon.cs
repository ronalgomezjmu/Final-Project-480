using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject bulletTempalte;
    public Transform firePoint;

    public int ammoCount = 10;
    private int currentAmmoCount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentAmmoCount = ammoCount;
    }

    public void shoot()
    {
        GameObject bullet = Instantiate(bulletTempalte, firePoint.position, firePoint.rotation);
        Rigidbody rigidbody = bullet.GetComponent<Rigidbody>();
        rigidbody.AddForce(firePoint.up * 5000f);
    }
}
