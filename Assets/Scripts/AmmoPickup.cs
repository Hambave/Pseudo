using UnityEngine;

public class AmmoPickup : CellObject
{
    public int AmmoGranted = 3;

    public override void PlayerEntered()
    {
        GameManager.Instance.ChangeAmmo(AmmoGranted);
        Destroy(gameObject);
    }
}