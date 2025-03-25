using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private LayerMask grassLayer;
    [SerializeField] private int stepsInGrass;
    [SerializeField] private int minStepsToEncounter;
    [SerializeField] private int maxStepsToEncounter;
    
    private PlayerControls playerControls;
    private Rigidbody rb;
    private Vector3 movement;
    private bool movingInGrass;
    private float stepTimer;
    private int stepsToEncounter;

    private const string IS_RUN_PARAM = "IsRun";
    private const string BATTLE_SCENE = "BattleScene";
    private const float TIME_PER_STEP = 0.5f;

    private void Awake()
    {
        playerControls = new PlayerControls();
        CalculateStepsToEncounter();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        float x = playerControls.Player.Move.ReadValue<Vector2>().x;
        float z = playerControls.Player.Move.ReadValue<Vector2>().y;

        movement = new Vector3(x, 0f, z);

        anim.SetBool(IS_RUN_PARAM, movement != Vector3.zero);

        if(x!= 0 && x < 0)
        {
            playerRenderer.flipX = true;
        }
        else if(x!= 0 && x > 0)
        {
            playerRenderer.flipX = false;
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + movement * moveSpeed * Time.fixedDeltaTime);

        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f, grassLayer);
        movingInGrass = colliders.Length != 0 && movement != Vector3.zero;

        if(movingInGrass == true)
        {
            stepTimer += Time.deltaTime;
            if(stepTimer >= TIME_PER_STEP)
            {
                stepsInGrass++;
                stepTimer = 0;

                if(stepsInGrass >= stepsToEncounter)
                {
                    stepsInGrass = 0;
                    SceneManager.LoadScene(BATTLE_SCENE);
                }
            }
        }
    }

    private void CalculateStepsToEncounter()
    {
        stepsToEncounter = Random.Range(minStepsToEncounter, maxStepsToEncounter);
    }
}
