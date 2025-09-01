# MonkeTilla

**MonkeTilla** is a mod for Gorilla Game that provides a simple framework for a modded queue system.  

It creates a dedicated **"Modded"** queue by default and allows other mods to register their own custom queues.  
This ensures that modded players can play together in various game modes without affecting the vanilla game experience for others.  

---

## For Mod Developers

### How to Check for a Modded Lobby
If you are developing your own mod, you can easily check if a player is currently in any modded or custom lobby created by **MonkeTilla**.  

To check if the current room is a modded lobby, simply read the `MonkeTilla.IsInModdedLobby` variable.

**Example:**
```
// First, make sure you are in a room
if (Photon.Pun.PhotonNetwork.InRoom)
{
    // Now, check the public variable from MonkeTilla
    if (MonkeTilla.IsInModdedLobby)
    {
        // Your code for modded lobbies goes here
        UnityEngine.Debug.Log("Welcome to a modded lobby! Enabling special features.");
    }
    else
    {
        // Your code for regular lobbies goes here
        UnityEngine.Debug.Log("This is a standard lobby. Mod features are disabled.");
    }
} 
```

## How to Add Your Own Custom Queue

MonkeTilla exposes a public static method that allows your mod to register its own queue.
A UI with left/right arrows will automatically be created to cycle through all available queues.

To register your queue, call MonkeTilla.RegisterCustomQueue() from your mod's OnLoad() method.

## Rules:

You can register a maximum of 3 custom queues in total. (this will be changed at a later date, for now the user can only have 3 custom queues at once)

Queue names must be unique and will be converted to uppercase.

**Example:**
```
public class MyAwesomeMod : BaseMod
{
    public override void OnLoad()
    {
        // Register a custom queue for a "Hunt" game mode
        bool wasRegistered = MonkeTilla.RegisterCustomQueue("HUNT");

        if (wasRegistered)
        {
            UnityEngine.Debug.Log("Successfully registered HUNT queue!");
        }
        else
        {
            UnityEngine.Debug.Log("Failed to register HUNT queue. It might already exist or the max number of queues has been reached.");
        }
    }
}
```

