using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestsScript : MonoBehaviour
{
    
    public TextMeshProUGUI questText;
    private List<string> quests = new List<string>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        quests.Add("End Waddle Quackdonald's entire career");
        quests.Add("Collect exotic meats from Marinara Trench");
        quests.Add("Find the star");   
    }

    // Update is called once per frame
    void Update()
    {
        questText.text = "> " + string.Join("\n> ", quests);
    }
}
