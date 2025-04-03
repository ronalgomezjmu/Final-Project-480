using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public GameObject weapon;
    public GameObject controller;

    void Start()
    {
        Transform handTransform = controller.GetComponent<Transform>();
        weapon.transform.SetParent(handTransform);
        weapon.transform.localPosition = new Vector3(0, -0.025f, 0);
    }

    public void fireWeapon()
    {
        Debug.Log("SHOOT!");
    }

}
