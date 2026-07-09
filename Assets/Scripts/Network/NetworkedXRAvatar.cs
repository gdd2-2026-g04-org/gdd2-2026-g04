using Fusion;
using UnityEngine;

public class NetworkedXRAvatar : NetworkBehaviour
{
    [Header("Networked Avatar Parts")]
    [SerializeField] private Transform head;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    [SerializeField] private GameObject[] hideForLocalPlayer;

    [Header("Warrior Visuals")]
    [SerializeField] private GameObject warriorSwordVisual;
    [SerializeField] private GameObject warriorShieldVisual;

    [Header("Mage Visuals")]
    [SerializeField] private GameObject mageStaffVisual;
    
    [Header("Healer Visuals")]
    [SerializeField] private GameObject HealerStaffVisual;
    [SerializeField] private GameObject HealerBookVisual; 
    
    [Header("Archer Visuals")]
    [SerializeField] private GameObject archerBowVisual;
    
    // Networked properties that sync automatically to all players
    [Networked, OnChangedRender(nameof(OnClassChanged))]
    public PlayerClass SelectedClass { get; private set; }
    
    [Networked, OnChangedRender(nameof(OnReadyChanged))]
    public NetworkBool IsReady { get; private set; }

    private XRReferences xrReferences;

    public override void Spawned()
    {
        var isOwner = Object.HasStateAuthority;
        
        SetVisualsVisible(!isOwner);

        if (!isOwner)
        {
            ApplyClassVisuals();
            return;
        }
        
        xrReferences = XRReferences.Instance;

        if (LocalClassSelector.Instance != null)
        {
            LocalClassSelector.Instance.ClassChanged += SetClass;
            var selectedClass = LocalClassSelector.Instance.SelectedClass;

            if (selectedClass != PlayerClass.None) SetClass(selectedClass);
        }

        ApplyClassVisuals();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Object.HasStateAuthority && LocalClassSelector.Instance != null)
        {
            LocalClassSelector.Instance.ClassChanged -= SetClass;
        }
    }

    private void SetVisualsVisible(bool visible)
    {
        if (hideForLocalPlayer == null) return;
        
        foreach (GameObject visual in hideForLocalPlayer)
        {
            visual?.SetActive(visible);
        }
    }

    private void LateUpdate()
    {
        if (!Object.HasStateAuthority) return;
        
        if (!xrReferences) xrReferences = XRReferences.Instance;
        if (!xrReferences) return;
        
        CopyTransform(xrReferences.head, head);
        CopyTransform(xrReferences.leftHand, leftHand);
        CopyTransform(xrReferences.rightHand, rightHand);
    }

    private void CopyTransform(Transform src, Transform t)
    {
        if (!src || !t) return;
        t.SetPositionAndRotation(src.position, src.rotation);
    }

    public void SetClass(PlayerClass selectedClass)
    {
        if (!Object.HasStateAuthority)
        {
            Debug.LogWarning($"{name}: Only State Authority can change the selected class.");
            return;
        }

        if (selectedClass == PlayerClass.None)
        {
            Debug.LogWarning($"{name}: \"None\" is not a valid class.");
            return;
        }
        
        SelectedClass = selectedClass;
        IsReady = false;
        
        GetComponent<GameAssets.Health.PlayerHealth>()?.SetClass(selectedClass);
        
        ApplyClassVisuals();
        RPC_RequestLobbyCheck();
    }

    public void SetReady(bool ready)
    {
        if (!Object.HasStateAuthority) return;
        
        if (ready && SelectedClass == PlayerClass.None)
        {
            Debug.LogWarning($"{name}: Can't ready up with class being \"None\".");
            return;
        }

        IsReady = ready;
        
        NetworkManager.Instance?.CheckLobbyStatus();
        RPC_RequestLobbyCheck();
    }

    public void RequestBattleRestart()
    {
        if (!Object.HasStateAuthority) return;

        RPC_RequestBattleRestart();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_RequestBattleRestart()
    {
        if (Runner.IsSceneAuthority) NetworkManager.Instance?.RestartBattle();
    }

    private void OnClassChanged()
    {
        ApplyClassVisuals();
        
        NetworkManager.Instance?.CheckLobbyStatus();
    }

    private void OnReadyChanged()
    {
        Debug.Log($"{name}: class = {SelectedClass}, ready = {IsReady}");
        
        NetworkManager.Instance?.CheckLobbyStatus();
    }
    
    private void ApplyClassVisuals()
    {
        var showRemoteEquipment = !Object.HasStateAuthority;

        var warriorActive =
            showRemoteEquipment &&
            SelectedClass == PlayerClass.Warrior;

        var mageActive =
            showRemoteEquipment &&
            SelectedClass == PlayerClass.Mage;

        var healerActive =
            showRemoteEquipment &&
            SelectedClass == PlayerClass.Healer;

       var archerActive =
            showRemoteEquipment &&
            SelectedClass == PlayerClass.Archer;

        SetActive(warriorSwordVisual, warriorActive);
        SetActive(warriorShieldVisual, warriorActive);
        SetActive(mageStaffVisual, mageActive);
        SetActive(HealerBookVisual, healerActive);
        SetActive(HealerStaffVisual, healerActive);
        SetActive(archerBowVisual, archerActive);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_RequestLobbyCheck()
    {
        if (Runner.IsSceneAuthority)
        {
            NetworkManager.Instance?.RequestLobbyCheck();
        }
    }
    
    private static void SetActive(GameObject target, bool active)
    {
        if (target != null && target.activeSelf != active) target.SetActive(active);
    }

}