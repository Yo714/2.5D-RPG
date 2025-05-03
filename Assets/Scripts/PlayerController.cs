using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer playerSprite;
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
    private PartyManager partyManager;
    private Vector3 scale;

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

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void OnDestroy()
    {
        playerControls.Dispose();
    }

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        partyManager = GameObject.FindFirstObjectByType<PartyManager>();

        if(partyManager.GetPosition() != Vector3.zero)
        {
            transform.position = partyManager.GetPosition();
        }
    }

    void Update()
    {
        float x = playerControls.Player.Move.ReadValue<Vector2>().x;
        float z = playerControls.Player.Move.ReadValue<Vector2>().y;

        movement = new Vector3(x, 0f, z);

        anim.SetBool(IS_RUN_PARAM, movement != Vector3.zero);

        if(x!= 0 && x < 0)
        {
            playerSprite.transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
        }
        else if(x!= 0 && x > 0)
        {
            playerSprite.transform.localScale = new Vector3(scale.x, scale.y, scale.z);
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
                    partyManager.SetPosition(transform.position);
                    SceneManager.LoadScene(BATTLE_SCENE);
                }
            }
        }
    }

    private void CalculateStepsToEncounter()
    {
        stepsToEncounter = Random.Range(minStepsToEncounter, maxStepsToEncounter);
    }

    public void SetOverworldVisuals(Animator animator, SpriteRenderer spriteRenderer, Vector3 playerScale)
    {
        anim = animator;
        playerSprite = spriteRenderer;
        scale = playerScale;
    }
}
