using System;
using System.Collections;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;

namespace AgarPlugin
{
    public class AgarFoodManager : Plugin
    {
        public HashSet<FoodItem> foodItems;

        public IEnumerable<FoodItem> Food => foodItems;

        const float MAP_WIDTH = 20;

        public AgarFoodManager(PluginLoadData pluginLoadData) :base(pluginLoadData)
        {
            foodItems = new HashSet<FoodItem>();
            Random r = new Random();
            for(ushort index = 0; index < 20; ++index)
            {
                foodItems.Add(new FoodItem(
                    index,
                    (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2.0f,
                    (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2.0f,
                    (byte)r.Next(0, 200),
                    (byte)r.Next(0, 200),
                    (byte)r.Next(0, 200)
                ));
            }

            ClientManager.ClientConnected += ClientConnected;
        }

        public override bool ThreadSafe {
            get
            {
                return false;
            }
        }

        public override Version Version {
               get
            {
                return new Version(1, 0, 0);
            }
        }

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            using (DarkRiftWriter foodWriter = DarkRiftWriter.Create() )
            {
                foreach(FoodItem food in foodItems)
                {
                    foodWriter.Write(food.ID);
                    foodWriter.Write(food.X);
                    foodWriter.Write(food.Y);
                    foodWriter.Write(food.ColorR);
                    foodWriter.Write(food.ColorG);
                    foodWriter.Write(food.ColorB);
                }

                using (Message foodMessage = Message.Create(Tags.SpawnFoodTag, foodWriter))
                {
                    e.Client.SendMessage(foodMessage, SendMode.Reliable);
                }
            }
        }

        public void Eat(FoodItem food)
        {
            Random r = new Random();
            food.X = (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2.0f;
            food.Y = (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2.0f;

            using (DarkRiftWriter foodWriter = DarkRiftWriter.Create())
            {
                foodWriter.Write(food.ID);
                foodWriter.Write(food.X);
                foodWriter.Write(food.Y);

                using (Message foodMessage = Message.Create(Tags.MoveFoodTag, foodWriter))
                {
                    foreach (IClient c in ClientManager.GetAllClients())
                        c.SendMessage(foodMessage, SendMode.Reliable);
                }
            }
        }
    }
}
