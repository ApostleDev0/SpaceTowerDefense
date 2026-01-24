using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button continueButton;
    [SerializeField] private float typingSpeed = 0.02f;
    #endregion

    #region Private Fields
    private Queue<string> _sentences = new Queue<string>();
    private bool _isTyping = false;
    private string _currentSentence;
    private Action _onDialogueFinished;

    private Coroutine _typingCoroutine;
    private WaitForSecondsRealtime _typingWait;
    #endregion

    private void Awake()
    {
        _typingWait = new WaitForSecondsRealtime(typingSpeed);
    }
    private void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(DisplayNextSentence);
        }
    }

    //====PUBLIC
    public void Initialize(DialogueData data, Action onFinished)
    {
        if (data == null)
        {
            return;
        }
        _onDialogueFinished = onFinished;
        _sentences.Clear();

        if (nameText != null)
        {
            nameText.text = data.CharacterName;
        }
        if (portraitImage != null)
        {
            if (data.Portrait != null)
            {
                portraitImage.sprite = data.Portrait;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                portraitImage.gameObject.SetActive(false);
            }
        }
        
        // add sentence
        if (data.Sentences != null)
        {
            foreach (string sentence in data.Sentences)
            {
                _sentences.Enqueue(sentence);
            }
        }
        gameObject.SetActive(true);
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // skip the dialogue when typing
        if (_isTyping)
        {
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);

            dialogueText.text = _currentSentence;
            dialogueText.maxVisibleCharacters = _currentSentence.Length;

            _isTyping = false;
            return;
        }

        // end dialogue
        if (_sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        // start new dialogue
        _currentSentence = _sentences.Dequeue();

        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        _typingCoroutine = StartCoroutine(TypeSentenceRoutine(_currentSentence));
    }

    private IEnumerator TypeSentenceRoutine(string sentence)
    {
        _isTyping = true;
        dialogueText.text = sentence;
        dialogueText.maxVisibleCharacters = 0;

        for (int i = 0; i <= sentence.Length; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return _typingWait;
        }
        _isTyping = false;
    }

    private void EndDialogue()
    {
        gameObject.SetActive(false);
        if (dialogueText != null) dialogueText.maxVisibleCharacters = int.MaxValue;
        _onDialogueFinished?.Invoke();
    }
}
