using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button continueButton;
    [SerializeField] private float typingSpeed = 0.02f;

    private Queue<string> _sentences = new Queue<string>();
    private bool _isTyping = false;
    private string _currentSentence;
    private Action _onDialogueFinished;

    private void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(DisplayNextSentence);
        }
    }

    public void Initialize(DialogueData data, Action onFinished)
    {
        _onDialogueFinished = onFinished;
        _sentences.Clear();
        nameText.text = data.CharacterName;

        if (data.Portrait != null)
        {
            portraitImage.sprite = data.Portrait;
            portraitImage.gameObject.SetActive(true);
        }
        else
        {
            portraitImage.gameObject.SetActive(false);
        }

        foreach (string sentence in data.Sentences)
        {
            _sentences.Enqueue(sentence);
        }
        gameObject.SetActive(true);
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (_isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = _currentSentence;
            _isTyping = false;
            return;
        }

        if (_sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        _currentSentence = _sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentenceRoutine(_currentSentence));
    }

    private IEnumerator TypeSentenceRoutine(string sentence)
    {
        _isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        _isTyping = false;
    }

    private void EndDialogue()
    {
        gameObject.SetActive(false);
        _onDialogueFinished?.Invoke();
    }
}
