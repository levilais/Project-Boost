﻿using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour {

    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 100f;
    [SerializeField] float levelLoadDelay = 2f;

    [SerializeField] AudioClip mainEngineSound;
    [SerializeField] AudioClip deathSound;
    [SerializeField] AudioClip successSound;

    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem deathParticles;
    [SerializeField] ParticleSystem successParticles;

    AudioSource audioSource;
    Rigidbody rigidBody;

    enum State { Alive, Dying, Transcending };
    State state = State.Alive;

    bool collisionOn = true;
    bool debugOn = true;

	// Use this for initialization
    void Start () 
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (state == State.Alive)
        {
            RespondToThrustInput();
            RespondToRotateInput();
            RespondToDebugInput();
        }
	}

    private void RespondToDebugInput()
    {
        if (debugOn == true)
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                LoadNextScene();
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                toggleCollisionOn();
            }
        }
    }

    private void toggleCollisionOn()
    {
        collisionOn = !collisionOn;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive) { return; } // ignore collisions when dead

        switch (collision.gameObject.tag) 
        {
            case "Friendly":
                // do nothing
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                if (collisionOn == true)
                {
                    StartDeathSequence();
                }
                break;
        }
    }

    private void StartSuccessSequence()
    {
        audioSource.PlayOneShot(successSound);
        successParticles.Play();
        Invoke("LoadNextScene", levelLoadDelay);
        state = State.Transcending;
    }

    private void StartDeathSequence()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(deathSound);
        deathParticles.Play();
        Invoke("LoadFirstLevel", levelLoadDelay);
        state = State.Dying;
    }


    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(1); // todo allow formore than 2 levels
    }

    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space)) // can thrust while rotating
        {
            ApplyThrust();
        }
        else 
        {
            audioSource.Stop();
            mainEngineParticles.Stop();
        }
    }

    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngineSound);
        }
        mainEngineParticles.Play();
    }

    private void RespondToRotateInput()
    {
        rigidBody.freezeRotation = true; // take manual control of rotation

        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }

        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }

        rigidBody.freezeRotation = false; // resume physics control of rotation
    }
}
