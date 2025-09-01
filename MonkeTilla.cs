using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MonkeTilla : BaseMod
{
    public static bool IsInModdedLobby { get; internal set; }
    internal static bool IsModdedQueueSelected;
    private static GameObject managerObject;

    public override void OnLoad()
    {
        if (managerObject == null)
        {
            managerObject = new GameObject("MonkeTillaManager");
            managerObject.AddComponent<ModdedQueueManager>();
            GameObject.DontDestroyOnLoad(managerObject);
        }
    }

    public override void OnUnload()
    {
        if (managerObject != null)
        {
            GameObject.Destroy(managerObject);
        }
    }
}

public class ModdedQueueManager : MonoBehaviourPunCallbacks
{
    private GameObject moddedButton;
    private MeshRenderer moddedButtonRenderer;

    void Start()
    {
        CreateModdedQueueButton();
    }

    void Update()
    {
        if (GorillaQueue.instance != null && (GorillaQueue.instance.currentQueue == "COMPETITIVE" || GorillaQueue.instance.currentQueue == "DEFAULT"))
        {
            if (MonkeTilla.IsModdedQueueSelected)
            {
                MonkeTilla.IsModdedQueueSelected = false;
                UpdateVisuals();
            }
        }

        if (MonkeTilla.IsModdedQueueSelected && PhotonNetworkController.instance != null)
        {
            PhotonNetworkController.instance.currentGameType = "MODDED";
        }
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameMode", out object gameMode) && gameMode is string mode && mode == "MODDED")
        {
            MonkeTilla.IsInModdedLobby = true;
        }
        else
        {
            MonkeTilla.IsInModdedLobby = false;
        }
    }

    public override void OnLeftRoom()
    {
        MonkeTilla.IsInModdedLobby = false;
    }

    public void SelectModdedQueue()
    {
        MonkeTilla.IsModdedQueueSelected = true;
        GorillaQueue.instance.currentQueue = "MODDED_INTERNAL";
        PlayerPrefs.SetString("gorillaQueue", "MODDED");
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (GorillaQueue.instance == null || moddedButtonRenderer == null) return;

        bool isModded = MonkeTilla.IsModdedQueueSelected;

        moddedButtonRenderer.material = isModded ? GorillaQueue.instance.defaultMaterial : GorillaQueue.instance.transparentDefaultMaterial;
        
        if (GorillaQueue.instance.competitiveRenderer != null)
        {
            GorillaQueue.instance.competitiveRenderer.material = GorillaQueue.instance.transparentCompetitiveMaterial;
        }
        if (GorillaQueue.instance.defaultRenderer != null)
        {
            GorillaQueue.instance.defaultRenderer.material = GorillaQueue.instance.transparentDefaultMaterial;
        }
    }

    private void CreateModdedQueueButton()
    {
        if (GorillaQueue.instance == null || GorillaQueue.instance.competitiveRenderer == null) return;

        GameObject originalButton = GorillaQueue.instance.competitiveRenderer.gameObject;
        moddedButton = Instantiate(originalButton, originalButton.transform.parent);
        moddedButton.name = "ModdedQueueButton";
        moddedButton.transform.localPosition = new Vector3(0.22f, 0.911f, -0.103f);

        GorillaQueueChoice oldTrigger = moddedButton.GetComponent<GorillaQueueChoice>();
        if (oldTrigger != null)
        {
            Destroy(oldTrigger);
        }

        moddedButtonRenderer = moddedButton.GetComponent<MeshRenderer>();
        moddedButtonRenderer.material = GorillaQueue.instance.transparentDefaultMaterial;

        var trigger = moddedButton.AddComponent<ModdedQueueButtonTrigger>();
        trigger.manager = this;

        GameObject originalTextObject = null;
        Text[] allTexts = Resources.FindObjectsOfTypeAll<Text>();
        foreach (Text t in allTexts)
        {
            if (t.gameObject.name == "competitivequeuetext")
            {
                originalTextObject = t.gameObject;
                break;
            }
        }
        
        if (originalTextObject != null)
        {
            GameObject moddedButtonText = Instantiate(originalTextObject, originalTextObject.transform.parent);
            moddedButtonText.SetActive(true);
            moddedButtonText.name = "ModdedQueueText";

            var rectTransform = moddedButtonText.GetComponent<RectTransform>();
            if(rectTransform != null)
            {
                 rectTransform.localPosition = new Vector3(82f, -539f, 1f);
            }
            else
            {
                moddedButtonText.transform.localPosition = new Vector3(82f, -539f, 1f);
            }
           
            Text textComponent = moddedButtonText.GetComponent<Text>();
            if (textComponent != null)
            {
                textComponent.text = "Modded";
            }
        }
    }
}

public class ModdedQueueButtonTrigger : GorillaTriggerBox
{
    public ModdedQueueManager manager;

    public override void OnBoxTriggered()
    {
        base.OnBoxTriggered();
        if (manager != null)
        {
            manager.SelectModdedQueue();
        }
    }
}
