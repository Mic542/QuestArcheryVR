using UnityEngine;
using TMPro;

public class Arrow : MonoBehaviour
{
    public float m_Speed = 2000.0f;
    public Transform m_Tip = null;

    private Rigidbody m_Rigidbody = null;
    private bool m_IsStopped = true;
    private Vector3 m_LastPos = Vector3.zero;

    private OVRGrabbable m_Grabbable;

    private RaycastHit hits;

    public ParticleSystem m_Particle;
    public ParticleSystem m_AppleSplash;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Grabbable = GetComponent<OVRGrabbable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Bow")) return;
        Bow.Instance.AttachBowToArrow();
        Destroy(m_Grabbable);
    }

    private void FixedUpdate()
    {
        if (m_IsStopped) return;
        int mask = 1 << 8;
        mask = ~mask;

        m_Rigidbody.MoveRotation(Quaternion.LookRotation(m_Rigidbody.velocity, transform.up));

        if (Physics.Linecast(m_LastPos, m_Tip.position, out hits, mask))
        {
            int getScore = 0;
            var m_Target = hits.collider.gameObject.GetComponent<TargetScript>();
            if (hits.collider.gameObject.tag.Equals("Target") && Bow.Instance.InGame)
            {
                getScore = m_Target.point;
                m_Particle.Play();
                Bow.Instance.PopUpTextPoint(getScore);
            }

            if (hits.collider.gameObject.tag.Equals("Apple"))
            {
                m_AppleSplash.Play();
                Bow.Instance.StartGame();
                Destroy(gameObject);
            }

            Bow.Instance.AddScore(getScore);
            Stop();
        }

        m_LastPos = m_Tip.position;
    }

    private void Stop()
    {
        m_IsStopped = true;

        m_Rigidbody.isKinematic = true;
        m_Rigidbody.useGravity = false;
    }

    public void Fire(float pullValue)
    {
        m_IsStopped = false;
        transform.parent = null;

        m_Rigidbody.isKinematic = false;
        m_Rigidbody.useGravity = true;
        m_Rigidbody.AddForce(transform.forward * (pullValue * m_Speed));

        if (!Bow.Instance.InGame) Destroy(gameObject, 5.0f);

        Destroy(gameObject, 15f);
    }
}
