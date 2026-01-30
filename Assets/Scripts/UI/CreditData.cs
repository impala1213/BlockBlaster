using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreditData", menuName = "Credits/Credit Data")]
public class CreditData : ScriptableObject
{
    [Serializable]
    public class TeamEntry
    {
        public string role;
        public List<string> names = new List<string>();
    }

    [Serializable]
    public class ResourceEntry
    {
        public string assetName;
        public string creator;
        public string source;
        public string license;
        public string link;
    }

    [Serializable]
    public class CreditSection
    {
        public string title;
        public List<TeamEntry> teamEntries = new List<TeamEntry>();
        public List<ResourceEntry> resourceEntries = new List<ResourceEntry>();

        [TextArea(2, 6)]
        public string customLines;
    }

    public List<CreditSection> sections = new List<CreditSection>();
}
