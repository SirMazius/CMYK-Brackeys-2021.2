using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIButtonController : SerializedMonoBehaviour
{
    private Button _button;
    private Animator _animator;
    public SoundId ClickSound;
    private const string _pressedKey = "Pressed";
    private bool _interactable
    {
        get => _button ? _button.interactable : false;
        set => _button.interactable = value;
    }

    public float delayBetweenClicks = 0.5f;
    [SerializeField]
    public UnityEvent onPressed = new UnityEvent();


    [ExecuteAlways]
    private void Awake()
    {
        if (!_button)
        {
            _button = GetComponentInChildren<Button>(true);
            _button.onClick.AddListener(Pressed);
        }

        if (!_animator)
            _animator = GetComponentInChildren<Animator>(true);

        if(ClickSound >= 0)
            onPressed.AddListener(()=>AudioManager.self.PlayAdditively(ClickSound));
    }

    public void Pressed()
    {
        if(_interactable)
        {
            _animator.SetTrigger(_pressedKey);
            StartCoroutine(ClickAgainDelay());
            onPressed.Invoke();
        }
    }

    public IEnumerator ClickAgainDelay()
    {
        _interactable = false;
        yield return new WaitForSeconds(delayBetweenClicks);
        _interactable = true;
    }
}
