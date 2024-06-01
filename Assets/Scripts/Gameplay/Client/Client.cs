using System;
using erulathra;
using UnityEngine;

public enum ClientState
{
    WantsBeer,
    Unsatisfied,
    DrinkingBeer,
    MugReturned
}

public class Client : MonoBehaviour
{
    public event Action<ClientState> OnClientStateChanged;
    public event Action<Client> OnClientExitedBar;
    
    [SerializeField]
    private ClientHandTrigger clientHandTrigger;
    
    [field: SerializeField]
    public bool IsRightClient { get; private set; }
    
    public float WalkSpeed { get; private set; }
    public float ReturnSpeed { get; private set; }
    public float MugReturnSpeed { get; private set; }
    
    private float timeSpawned;
    private float timeReturningStarted;
    private Vector3 startReturnPosition;

    private ClientQueue clientQueue;
    public MugComponent CollectedMug { get; private set; }

    
    private ClientState state = ClientState.WantsBeer;
    public void SetState(ClientState newState)
    {
        state = newState;
        OnClientStateChanged?.Invoke(state);
    }

    public void Initialize(ClientQueue queue)
    {
        clientQueue = queue;
        timeSpawned = Time.timeSinceLevelLoad;
        ReturnSpeed = GameplaySettings.Global.ClientReturnSpeed;
        MugReturnSpeed = GameplaySettings.Global.ClientReturnMugSpeed;

        state = ClientState.WantsBeer;
        clientHandTrigger.enabled = true;

        ClientSubsystem clientSubsystem = SceneSubsystemManager.GetSubsystem<ClientSubsystem>();
        WalkSpeed = clientSubsystem.CurrentClientSpeed;
    }

    public void Awake()
    {
        clientHandTrigger.OnMugCollected += OnMugCollected;
    }

    private void Update()
    {
        if (state == ClientState.WantsBeer)
        {
            HandleGoToBarLogic();
        }
        else if (state is ClientState.DrinkingBeer
                       or ClientState.Unsatisfied)
        {
            HandleReturnLogic();
        }
    }

    private void HandleGoToBarLogic()
    {
        float queueLength = clientQueue.length;

        float elapsedTime = Time.timeSinceLevelLoad - timeSpawned;
        float queueFract = elapsedTime * WalkSpeed / queueLength;

        transform.position = Vector3.Lerp(
            clientQueue.startPoint.position,
            clientQueue.endPoint.position,
            queueFract
        );

        if (queueFract >= 1f - Single.Epsilon)
        {
            SetState(ClientState.Unsatisfied);
            ReturnToExit();
        }
    }

    private void HandleReturnLogic()
    {
        float queueLength = Vector3.Distance(startReturnPosition, clientQueue.returnQueueEndPoint.position);

        float elapsedTime = Time.timeSinceLevelLoad - timeReturningStarted;
        float queueFract = elapsedTime * ReturnSpeed / queueLength;

        transform.position = Vector3.Lerp(
            startReturnPosition,
            clientQueue.returnQueueEndPoint.position,
            queueFract
        );

        if (queueFract >= 1f - Single.Epsilon)
        {
            ExitTavern();
        }
    }

    private void OnMugCollected(MugComponent mug)
    {
        if (mug.gameObject.activeSelf &&
            mug.FillPercentage < GameplaySettings.Global.MinimalMugFillAmount)
        {
            SetState(ClientState.Unsatisfied);
        }
        else
        {
            CollectedMug = mug;
            SetState(ClientState.DrinkingBeer);
        }
        
        mug.gameObject.SetActive(false);
        
        clientHandTrigger.enabled = false;
        ReturnToExit();
    }

    private void ReturnToExit()
    {
        timeReturningStarted = Time.timeSinceLevelLoad;
        startReturnPosition = clientQueue.NearestReturnQueuePoint(transform.position);
        transform.rotation = clientQueue.returnQueueStartPoint.rotation;
    }

    private void ExitTavern()
    {
        if (state == ClientState.DrinkingBeer)
        {
            ReturnMug();

            ScoreSubsystem scoreSubsystem = SceneSubsystemManager.GetSubsystem<ScoreSubsystem>();
            scoreSubsystem.AddPoint();
        }

        if (state == ClientState.Unsatisfied)
        {
            HeartsSubsystem heartsSubsystem = SceneSubsystemManager.GetSubsystem<HeartsSubsystem>();
            heartsSubsystem.HandleUnsatisfiedClient();
        }

        OnClientExitedBar?.Invoke(this);
    }

    private void ReturnMug()
    {
        SetState(ClientState.MugReturned);
        clientQueue.ReturnMug(this);
        CollectedMug.StartSliding();
        CollectedMug = null;
    }
}
