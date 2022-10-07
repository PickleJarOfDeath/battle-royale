using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Rendering.PostProcessing;

public class PlayerController : MonoBehaviourPun
{
    [Header("Info")]
    public int id;
    private int curAttackerId;

    [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;
    public int curHP;
    public int maxHP;
    public int kills;
    public bool dead;

    private bool flashingDamage;
    public bool takingDamage;

    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;
    public PlayerWeapon weapon;
    public MeshRenderer mr;
    // public PlayerCondition playerCondition;


    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.players[id - 1] = this;

        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            // GetComponentInChildren<PostProcessVolume>().gameObject.SetActive(false);
            rig.isKinematic = true;
        }
        else
        {
            GameUI.instance.Initialize(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine || dead)
            return;

        Move();

        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();

        if (Input.GetMouseButtonDown(0))
            weapon.TryShoot();
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
        dir.y = rig.velocity.y;

        rig.velocity = dir;
    }

    void TryJump()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, 1.5f))
        {
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    [PunRPC]

    public void TakeDamage(int attackerId, int damage)
    {
        if (dead)
        {
            return;
        }

        curHP -= damage;
        curAttackerId = attackerId;

        photonView.RPC("DamageFlash", RpcTarget.Others);

        GameUI.instance.UpdateHealthBar();

        TakeDamageVis();

        if (curHP <= 0)
            photonView.RPC("Die", RpcTarget.All);


    }

    void TakeDamageVis()
    {
        if (takingDamage)
            return;

        StartCoroutine(TakingDamageCoroutine());

        IEnumerator TakingDamageCoroutine()
        {
            takingDamage = true;
            yield return new WaitForSeconds(0.05f);
            flashingDamage = false;
        }

        takingDamage = false;
    }

    [PunRPC]
    void DamageFlash()
    {
        if (flashingDamage)
            return;

        StartCoroutine(DamageFlashCoRoutine());

        IEnumerator DamageFlashCoRoutine()
        {
            flashingDamage = true;

            Color defaultColor = mr.material.color;
            mr.material.color = Color.red;

            yield return new WaitForSeconds(0.05f);

            mr.material.color = defaultColor;
            flashingDamage = false;
        }
    }

    [PunRPC]

    void Die()
    {
        curHP = 0;
        dead = true;

        GameManager.instance.alivePlayers--;

        if (PhotonNetwork.IsMasterClient)
            GameManager.instance.CheckWinCondition();

        if (photonView.IsMine)
        {
            if (curAttackerId != 0)
                GameManager.instance.GetPlayer(curAttackerId).photonView.RPC("AddKill", RpcTarget.All);

            GetComponentInChildren<CameraController>().SetAsSpectator();

            rig.isKinematic = true;
            transform.position = new Vector3(0, -50, 0);
        }
    }

    [PunRPC]
    public void AddKill()
    {
        kills++;

        GameUI.instance.UpdatePlayerInfoText();
    }

    [PunRPC]
    public void Heal(int amountToHeal)
    {
        curHP += Mathf.Clamp( curHP + amountToHeal, 0, maxHP);

        GameUI.instance.UpdateHealthBar();
    }
}
