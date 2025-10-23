using System;
using System.Collections.Generic;
using NPC;
using UnityEngine;
namespace Schema
{
    
// This script defines the data structure for your allied NPCs.
// It can be in its own file, e.g., "SquadData.cs".

    
    [Serializable]
    public class SquadData
    {
        public List<NPCData> squad_members = new List<NPCData>();

        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }
    }
}