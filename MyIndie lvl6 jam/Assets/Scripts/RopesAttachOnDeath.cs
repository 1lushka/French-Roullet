// RopesAttachOnDeath.cs
using UnityEngine;

public class RopesAttachOnDeath : MonoBehaviour
{
    [Header("Менеджер здоровья верёвок")]
    [SerializeField] private RopesHealthManager manager;

    [Header("Куда прикреплять")]
    [SerializeField] private Transform leftAnchor;
    [SerializeField] private Transform rightAnchor;

    [Header("Как найти верёвки")]
    [Tooltip("Если пусто и массив 'ropes' не задан, возьмём всех TapePiece в сцене.")]
    [SerializeField] private Transform ropesRoot;
    [SerializeField] private TapePiece[] ropes;

    [Header("Настройки прикрепления")]
    [Tooltip("Сохранять мировые позицию/поворот/масштаб при SetParent.")]
    [SerializeField] private bool worldPositionStays = true;
    [Tooltip("Если X ровно равен X разрушенной — отправлять направо (иначе налево).")]
    [SerializeField] private bool equalGoesRight = false;

    private bool _done;

    [SerializeField] private Vector2 postZeroDelayRange = new Vector2(0.4f, 1.2f); // случайная задержка
    [SerializeField] private float leftMoveSpeed = 2.5f;                            // скорость влево (м/с)
    [SerializeField] private Animator deathAnimator;                                // аниматор, который дёргаем
    [SerializeField] private string deathTrigger = "Death";                         // имя триггера
    private bool _moveLeftActive;                                                   // флаг движения

    [SerializeField] private AudioSource DeathAudioSource;

    [SerializeField] private AudioClip giliotina;
    [SerializeField] private AudioClip HeadCut;

    private void Awake()
    {
        if (manager == null) manager = FindFirstObjectByType<RopesHealthManager>();

        if ((ropes == null || ropes.Length == 0))
        {
            if (ropesRoot != null)
                ropes = ropesRoot.GetComponentsInChildren<TapePiece>(includeInactive: false);
            else
                ropes = FindObjectsByType<TapePiece>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }
    }

    private void OnEnable()
    {
        if (manager != null)
            manager.OnFirstHit0 += HandleFirstZero;
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.OnFirstHit0 -= HandleFirstZero;
    }

    private void HandleFirstZero(TapePiece destroyed)
    {
        if (_done || destroyed == null || leftAnchor == null || rightAnchor == null) return;
        _done = true;

        float pivotX = destroyed.transform.position.x;

        foreach (var r in ropes)
        {
            if (r == null || r == destroyed) continue;

            float x = r.transform.position.x;

            // Определяем сторону  
            bool goRight = x > pivotX || (Mathf.Approximately(x, pivotX) && equalGoesRight);
            var targetParent = goRight ? rightAnchor : leftAnchor;

            // Просто прикрепляем (без движения)  
            r.transform.SetParent(targetParent, worldPositionStays);
        }
        StartCoroutine(PostZeroSequence(destroyed));
    }

    private System.Collections.IEnumerator PostZeroSequence(TapePiece destroyed)
    {
        // случайная задержка
        DeathAudioSource.Play();
        
        float delay = Random.Range(postZeroDelayRange.x, postZeroDelayRange.y);
        if (delay > 0f) yield return new WaitForSeconds(delay);

        // 1) уничтожаем кусок destroyed
        if (destroyed != null) Destroy(destroyed.gameObject);

        // 2) включаем движение левого трансформа влево
        _moveLeftActive = true;
        DeathAudioSource.clip = giliotina;
        DeathAudioSource.Play();
        // 3) триггерим аниматор
        if (deathAnimator != null && !string.IsNullOrEmpty(deathTrigger))
            deathAnimator.SetTrigger(deathTrigger);
    }

    private void Update()
    {
        if (_moveLeftActive && leftAnchor != null && leftMoveSpeed > 0f)
            leftAnchor.Translate(Vector3.left * leftMoveSpeed * Time.deltaTime, Space.World);
    }
}
