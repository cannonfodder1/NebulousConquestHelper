using System;

namespace NebulousConquestHelper
{
    public enum ResourceType
    {
        Fuel,
        Rares,
        Metals,
        Polymers,
        Parts,
        Restores
    }

    public class Resource
    {
        public ResourceType Type;
        public int Stockpile = 0;
        public int Production = 0;
        public int Consumption = 0;
        
        public Resource() { }

        public Resource(ResourceType t) {
            Type = t;
        }

        public Resource(ResourceType t, int s)
        {
            Type = t;
            Stockpile = s;
        }

        public Resource(ResourceType t, int s, int p, int c)
        {
            Type = t;
            Stockpile = s;
            Production = p;
            Consumption = c;
        }

        public void Produce(float percent = 1.0f)
        {
            Stockpile = (int)(Stockpile + (Production * percent));
        }

        public void Consume(float percent = 1.0f)
        {
            Stockpile = (int)(Stockpile - (Consumption * percent));
        }

        public int GetBalance()
        {
            return Production - Consumption;
        }

        public float GetSatisfaction()
        {
            if (Consumption > 0)
            {
                return Math.Min((float) Stockpile / Consumption, 1.0f);
            }
            else
            {
                return 1.0f;
            }
        }
    }
}
