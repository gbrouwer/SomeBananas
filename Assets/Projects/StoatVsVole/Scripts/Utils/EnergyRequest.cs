using System;
using UnityEngine;

namespace StoatVsVole
{
    /// <summary>
    /// Represents a two-phase energy exchange request between two agents.
    /// Tracks the requester, provider, requested amount, and the lifecycle of the request.
    /// </summary>
    [Serializable]
    public class EnergyRequest
    {
        #region Public Fields

        public AgentController requester;
        public AgentController provider;
        public float amountRequested;
        public float timeRequested;
        public bool isCompleted;
        public bool isCancelled;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new energy request between two agents.
        /// </summary>
        /// <param name="requester">The agent requesting energy.</param>
        /// <param name="provider">The agent providing energy.</param>
        /// <param name="amountRequested">The amount of energy being requested.</param>
        public EnergyRequest(AgentController requester, AgentController provider, float amountRequested)
        {
            this.requester = requester;
            this.provider = provider;
            this.amountRequested = amountRequested;
            this.timeRequested = Time.time;
            this.isCompleted = false;
            this.isCancelled = false;
        }

        #endregion
    }
}
