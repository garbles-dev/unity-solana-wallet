using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Solana.Unity.SDK.Editor
{

    [Serializable]
    internal class CandyMachineGuards
    {
        [SerializeField, JsonProperty("default")]
        private CandyMachineGuardSet defaultGuards;

        [SerializeField, JsonProperty]
        private CandyMachineGuardGroup[] groups;
    }
}