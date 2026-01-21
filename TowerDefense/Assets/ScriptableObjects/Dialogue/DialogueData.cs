using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Objects/DialogueData")]
public class DialogueData : ScriptableObject
{
    public string characterName;
    public Sprite portrait;
    [TextArea(3, 10)]
    public string[] sentences;
}
