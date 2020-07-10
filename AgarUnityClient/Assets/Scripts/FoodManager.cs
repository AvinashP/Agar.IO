using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System;

public class FoodManager : MonoBehaviour
{
    [SerializeField]
    UnityClient client;

    Dictionary<ushort, AgarObject> Food = new Dictionary<ushort, AgarObject>();

    [SerializeField]
    GameObject foodPrefab;

    private void Awake()
    {
        client.MessageReceived += OnMessageReceived;
    }

    // Use this for initialization
    void Start()
    {

    }

    public void Add(ushort ID, AgarObject food)
    {
        Food.Add(ID, food);
    }



    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        {
            if (message.Tag == Tags.SpawnFoodTag)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    while (reader.Position < reader.Length)
                    {
                        ushort id = reader.ReadUInt16();
                        Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle());
                        Color32 color = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), 255);

                        GameObject foodGo = Instantiate(foodPrefab, position, Quaternion.identity);
                        AgarObject foodObj = foodGo.GetComponent<AgarObject>();
                        foodObj.SetColor(color);
                        Add(id, foodObj);
                    }
                }
            }
            else if (message.Tag == Tags.MoveFoodTag)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort id = reader.ReadUInt16();
                    Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle());

                    if (Food.ContainsKey(id))
                    {
                        AgarObject foodObj = Food[id];
                        foodObj.transform.position = position;
                    }
                }
            }

        }
    }
}
