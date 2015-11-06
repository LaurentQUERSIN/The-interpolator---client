using UnityEngine;
using UnityEngine.UI;
using Stormancer;
using Stormancer.Core;
using System.Collections.Generic;
using System.IO;

public class NetworkManager : MonoBehaviour
{
    public GameObject playerShip;
    public GameObject remotePlayerShipPrefab;
    public InputField nicknameText;
    public Text connectionText;
    public Button connectButton;
    public Button disconnectButton;

    public ChatPanel chatPanel;
    private Dictionary<long, GameObject> _remotePlayers = new Dictionary<long, GameObject>();

    private Client _client;
    private Scene _scene;

    private string _version = "a0.1";
    private bool _connecting = false;

    private long _id = -100000;

	public void Connect()
    {
        if (_connecting == true)
            return;
        _connecting = true;
        connectionText.text = "connecting";
        if (nicknameText.text == "")
        {
            _connecting = false;
            connectionText.text = "Enter user name";
            return;
        }
        if (_client != null)
        {
            _client.Dispose();
            _client = null;
        }
        ClientConfiguration config = ClientConfiguration.ForAccount("7794da14-4d7d-b5b5-a717-47df09ca8492", "theinterpolator");
        _client = new Client(config);
        var cdto = new ConnectionDTO();
        cdto.version = _version;
        cdto.name = nicknameText.text;
        _client.GetPublicScene("interpolator", cdto).ContinueWith(getSceneTask =>
        {
            if (getSceneTask.IsFaulted)
            {
                _connecting = false;
                MainThread.Post(() =>
                {
                    connectionText.text = "connection failed";
                });
            }
            else
            {
                _scene = getSceneTask.Result;
                _scene.AddRoute("chat", OnChat);
                _scene.AddRoute("update_position", OnUpdatePosition);
                _scene.AddRoute("get_id", OnGetId);
                _scene.AddRoute("create_player", OnCreatePlayer);
                _scene.AddRoute("remove_player", OnRemovePlayer);
                _scene.Connect().ContinueWith(connectTask =>
                {
                    if (_scene.Connected == false)
                    {
                        _connecting = false;
                        MainThread.Post(() =>
                        {
                            connectionText.text = connectTask.Exception.Message;
                        });
                    }
                    else
                    {
                        MainThread.Post(() =>
                        {
                            playerShip.GetComponent<PlayerShip>().connected = true;
                            nicknameText.gameObject.SetActive(false);
                            connectButton.gameObject.SetActive(false);
                            connectionText.gameObject.SetActive(false);
                            chatPanel.gameObject.SetActive(true);
                            chatPanel._connected = true;
                        });
                    }
                });
            }
        });
    }

    public void Disconnect()
    {
        Application.Quit();
    }

    public void OnGetId(Packet<IScenePeer> packet)
    {
        using (var reader = new BinaryReader(packet.Stream))
        {
            _id = reader.ReadInt64();
            Debug.Log("get id = " + _id);
        }
    }

    public void OnCreatePlayer(Packet<IScenePeer> packet)
    {
        using (var reader = new BinaryReader(packet.Stream))
        {
            var id = reader.ReadInt64();
            if (id != _id && _id > -5000)
            {
                MainThread.Post(() =>
                {
                    Debug.Log("new remote player id = " + id);
                    GameObject np = Instantiate(remotePlayerShipPrefab);
                    _remotePlayers.Add(id, np);
                });
            }
        }
    }

    public void OnRemovePlayer(Packet<IScenePeer> packet)
    {
        using (var reader = new BinaryReader(packet.Stream))
        {
            var id = reader.ReadInt64();
            Debug.Log("deleting remote player id = " + _id);
            GameObject np;
            if (_remotePlayers.TryGetValue(id, out np))
            {
                MainThread.Post(() =>
                {
                    Destroy(np);
                });
                _remotePlayers.Remove(id);
            }
        }
    }

    public void OnChat(Packet<IScenePeer> packet)
    {
        MainThread.Post(() =>
        {
            Debug.Log("received chat message");
            chatPanel.chat.text += "\n" + packet.ReadObject<string>();
        });
    }

    public void OnSendChat()
    {
        if (chatPanel.playerText.text != "")
        {
            _scene.Send<string>("chat", chatPanel.playerText.text);
        }
    }

    public void OnUpdatePosition(Packet<IScenePeer> packet)
    {
        using (var reader = new BinaryReader(packet.Stream))
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var id = reader.ReadInt64();
                var x = reader.ReadSingle();
                var y = reader.ReadSingle();
                var z = reader.ReadSingle();

                var vx = reader.ReadSingle();
                var vy = reader.ReadSingle();
                var vz = reader.ReadSingle();

                var rx = reader.ReadSingle();
                var ry = reader.ReadSingle();
                var rz = reader.ReadSingle();
                var rw = reader.ReadSingle();

                GameObject np;
                if (_remotePlayers.TryGetValue(id, out np))
                {
                    MainThread.Post(() =>
                    {
                        np.GetComponent<RemotePlayer>().SetNextPos(new Vector3(x, y, z), new Vector3(vx, vy, vz), new Quaternion(rx, ry, rz, rw));
                    });
                }
            }
        }
    }

    private long _lastupdate = 0;
    private Vector3 _lastPos = Vector3.zero;

	void Update ()
    {
	    if (_client != null && _scene != null && _scene.Connected && _lastupdate + 100 < _client.Clock)
        {
            _lastupdate = _client.Clock;
            _scene.SendPacket("update_position", w =>
            {
                using (var writer = new BinaryWriter(w, System.Text.Encoding.UTF8))
                {
                    writer.Write(_id);
                    writer.Write(playerShip.transform.position.x);
                    writer.Write(playerShip.transform.position.y);
                    writer.Write(playerShip.transform.position.z);

                    var vect = playerShip.transform.position - _lastPos;
                    _lastPos = playerShip.transform.position;
                    writer.Write(vect.x);
                    writer.Write(vect.y);
                    writer.Write(vect.z);

                    writer.Write(playerShip.transform.rotation.x);
                    writer.Write(playerShip.transform.rotation.y);
                    writer.Write(playerShip.transform.rotation.z);
                    writer.Write(playerShip.transform.rotation.w);
                }
            }, PacketPriority.MEDIUM_PRIORITY, PacketReliability.UNRELIABLE_SEQUENCED);
        }
	}

    void Start()
    {
        UniRx.MainThreadDispatcher.Initialize();
        connectButton.onClick.AddListener(this.Connect);
        disconnectButton.onClick.AddListener(this.Disconnect);
        chatPanel.sendButton.onClick.AddListener(this.OnSendChat);
    }

    void OnApplicationQuit()
    {
        if (_scene != null && _scene.Connected)
            _scene.Disconnect();
    }
}
