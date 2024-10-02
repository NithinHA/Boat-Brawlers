using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Cinemachine;
using UnityEngine;

public class MainMenuController : Singleton<MainMenuController>
{
    [SerializeField] private SerializedDictionary<RaftType, GameObject> _raftMap = new SerializedDictionary<RaftType, GameObject>();
    [SerializeField] private SerializedDictionary<WeaponType, GameObject> m_WeaponMap = new SerializedDictionary<WeaponType, GameObject>();

    public CinemachineVirtualCamera MenuCam;
    public CinemachineVirtualCamera PlayerCam;
    public CinemachineVirtualCamera RaftSelectionCam;
    public CinemachineVirtualCamera WeaponSelectionCam;
    
    protected override void Start()
    {
        base.Start();
        MenuCameraSwitcher.SwitchCamera(MenuCam);
        OnRaftChange(GameManager.Instance.ActiveRaft);
        OnWeaponChange(GameManager.Instance.SelectedWeapon);
    }

    private void OnEnable()
    {
        MenuCameraSwitcher.Register(MenuCam);
        MenuCameraSwitcher.Register(PlayerCam);
        MenuCameraSwitcher.Register(RaftSelectionCam);
        MenuCameraSwitcher.Register(WeaponSelectionCam);
    }

    private void OnDisable()
    {
        MenuCameraSwitcher.Unregister(MenuCam);
        MenuCameraSwitcher.Unregister(PlayerCam);
        MenuCameraSwitcher.Unregister(RaftSelectionCam);
        MenuCameraSwitcher.Unregister(WeaponSelectionCam);
    }
    
#region Event listeners
    
    public void OnRaftChange(RaftType type)
    {
        GameManager.Instance.ActiveRaft = type;
        foreach (KeyValuePair<RaftType, GameObject> item in _raftMap)
        {
            item.Value.SetActive(false);
        }

        _raftMap[type].SetActive(true);
    }

    public void OnWeaponChange(WeaponType type)
    {
        GameManager.Instance.SelectedWeapon = type;
        foreach (KeyValuePair<WeaponType, GameObject> item in m_WeaponMap)
        {
            item.Value.SetActive(false);
        }

        m_WeaponMap[type].SetActive(true);
    }

#endregion
}