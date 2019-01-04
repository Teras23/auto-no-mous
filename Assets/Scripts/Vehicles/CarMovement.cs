using System;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private bool _isDed;

    public bool isManual = false;

    public float drag = 2f;
    public float angularDrag = 1.1f;

    public float speedMultiplier = 20f;

    public float rotMultiplierLimit = 1.1f;
    public float RotationMultiplier =>
        -Math.Min(_rigidBody.velocity.magnitude * 0.1f, Math.Abs(rotMultiplierLimit) < 0.1 ? float.MaxValue : rotMultiplierLimit);

    private Rigidbody2D _rigidBody;
    private float?[] _sensorOutput;


    // Start is called before the first frame update
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _rigidBody.angularDrag = drag;
        _rigidBody.drag = angularDrag;
    }

    // Update is called once per frame
    void Update()
    {
        if (isManual)
        {
            Turn(Input.GetAxis("Horizontal"));
            Gas(Input.GetAxis("Vertical"));
        }
    }

    void FixedUpdate()
    {
        // do sth with sensors
    }

    public void Turn(float amount)
    {
        _rigidBody.AddTorque(amount * RotationMultiplier);
    }

    public void Gas(float amount)
    {
        _rigidBody.AddForce(transform.up * amount * speedMultiplier);
    }

    public void PlayParticleEffect()
    {
        var particles = GetComponent<ParticleSystem>();
        if (particles != null && !particles.isPlaying)
        {
            particles.Play();
        }
    }

    public void StopParticleEffect()
    {
        var particles = GetComponent<ParticleSystem>();
        if (particles!= null && particles.isPlaying)
        {
            particles.Stop();
        }
    }

    public void CueDejaVu()
    {
        var music = GetComponent<AudioSource>();
        if (music != null && !music.isPlaying)
        {
            music.Play();
        }
    }

    public void StopDejaVu()
    {
        var music = GetComponent<AudioSource>();
        if (music != null && music.isPlaying)
        {
            music.Stop();
        }
    }

    public void Die()
    {
        _isDed = true;
    }

    public bool IsDed() => _isDed;
}
