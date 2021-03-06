﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour {
    Rigidbody2D rb;
    Animator anim;

    private enum State {
        idle,
        running,
        jumping,
        falling,
        hurt,
    }

    private State state = State.idle;
    private Collider2D coll;
    [SerializeField] private LayerMask ground;
    [SerializeField] private float speed = 7f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] public int monetine = 0;
    [SerializeField] private TextMeshProUGUI monetineText;
    [SerializeField] private float hurtForce = 10f;
    [SerializeField] private int health;
    [SerializeField] private TextMeshProUGUI healthAmount;
    [SerializeField] public int indizi = 0;
    [SerializeField] private TextMeshProUGUI indiziText;
    [SerializeField] private AudioSource passi;
    [SerializeField] private AudioSource monetinaSound;
    [SerializeField] private AudioSource saltoSound;
    [SerializeField] private AudioSource colpitoSound;
    [SerializeField] private AudioSource indizioSound;
    private TextMeshProUGUI testoIndizio;
    private bool fadeIn = false;
    private bool fadeOut = false;
    private float fadeSpeed = 0.1f;
    private Color color;
    private List<string> volevi = new List<string>();
    private int count = 0;


    private void Start() {
        PlayerPrefs.SetString("scenaPrecedente", SceneManager.GetActiveScene().name);
        
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        healthAmount.text = health.ToString();
        indiziText.text = indizi.ToString();

        testoIndizio = GameObject.FindWithTag("Player").GetComponentInChildren<TextMeshProUGUI>();
        testoIndizio.enabled = false;


        if (Application.loadedLevelName == "PrimoLivello"){
            monetine = 0;
            PlayerPrefs.SetInt("monetine", monetine);
            monetineText.text = monetine.ToString();
        }
        else{
            monetine = PlayerPrefs.GetInt("monetine");
            monetineText.text = monetine.ToString();
        }
        volevi.Add("Online non sono reperibili informazioni personali");
        volevi.Add("L’utente fa utilizzo di una Virtual Private Network");
        volevi.Add("L’utente utilizza la Verifica a due passaggi");
    }

    private void Update() {
        if (Time.deltaTime != 0) {

            if (state != State.hurt)
            {
                Movement();
            }

            AnimationState();

            anim.SetInteger("state", (int)state); //Imposto l'animazione grazie all'enum
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Monetine")){
            monetinaSound.Play();
            Destroy(collision.gameObject);
            monetine++;
            monetineText.text = monetine.ToString();
        }

        if (collision.CompareTag("Indizi")){
            indizioSound.Play();
            indizi++;
            indiziText.text = indizi.ToString();
            Destroy(collision.gameObject);
            testoIndizio.enabled = true;
            if (SceneManager.GetActiveScene().name.Equals("QuintoLivello")){
               testoIndizio.SetText(volevi[count]);
            }

            count++;
            StartCoroutine(Fade());
        }
    }

    private IEnumerator Fade() {
        yield return new WaitForSeconds(3);
        testoIndizio.enabled = false;
    }


    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Enemy")){
            Nemici enemy = other.gameObject.GetComponent<Nemici>();
            if (state == State.falling){
                enemy.JumpedOn();
                saltoSound.Play();
                Jump();
            }
            else{
                state = State.hurt;
                colpitoSound.Play();
                HandleHealth();
                if (other.gameObject.transform.position.x > transform.position.x){
                    //Nemico alla mia destra. Mi sposta a sinistra
                    rb.velocity = new Vector2(-hurtForce, rb.velocity.y);
                }
                else{
                    //Nemico alla mia sinistra. Mi sposta a destra
                    rb.velocity = new Vector2(hurtForce, rb.velocity.y);
                }
            }
            
        }
    }

    private void HandleHealth() {
        health -= 1;
        healthAmount.text = health.ToString();
        if (health <= 0){
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void Movement() {
        float hDirection = Input.GetAxis("Horizontal");

        //Movimento sinistra
        if (hDirection < 0){
            rb.velocity = new Vector2(-speed, rb.velocity.y);
            transform.localScale = new Vector2(-1, 1);
        }
        //Movimento destra
        else if (hDirection > 0){
            rb.velocity = new Vector2(speed, rb.velocity.y);
            transform.localScale = new Vector2(1, 1);
        }

        //Salto
        if (Input.GetButtonDown("Jump") && coll.IsTouchingLayers(ground)){
            Jump();
        }
        
    }

    private void Jump() {
        saltoSound.Play();
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        state = State.jumping;
    }

    private void AnimationState() {
        if (state == State.jumping){
            if (rb.velocity.y < .1f){
                state = State.falling;
            }
        }
        else if (state == State.falling){
            if (coll.IsTouchingLayers(ground)){
                state = State.idle;
            }
        }else if (state == State.hurt){
            if (Mathf.Abs(rb.velocity.x) < .1f){
                state = State.idle;
            }
        }
        else if (Mathf.Abs(rb.velocity.x) > 2f){
            //Movimento
            state = State.running;
        }
        else{
            state = State.idle;
        }
        
    }

    private void Passi() {
       passi.Play(); 
    }
}
