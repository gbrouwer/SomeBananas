using System;
using UnityEngine;

namespace StoatVsVole
{
    [Serializable]
    public class EnergyRequest
    {
        public AgentController requester;
        public AgentController provider;
        public float amountRequested;
        public float timeRequested;
        public bool isCompleted;
        public bool isCancelled;

        public EnergyRequest(AgentController requester, AgentController provider, float amountRequested)
        {
            this.requester = requester;
            this.provider = provider;
            this.amountRequested = amountRequested;
            this.timeRequested = Time.time;
            this.isCompleted = false;
            this.isCancelled = false;
        }
    }
}
