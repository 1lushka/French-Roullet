using UnityEngine;

public class Crowd4FrameToggleRenderers : MonoBehaviour
{
    [SerializeField] private Renderer[] frameRenderers; // 4 рендера кадров
    [SerializeField] private float fps = 8f;
    [SerializeField] private bool randomStartOffset = true;

    private float offset;
    private int lastIndex = -1;

    private void Awake()
    {
        offset = randomStartOffset ? Random.value * 10f : 0f;
        SetFrame(0);
    }

    private void Update()
    {
        int index = (int)((Time.time + offset) * fps) % frameRenderers.Length;
        if (index != lastIndex) SetFrame(index);
    }

    private void SetFrame(int index)
    {
        lastIndex = index;
        for (int i = 0; i < frameRenderers.Length; i++)
            frameRenderers[i].enabled = (i == index);
    }
}
