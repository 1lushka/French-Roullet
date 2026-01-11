using UnityEngine;

public class Axe : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float startSpeed = 5f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private int damage = 1;
    [SerializeField] private Transform target;
    [SerializeField] private float arcHeight = 2f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem ropeHitParticles;
    [SerializeField] private ParticleSystem defaultHitParticles;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] throwSounds;       
    [SerializeField] private AudioClip[] ropeHitSounds;
    [SerializeField] private AudioClip[] shieldHitSounds;

    [Header("Settings")]
    [SerializeField] private float hitCooldown = 0.3f;

    private bool isThrown = false;
    private Animator anim;
    private float currentSpeed;
    private TrailRenderer trail;
    private float lastHitTime = -999f;
    private Vector3 arcStart;

    private void Awake()
    {
        arcStart = transform.position;
        anim = GetComponent<Animator>();
        trail = GetComponent<TrailRenderer>();
        if (trail == null)
            trail = GetComponentInChildren<TrailRenderer>();
        if (trail != null)
            trail.enabled = false;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void Throw()
    {
        if (anim) anim.SetBool("Flying", true);

        isThrown = true;
        currentSpeed = startSpeed;
        if (trail != null)
            trail.enabled = true;

        // 🎵 Звук броска
        PlayRandomSound(throwSounds);
    }

    void Update()
    {
        if (isThrown)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
            Vector3 targetPos = transform.position + transform.forward * -currentSpeed * Time.deltaTime;
            targetPos = ApplyArcHeight(targetPos);
            ObjectMover.MoveTo(transform, targetPos);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastHitTime < hitCooldown)
            return;

        lastHitTime = Time.time;
        isThrown = false;

        if (trail != null)
            trail.enabled = false;

        if (anim)
        {
            anim.SetBool("Flying", false);
            anim.SetBool("InShield", false);
        }

        TapePiece tape = collision.gameObject.GetComponent<TapePiece>();
        if (tape == null)
            tape = collision.gameObject.GetComponentInParent<TapePiece>();

        if (tape != null)
        {
            tape.TakeDamage(damage);

            if (ropeHitParticles != null)
                Instantiate(ropeHitParticles, collision.contacts[0].point, Quaternion.identity);

            PlayRandomSound(ropeHitSounds);
        }
        else
        {
            if (defaultHitParticles != null)
                Instantiate(defaultHitParticles, collision.contacts[0].point, Quaternion.identity);

            if (anim)
                anim.SetBool("InShield", true);


            PlayRandomSound(shieldHitSounds);
        }
    }

    private void PlayRandomSound(AudioClip[] clips)
    {
        if (audioSource == null || clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }

    private Vector3 ApplyArcHeight(Vector3 currentPos)
    {
        if (target == null) return currentPos;

        Vector3 a = arcStart;
        Vector3 b = target.position;
        b.y = a.y;

        Vector3 ab = b - a;
        float abLenSqr = ab.sqrMagnitude;
        if (abLenSqr < 1e-6f) return currentPos;



        float s = Mathf.Clamp01(Vector3.Dot(currentPos - a, ab) / abLenSqr);
        float yOffset = 4f * arcHeight * s * (1f - s);

        currentPos.y = a.y + yOffset;

        return currentPos;
    }


}
