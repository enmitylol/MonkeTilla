using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MonkeTilla : BaseMod
{
    public static bool IsInModdedLobby { get; internal set; }
    private static GameObject managerObject;
    internal static List<string> CustomQueues = new List<string>();

    public override void OnLoad()
    {
        if (managerObject == null)
        {
            managerObject = new GameObject("MonkeTillaManager");
            managerObject.AddComponent<CustomQueueManager>();
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

    public static bool RegisterCustomQueue(string queueName)
    {
        if (CustomQueues.Count >= 3 || string.IsNullOrEmpty(queueName) || CustomQueues.Contains(queueName.ToUpper()))
        {
            return false;
        }
        
        CustomQueues.Add(queueName.ToUpper());
        
        if (managerObject != null)
        {
            var manager = managerObject.GetComponent<CustomQueueManager>();
            if (manager != null)
            {
                manager.OnCustomQueueRegistered();
            }
        }
        return true;
    }
}

public class CustomQueueManager : MonoBehaviourPunCallbacks
{
    private List<string> allQueues = new List<string> { "DEFAULT", "COMPETITIVE", "MODDED" };
    private int currentQueueIndex = 0;

    private MeshRenderer moddedButtonRenderer;
    private Text queueDisplayText;
    private Text defaultText;
    private Text competitiveText;
    private Text moddedText;
    
    private GorillaQueue gorillaQueue;
    private PhotonNetworkController networkController;

    void Start()
    {
        gorillaQueue = GorillaQueue.instance;
        networkController = PhotonNetworkController.instance;
        
        if (gorillaQueue == null) return;
        
        defaultText = FindText("DefaultQueueText");
        competitiveText = FindText("competitivequeuetext");

        CreateButtonsAndText();
        UpdateVisuals();
    }
    
    public void OnCustomQueueRegistered()
    {
        allQueues = new List<string> { "DEFAULT", "COMPETITIVE", "MODDED" };
        allQueues.AddRange(MonkeTilla.CustomQueues);
        UpdateVisuals();
    }

    void Update()
    {
        // This was previously setting the currentGameType every frame,
        // causing an automatic connection on startup. It is now handled
        // only when a queue is explicitly selected by the player.
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameMode", out object gameMode) && gameMode is string mode)
        {
            MonkeTilla.IsInModdedLobby = (mode != "DEFAULT" && mode != "COMPETITIVE");
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

    public void CycleQueue(int direction)
    {
        if (allQueues.Count <= 3) return;

        currentQueueIndex += direction;
        if (currentQueueIndex >= allQueues.Count)
        {
            currentQueueIndex = 0;
        }
        if (currentQueueIndex < 0)
        {
            currentQueueIndex = allQueues.Count - 1;
        }
        
        SelectQueueByIndex(currentQueueIndex);
    }
    
    public void SelectQueueByIndex(int index)
    {
        if (index < 0 || index >= allQueues.Count) return;

        currentQueueIndex = index;
        gorillaQueue.currentQueue = allQueues[currentQueueIndex];
        PlayerPrefs.SetString("gorillaQueue", allQueues[currentQueueIndex]);
        if (networkController != null)
        {
            networkController.currentGameType = allQueues[currentQueueIndex];
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (gorillaQueue == null) return;
        
        string selectedQueue = allQueues[currentQueueIndex];
        bool hasCustomQueues = allQueues.Count > 3;

        if (defaultText != null) defaultText.gameObject.SetActive(!hasCustomQueues);
        if (competitiveText != null) competitiveText.gameObject.SetActive(!hasCustomQueues);
        if (moddedText != null) moddedText.gameObject.SetActive(!hasCustomQueues);
        if (queueDisplayText != null) queueDisplayText.gameObject.SetActive(hasCustomQueues);

        gorillaQueue.defaultRenderer.material = (selectedQueue == "DEFAULT") ? gorillaQueue.defaultMaterial : gorillaQueue.transparentDefaultMaterial;
        gorillaQueue.competitiveRenderer.material = (selectedQueue == "COMPETITIVE") ? gorillaQueue.competitiveMaterial : gorillaQueue.transparentCompetitiveMaterial;
        
        if(moddedButtonRenderer != null)
        {
            moddedButtonRenderer.material = (selectedQueue == "MODDED") ? gorillaQueue.defaultMaterial : gorillaQueue.transparentDefaultMaterial;
        }

        if (hasCustomQueues && queueDisplayText != null)
        {
            queueDisplayText.text = selectedQueue;
            queueDisplayText.fontStyle = (currentQueueIndex > 2) ? FontStyle.Bold : FontStyle.Normal;
        }
    }
    
    private void CreateButtonsAndText()
    {
        GameObject moddedButton = CreateButton("ModdedQueueButton", new Vector3(0.22f, 0.911f, -0.103f));
        moddedButton.AddComponent<QueueSelectTrigger>().Initialize(this, 2);
        moddedButtonRenderer = moddedButton.GetComponent<MeshRenderer>();
        
        GameObject leftButton = CreateButton("CycleQueueLeftButton", new Vector3(-0.014f, 0.911f, -1.229f));
        leftButton.AddComponent<QueueCycleTrigger>().Initialize(this, -1);
        
        GameObject rightButton = CreateButton("CycleQueueRightButton", new Vector3(-0.014f, 0.911f, 1.2f));
        rightButton.AddComponent<QueueCycleTrigger>().Initialize(this, 1);
        
        if (competitiveText != null)
        {
           moddedText = CreateText("ModdedQueueText", "modded", new Vector3(82f, -539f, 1f), competitiveText);
        }

        if (defaultText != null)
        {
            queueDisplayText = CreateText("CustomQueueText", "DEFAULT", defaultText.rectTransform.localPosition, defaultText);
        }
    }

    private GameObject CreateButton(string name, Vector3 position)
    {
        GameObject originalButton = gorillaQueue.competitiveRenderer.gameObject;
        GameObject newButton = Instantiate(originalButton, originalButton.transform.parent);
        newButton.name = name;
        newButton.transform.localPosition = position;
        Destroy(newButton.GetComponent<GorillaQueueChoice>());
        return newButton;
    }

    private Text FindText(string name)
    {
        foreach (Text t in Resources.FindObjectsOfTypeAll<Text>())
        {
            if (t.gameObject.name == name) return t;
        }
        return null;
    }
    
    private Text CreateText(string name, string content, Vector3 position, Text prefab)
    {
        if(prefab == null) return null;
        
        GameObject textObject = Instantiate(prefab.gameObject, prefab.transform.parent);
        textObject.SetActive(true);
        textObject.name = name;
        
        var rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.localPosition = position;

        Text textComponent = textObject.GetComponent<Text>();
        textComponent.text = content;

        return textComponent;
    }
}

public class QueueCycleTrigger : GorillaTriggerBox
{
    private CustomQueueManager manager;
    private int direction;

    public void Initialize(CustomQueueManager manager, int direction)
    {
        this.manager = manager;
        this.direction = direction;
    }

    public override void OnBoxTriggered()
    {
        base.OnBoxTriggered();
        manager?.CycleQueue(direction);
    }
}

public class QueueSelectTrigger : GorillaTriggerBox
{
    private CustomQueueManager manager;
    private int queueIndexToSelect;

    public void Initialize(CustomQueueManager manager, int index)
    {
        this.manager = manager;
        this.queueIndexToSelect = index;
    }
    
    public override void OnBoxTriggered()
    {
        base.OnBoxTriggered();
        manager?.SelectQueueByIndex(queueIndexToSelect);
    }
}
