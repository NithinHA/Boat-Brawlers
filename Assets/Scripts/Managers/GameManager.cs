using System;

public class GameManager : Singleton<GameManager>
{
    public RaftType ActiveRaft;
    public WeaponType SelectedWeapon;

    public Action<bool> PlayerMovementToggle;
}