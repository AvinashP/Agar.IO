using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Client;
using DarkRift.Client.Unity;
using DarkRift;

public class NetworkPlayerManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    UnityClient client;

    Dictionary<ushort, AgarObject> networkPlayers = new Dictionary<ushort, AgarObject>();

    public void Add(ushort id, AgarObject player)
    {
        networkPlayers.Add(id, player);
    }

    void Awake()
    {
        client.MessageReceived += OnMessageReceived;
    }

    public void DestroyPlayer(ushort id)
    {
        AgarObject o = networkPlayers[id];

        Destroy(o.gameObject);

        networkPlayers.Remove(id);
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        {
            if (message.Tag == Tags.MovePlayerTag)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort id = reader.ReadUInt16();
                    if (networkPlayers.ContainsKey(id))
                    {
                        Vector3 newPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle());
                        networkPlayers[id].SetMovePosition(newPosition);
                    }
                }
            }
            else if(message.Tag == Tags.RadiusUpdateFlag)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort id = reader.ReadUInt16();
                    if (networkPlayers.ContainsKey(id))
                    {
                        float newRadius = reader.ReadSingle();
                        networkPlayers[id].SetRadius(newRadius);
                    }
                }
            }
        }
    }
}
