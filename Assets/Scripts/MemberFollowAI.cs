using UnityEngine;

public class MemberFollowAI : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private int speed;

    private float followDist;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Vector3 scale;

    private const string IS_RUN_PARAM = "IsRun";

    private void Awake() 
    {
        scale = transform.localScale;
    }

    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        followTarget = GameObject.FindFirstObjectByType<PlayerController>().transform;
    }

    void FixedUpdate()
    {
        if(Vector3.Distance(transform.position, followTarget.position) > followDist)
        {
            anim.SetBool(IS_RUN_PARAM, true);
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, followTarget.position, step);

            if(followTarget.position.x - transform.position.x < 0)
            {
                spriteRenderer.transform.localScale = new Vector3(scale.x, scale.y, scale.z);
            }
            else
            {
                spriteRenderer.transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
            }
        }
        else
        {
            anim.SetBool(IS_RUN_PARAM, false);
        }
    }

    public void SetFollowDistance(float followDistance)
    {
        followDist = followDistance;
    }
}
