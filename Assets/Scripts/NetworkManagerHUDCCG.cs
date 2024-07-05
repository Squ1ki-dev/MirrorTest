using System.ComponentModel;
using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
[AddComponentMenu("Network/NetworkManagerHUD CCG")]
[RequireComponent(typeof(NetworkManager))]
[EditorBrowsable(EditorBrowsableState.Never)]
[HelpURL("https://mirror-networking.com/docs/Components/NetworkManagerHUD.html")]
public class NetworkManagerHUDCCG : MonoBehaviour
{
    NetworkManager manager;

    string username = "";

    public bool showGUI = true;
    public int offsetX;
    public int offsetY;

    private void Awake()
    {
        manager = GetComponent<NetworkManager>();

        if (PlayerPrefs.GetString("Name") != null) username = PlayerPrefs.GetString("Name");
    }

    private void OnGUI()
    {
        if (!showGUI)
            return;

        GUILayout.BeginArea(new Rect(10 + offsetX, 40 + offsetY, 215, 9999));
        if (!NetworkClient.isConnected && !NetworkServer.active)
            StartButtons();
        else
            StatusLabels();

        if (NetworkClient.isConnected && !ClientScene.ready)
        {
            if (GUILayout.Button("Client Ready"))
            {
                ClientScene.Ready(NetworkClient.connection);
                

                if (ClientScene.localPlayer == null)
                    ClientScene.AddPlayer(NetworkClient.connection);
            }
        }

        StopButtons();

        GUILayout.EndArea();
    }

    private void StartButtons()
    {
        if (!NetworkClient.active)
        {
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                if (GUILayout.Button("Host (Server + Client)"))
                {
                    manager.StartHost();

                    PlayerPrefs.SetString("Name", username);

                    showGUI = false;
                }
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Client"))
            {
                manager.StartClient();

                PlayerPrefs.SetString("Name", username);

                showGUI = false;
            }
            manager.networkAddress = GUILayout.TextField(manager.networkAddress);
            GUILayout.EndHorizontal();

            username = GUILayout.TextField(username);

            if (Application.platform == RuntimePlatform.WebGLPlayer)
                GUILayout.Box("(  WebGL cannot be server  )");
            else
                if (GUILayout.Button("Server Only")) manager.StartServer();
        }
        else
        {
            GUILayout.Label("Connecting to " + manager.networkAddress + "..");
            if (GUILayout.Button("Cancel Connection Attempt"))
                manager.StopClient();
        }
    }

    private void StatusLabels()
    {
        if (NetworkServer.active)
            GUILayout.Label("Server: active. Transport: " + Transport.activeTransport);
        if (NetworkClient.isConnected)
            GUILayout.Label("Client: address=" + manager.networkAddress);
    }

    private void StopButtons()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            if (GUILayout.Button("Stop Host"))
                manager.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            if (GUILayout.Button("Stop Client"))
                manager.StopClient();
        }
        else if (NetworkServer.active)
        {
            if (GUILayout.Button("Stop Server"))
                manager.StopServer();
        }
    }
}