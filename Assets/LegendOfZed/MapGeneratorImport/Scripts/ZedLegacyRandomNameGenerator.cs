using System.Collections.Generic;
using UnityEngine;

namespace LegendOfZed.LegacyMapGenerator
{
    public class ZedLegacyRandomNameGenerator : MonoBehaviour
    {
        public List<string> womensFirstNames = new List<string>();
        public List<string> mensFirstNames = new List<string>();
        public List<string> surNames = new List<string>();

        private void Start()
        {
            FirstNameList(true);
            FirstNameList(false);
            SurNameList();
        }

        private void FirstNameList(bool male)
        {
            TextAsset firstNameText = Resources.Load<TextAsset>(male ? "FirstNames_Men" : "FirstNames_Women");
            if (firstNameText == null)
            {
                return;
            }

            string[] lines = firstNameText.text.Split('\n');
            List<string> target = male ? mensFirstNames : womensFirstNames;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (!string.IsNullOrEmpty(line))
                {
                    target.Add(line);
                }
            }
        }

        private void SurNameList()
        {
            TextAsset surNameText = Resources.Load<TextAsset>("SurNames");
            if (surNameText == null)
            {
                return;
            }

            string[] lines = surNameText.text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (!string.IsNullOrEmpty(line))
                {
                    surNames.Add(line);
                }
            }
        }

        public string GenerateName()
        {
            bool male = Random.Range(0, 2) == 0;
            List<string> firstNames = male ? mensFirstNames : womensFirstNames;
            if (firstNames.Count == 0 || surNames.Count == 0)
            {
                return "DEFAULT NAME";
            }

            return firstNames[Random.Range(0, firstNames.Count)] + " " + surNames[Random.Range(0, surNames.Count)];
        }

        public string GenerateZedName(string humanName)
        {
            string firstName = humanName;
            if (firstName == "DEFAULT" || string.IsNullOrEmpty(firstName))
            {
                firstName = GenerateName();
            }

            return firstName + " ZOMBIE";
        }
    }
}
