using UnityEngine;
using UnityEngine.AI;
using EspduSender;
using System.Net;


public class Bots : MonoBehaviour
{
    public NavMeshAgent[] bots;
    public NavMeshAgent Bot;
    public int numbots;
    public ParticleSystem explosionEffect;
    public Sender sender;

    // Start is called before the first frame update
    void Start()
    {
        bots = new NavMeshAgent[numbots];

        for(int i = 0; i < numbots; i++)
        {
            Vector3 spawnPosition =
                UnityEngine.Random.insideUnitCircle * numbots * 0.9f;
            bots[i] =
               Instantiate(Bot, spawnPosition, Quaternion.identity) as
               NavMeshAgent;
            bots[i].name = "Bot";
            bots[i].GetComponent<NPCMove>().agent = Bot;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ParticleSystem exp = explosionEffect;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, LayerMask.NameToLayer("Terrain")))
            {
                exp.transform.position = hit.point;
                exp.Play();
                if(hit.collider.tag == "AI" || hit.collider.name == "Bot")
                {
                    Destroy(hit.collider.gameObject);
                }
            }
        }
    }
}
