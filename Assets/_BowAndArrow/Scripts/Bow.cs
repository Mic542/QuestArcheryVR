using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Bow : MonoBehaviour
{
    public static Bow Instance;

    [Header("Assets")]
    public GameObject m_ArrowPrefab = null;
    public TextMeshProUGUI m_UIAmmoTxt;
    public TextMeshProUGUI m_ScoreUI;
    public TextMeshProUGUI m_GetPointText;
    public GameObject m_StartingObj;
    public TextMeshProUGUI m_Title;
    public GameObject m_ImageTitle;

    [Header("Bow")]
    public float m_GrabThreshold = 0.15f;
    public Transform m_Start = null;
    public Transform m_End = null;
    public Transform m_Socket = null;

    [Header("Other Hand")]
    public Transform m_OtherHand = null;

    public Transform m_ArrowInstantiateLoc;

    private Transform m_PullingHand = null;
    private Arrow m_CurrArrow = null;
    private Animator m_Animator = null;

    private float m_PullValue = 0.0f;
    public bool isAttached = false;
    public bool InGame { get; set; } = false;

    [Header("Arrow Box")]
    public Transform ArrowBoxLoc1;
    public Transform ArrowBoxLoc2;
    public Transform ArrowBoxLoc3;
    public GameObject moveHereText;
    public GameObject ArrowBox;

    public GameObject ArrowWoGrab;

    [Header("Barrel Highlight")]
    public List<GameObject> BarrelOutline;

    [Header("Particle Highlight")]
    public ParticleSystem particle;

    public int m_MaxAmmo = 10;

    private int m_Score { get; set; } = 0;
    private bool first = true; //if this is the first run aka just launched

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public void StartGame()
    {
        InGame = true;
        m_ImageTitle.GetComponent<TitleScript>().Out();
        m_StartingObj.SetActive(false);
        m_Score = 0;
        m_MaxAmmo = 10;

        moveHereText.SetActive(true);

        CreateArrowBox();

        m_ScoreUI.text = "Score " + m_Score;

        setOutline(true);
        particle.Play();

        if (m_CurrArrow != null)
        {
            isAttached = false;
            Destroy(m_CurrArrow.gameObject);
            m_CurrArrow = null;
        }
    }

    private void Start()
    {
        CreateArrow();
        m_UIAmmoTxt.text = "Arrow x" + m_MaxAmmo;
        m_StartingObj.SetActive(true);
        m_ImageTitle.GetComponent<TitleScript>().In();

        setOutline(false);
    }

    public void setOutline(bool value)
    {
        foreach (GameObject outline in BarrelOutline)
        {
            outline.GetComponent<Outline>().enabled = value;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void CreateArrow()
    {
        GameObject arrowObj = Instantiate(ArrowWoGrab, m_Socket.transform);

        arrowObj.transform.localPosition = new Vector3(0, 0, 0.425f);
        arrowObj.transform.localEulerAngles = Vector3.zero;

        AttachBowToArrow(arrowObj);
    }

    public void AttachBowToArrow(GameObject arrow)
    {
        if (isAttached || m_MaxAmmo <= 0) return;

        GameObject arrowObj = arrow;
        m_CurrArrow = arrowObj.GetComponent<Arrow>();

        m_CurrArrow.transform.parent = m_Socket.transform;
        m_CurrArrow.transform.localPosition = new Vector3(0, 0, 0.425f);
        m_CurrArrow.transform.localEulerAngles = Vector3.zero;

        if (InGame) m_MaxAmmo--;
        m_UIAmmoTxt.text = "Arrow x" + m_MaxAmmo;
        isAttached = true;
        moveHereText.SetActive(false);
        particle.Stop();
    }

    public void CreateArrowBox()
    {
        for (int i = 0; i < 10; i++)
        {
            Instantiate(m_ArrowPrefab, ArrowBoxLoc1);
            Instantiate(m_ArrowPrefab, ArrowBoxLoc2);
            Instantiate(m_ArrowPrefab, ArrowBoxLoc3);
        }
    }

    public void DestroyPreviousArrowBox()
    {
        foreach (GameObject fooObj in GameObject.FindGameObjectsWithTag("Arrow"))
        {
            Destroy(fooObj);
        }
    }

    public void AddScore(int score)
    {
        m_Score += score;
        m_ScoreUI.text = "Score " + m_Score;
    }

    IEnumerator ResetHitText()
    {
        yield return new WaitForSeconds(2.0f);
        m_GetPointText.text = "";
    }

    public void PopUpTextPoint(int point)
    {
        m_GetPointText.text = "+" + point;
        StartCoroutine(ResetHitText());
    }

    public void Pull(Transform hand)
    {
        if (!isAttached) return;
        float distance = Vector3.Distance(hand.position, m_Start.position);
        if (distance > m_GrabThreshold) return;
        m_PullingHand = hand;
    }

    public void Release()
    {
        if (m_PullValue > 0.25f)
        {
            FireArrow();
        } else
        {
            return;
        }

        m_PullingHand = null;
        m_PullValue = 0.0f;
        m_Animator.SetFloat("Blend", 0);
    }

    private void Update()
    {
        if (!m_PullingHand || !m_CurrArrow) return;

        m_PullValue = CalculatePull(m_PullingHand);
        m_PullValue = Mathf.Clamp(m_PullValue, 0.0f, 1.0f);

        m_Animator.SetFloat("Blend", m_PullValue);
    }

    private float CalculatePull(Transform pullHand)
    {
        Vector3 direction = m_End.position - m_Start.position;
        float magnitude = direction.magnitude;

        direction.Normalize();

        Vector3 diff = pullHand.position - m_Start.position;

        return Vector3.Dot(diff, direction) / magnitude;
    }

    private void FireArrow()
    {
        m_CurrArrow.Fire(m_PullValue);
        m_CurrArrow = null;

        isAttached = false;

        if (!InGame)
        {
            CreateArrow();
        }

        if (m_MaxAmmo.Equals(0))
        {
            StartCoroutine(WaitForLastArrow());
        }
    }

    IEnumerator WaitForLastArrow()
    {
        yield return new WaitForSeconds(2.0f);
        if (m_MaxAmmo.Equals(0))
        {
            m_UIAmmoTxt.text = "Game Over";
            m_MaxAmmo = 10;
            m_StartingObj.SetActive(true);
            m_Title.gameObject.SetActive(true);
            InGame = false;
            CreateArrow();
        }
    }
}
