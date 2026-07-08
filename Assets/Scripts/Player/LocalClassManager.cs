using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LocalClassManager : MonoBehaviour
{
    public static LocalClassManager Instance;
    
    public PlayerClass selectedClass = PlayerClass.None;

    [Header("Warrior Weapons Prefabs")]
    [SerializeField] private GameObject swordPrefab;
    [SerializeField] private GameObject shieldPrefab;

    [Header("Mage Weapons Prefabs")]
    [SerializeField] private GameObject mageStaffPrefab;

    [Header("Healer Weapons Prefabs")]
    [SerializeField] private GameObject healerStaffPrefab;

    [Header("Hunter Weapons Prefabs")]
    [SerializeField] private GameObject hunterBowPrefab;

    [Header("Lobby UI Buttons")]
    [SerializeField] private Button warriorButton;
    [SerializeField] private Button mageButton;
    [SerializeField] private Button healerButton;
    [SerializeField] private Button hunterButton;
    [SerializeField] private Button readyButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
        {
            // Force the class selection to start at None so no button is pre-selected
            selectedClass = PlayerClass.None;

            // 1. Lock the Ready button on start because no class has been chosen yet
            if (readyButton != null)
            {
                readyButton.interactable = false;
            }

            // 2. Link UI buttons to their selection functions
            if (warriorButton != null) warriorButton.onClick.AddListener(() => SelectClass(PlayerClass.Warrior));
            if (mageButton != null) mageButton.onClick.AddListener(() => SelectClass(PlayerClass.Mage));
            if (healerButton != null) healerButton.onClick.AddListener(() => SelectClass(PlayerClass.Healer));
            if (hunterButton != null) hunterButton.onClick.AddListener(() => SelectClass(PlayerClass.Archer));
            if (readyButton != null) readyButton.onClick.AddListener(StartBattleScene);

            // Reset the button highlights so everything starts white
            UpdateButtonHighlights();
        }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name is "NetworkScene" or "SampleScene")
        {
            EquipLocalClassWeapons();
        }
    }

    public void SelectClass(PlayerClass choice)
    {
        selectedClass = choice;
        Debug.Log($"Class selected: {selectedClass}");

        // Enable the Ready Button now that a class is chosen
        if (readyButton != null)
        {
            readyButton.interactable = true;
        }

        UpdateButtonHighlights();
    }

    public void StartBattleScene()
    {
        if (selectedClass == PlayerClass.None) return;
        SceneManager.LoadScene("NetworkScene");
    }

    private void UpdateButtonHighlights()
    {
        SetButtonColor(warriorButton, selectedClass == PlayerClass.Warrior);
        SetButtonColor(mageButton, selectedClass == PlayerClass.Mage);
        SetButtonColor(healerButton, selectedClass == PlayerClass.Healer);
        SetButtonColor(hunterButton, selectedClass == PlayerClass.Archer);
    }

    private void SetButtonColor(Button button, bool isSelected)
    {
        if (button == null) return;
        ColorBlock colors = button.colors;
        
        // Highlights green if selected, resets to white if not
        colors.normalColor = isSelected ? Color.green : Color.white;
        colors.selectedColor = isSelected ? Color.green : Color.white;
        
        button.colors = colors;
    }

    private void EquipLocalClassWeapons()
    {
        XRReferences xr = XRReferences.Instance;
        if (xr == null) return;

        switch (selectedClass)
        {
            case PlayerClass.Warrior:
                SpawnAndAttach(swordPrefab, xr.rightHand);
                SpawnAndAttach(shieldPrefab, xr.leftHand);
                break;

            case PlayerClass.Mage:
                GameObject staff = SpawnAndAttach(mageStaffPrefab, xr.rightHand);
                InitializeMageLogic(staff);
                break;

            case PlayerClass.Healer:
                GameObject mace = SpawnAndAttach(healerStaffPrefab, xr.rightHand);
                InitializeHealerLogic(mace);
                break;

            case PlayerClass.Archer:
                GameObject bow = SpawnAndAttach(hunterBowPrefab, xr.leftHand); // Bows usually in left hand
                InitializeHunterLogic(bow);
                break;
        }
    }

    private GameObject SpawnAndAttach(GameObject prefab, Transform handTransform)
    {
        if (prefab == null || handTransform == null) return null;

        GameObject weapon = Instantiate(prefab);
        Transform attachPoint = weapon.transform.Find("AttachPoint");
        weapon.transform.SetParent(handTransform);

        if (attachPoint != null)
        {
            weapon.transform.localRotation = Quaternion.Inverse(attachPoint.localRotation);
            weapon.transform.localPosition = -(weapon.transform.localRotation * attachPoint.localPosition);
        }
        else
        {
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
        }

        weapon.transform.localScale = Vector3.one;

        if (weapon.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (weapon.TryGetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>(out var grab))
        {
            grab.enabled = false;
        }

        return weapon;
    }

    // ==========================================
    // PLACEHOLDER METHODS FOR COMBAT DEVELOPERS
    // ==========================================

    private void InitializeMageLogic(GameObject staffInstance)
    {
        if (staffInstance == null) return;
        Debug.Log("Initializing Mage Combat Logic Placeholder...");
        
        // Example of where other developers can link script components:
        // var mageScript = staffInstance.AddComponent<MageStaffMagic>();
        // mageScript.InitializeSpellCast();
    }

    private void InitializeHealerLogic(GameObject healerInstance)
    {
        if (healerInstance == null) return;
        Debug.Log("Initializing Healer Combat Logic Placeholder...");
    }

    private void InitializeHunterLogic(GameObject bowInstance)
    {
        if (bowInstance == null) return;
        Debug.Log("Initializing Hunter Combat Logic Placeholder...");
    }
}