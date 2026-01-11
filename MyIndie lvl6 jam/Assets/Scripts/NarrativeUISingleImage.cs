// NarrativeUISingleImage.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NarrativeUISingleImage : MonoBehaviour
{
    [Header("Менеджер порогов")]
    [SerializeField] private RopesHealthManager manager;

    [Header("Персонажи")]
    [SerializeField] private CharacterNarrativeSO[] characters;

    [Header("UI")]
    [SerializeField] private TMP_Text reasonText;   // замените на Text если не используете TMP
    [SerializeField] private Image mainImage;       // одна картинка, которую меняем
    [SerializeField] private Animator flashAnimator;

    [Header("Поведение")]
    [Tooltip("Скрывать картинку до первого срабатывания порога")]
    [SerializeField] private bool hideImageUntilTriggered = true;

    private CharacterNarrativeSO _current;

    private void Awake()
    {
        if (manager == null) manager = FindObjectsByType<RopesHealthManager>(FindObjectsSortMode.None)[0];
    }
    /*
    private void OnEnable()
    {
        if (manager == null) return;

        manager.OnFirstHit2 += HandleHit2;
        manager.OnFirstHit1 += HandleHit1;
        manager.OnFirstHit0 += HandleHit0;
    }

    private void OnDisable()
    {
        if (manager == null) return;

        manager.OnFirstHit2 -= HandleHit2;
        manager.OnFirstHit1 -= HandleHit1;
        manager.OnFirstHit0 -= HandleHit0;
    }*/

    private void Start()
    {
        PickRandomCharacterAndApply();
    }

    private void PickRandomCharacterAndApply()
    {
        if (characters == null || characters.Length == 0)
        {
            Debug.LogWarning("[NarrativeUI] Characters array is empty.");
            return;
        }

        _current = characters[Random.Range(0, characters.Length)];

        if (reasonText != null)
            reasonText.text = _current.executionReasonText;

        if (mainImage != null)
        {
            if (hideImageUntilTriggered)
            {
                mainImage.sprite = null;
                mainImage.enabled = false;
            }
            else
            {
                mainImage.sprite = null;
                mainImage.enabled = true;
            }
        }
    }

    private void HandleHit2(TapePiece _)
    {
        if (_current == null || mainImage == null) return;
        mainImage.sprite = _current.spriteAt2;
        mainImage.enabled = mainImage.sprite != null;
        flashAnimator.SetTrigger("Flash");
    }

    private void HandleHit1(TapePiece _)
    {
        if (_current == null || mainImage == null) return;
        mainImage.sprite = _current.spriteAt1;
        mainImage.enabled = mainImage.sprite != null;
        flashAnimator.SetTrigger("Flash");

    }

    private void HandleHit0(TapePiece _)
    {
        if (_current == null || mainImage == null) return;
        mainImage.sprite = _current.spriteAt0;
        mainImage.enabled = mainImage.sprite != null;
        flashAnimator.SetTrigger("Dead");

    }
}

