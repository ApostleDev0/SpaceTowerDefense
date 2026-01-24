using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Scriptable Objects/DialogueData")]
public class DialogueData : ScriptableObject
{
    [SerializeField] private string characterName;
    [SerializeField] private Sprite portrait;
    [TextArea(3, 10)]
    [SerializeField] private string[] sentences;

    public string CharacterName => characterName;
    public Sprite Portrait => portrait;
    public string[] Sentences => sentences;
}
