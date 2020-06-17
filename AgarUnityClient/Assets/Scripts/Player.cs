using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Client;
using DarkRift.Client.Unity;
using DarkRift;
using System;

public class Player : MonoBehaviour
{
    const byte MOVEMENT_TAG = 1;

    [SerializeField]
    [Tooltip("The distance we can move before we send a position update.")]
    float moveDistance = 0.05f;

    public UnityClient Client { get; set; }

    Vector3 lastPosition;

    void Awake()
    {
        lastPosition = transform.position;

    }

    void Update()
    {
        if (Vector3.Distance(lastPosition, transform.position) > moveDistance)
        {
            /* Send position to server here */

            lastPosition = transform.position;

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(transform.position.x);
                writer.Write(transform.position.y);

                using (Message message = Message.Create(Tags.MovePlayerTag, writer))
                    Client.SendMessage(message, SendMode.Unreliable);
            }
        }
    }
}