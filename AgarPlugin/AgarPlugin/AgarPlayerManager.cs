﻿using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;

namespace AgarPlugin
{
    public class AgarPlayerManager : DarkRift.Server.Plugin
    {
        Dictionary<IClient, Player> players = new Dictionary<IClient, Player>();

        private Version _version = new Version(1, 0, 0);

        const float MAP_WIDTH = 20;

        public override bool ThreadSafe
        {
            get
            {
                return false;
            }
        }

        public override Version Version
        {
            get
            {
                return _version;
            }
        }

        public AgarPlayerManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
        }

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            Random r = new Random();
            Player newPlayer = new Player(
                e.Client.ID,
                (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2.0f,
                (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2.0f,
                1f,
                (byte)r.Next(0, 200),
                (byte)r.Next(0, 200),
                (byte)r.Next(0, 200)
            );

            using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
            {
                newPlayerWriter.Write(newPlayer.ID);
                newPlayerWriter.Write(newPlayer.X);
                newPlayerWriter.Write(newPlayer.Y);
                newPlayerWriter.Write(newPlayer.Radius);
                newPlayerWriter.Write(newPlayer.ColorR);
                newPlayerWriter.Write(newPlayer.ColorG);
                newPlayerWriter.Write(newPlayer.ColorB);

                using (Message newPlayerMessage = Message.Create(Tags.SpawnPlayerTag, newPlayerWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client))
                        client.SendMessage(newPlayerMessage, SendMode.Reliable);
                }
            }

            players.Add(e.Client, newPlayer);

            using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
            {
                foreach (Player player in players.Values)
                {
                    playerWriter.Write(player.ID);
                    playerWriter.Write(player.X);
                    playerWriter.Write(player.Y);
                    playerWriter.Write(player.Radius);
                    playerWriter.Write(player.ColorR);
                    playerWriter.Write(player.ColorG);
                    playerWriter.Write(player.ColorB);
                }

                using (Message playerMessage = Message.Create(Tags.SpawnPlayerTag, playerWriter))
                    e.Client.SendMessage(playerMessage, SendMode.Reliable);
            }

            e.Client.MessageReceived += MovementMessageReceived;
        }

        void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            players.Remove(e.Client);

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(e.Client.ID);

                using (Message message = Message.Create(Tags.DespawnPlayerTag, writer))
                {
                    foreach (IClient client in ClientManager.GetAllClients())
                        client.SendMessage(message, SendMode.Reliable);
                }
            }
        }

        void MovementMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.MovePlayerTag)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        float newX = reader.ReadSingle();
                        float newY = reader.ReadSingle();

                        Player player = players[e.Client];

                        player.X = newX;
                        player.Y = newY;

                        AgarFoodManager foodManager = PluginManager.GetPluginByType<AgarFoodManager>();
                        foreach(FoodItem food in foodManager.foodItems)
                        {
                            if (Math.Pow(player.X - food.X, 2) + Math.Pow(player.Y - food.Y, 2) < Math.Pow(player.Radius, 2))
                            {
                                player.Radius += food.Radius;
                                SendRadiusUpdate(player);
                                foodManager.Eat(food);
                            }
                        }

                        using (DarkRiftWriter writer = DarkRiftWriter.Create())
                        {
                            writer.Write(player.ID);
                            writer.Write(player.X);
                            writer.Write(player.Y);
                            message.Serialize(writer);
                        }

                        foreach (IClient c in ClientManager.GetAllClients().Where(x => x != e.Client))
                            c.SendMessage(message, e.SendMode);
                    }
                }
            }
        }

        void SendRadiusUpdate(Player player)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(player.ID);
                writer.Write(player.Radius);

                using (Message message = Message.Create(Tags.RadiusUpdateFlag, writer))
                {
                    foreach (IClient c in ClientManager.GetAllClients())
                        c.SendMessage(message, SendMode.Reliable);
                }
            }
        }
    }
}
