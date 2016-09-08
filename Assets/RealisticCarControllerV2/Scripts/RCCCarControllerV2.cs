#pragma warning disable 0414

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class RCCCarControllerV2 : MonoBehaviour
{

    //Rigidbody.
    private Rigidbody rigid;

    //Mobile Controller.
    //public bool mobileController = false;
    //public MobileGUIType _mobileControllerType;
    //public enum MobileGUIType{UIController, NGUIController}

    //Dashboard Type.
    public bool dashBoard = false;
    public DashboardType _dashboardType;
    public enum DashboardType { UIDashboard, NGUIDashboard }

    public bool useAccelerometerForSteer = false, steeringWheelControl = false;
    //public float gyroTiltMultiplier = 2.0f;

    //Display Information GUI
    public bool demoGUI = false;

    private Vector3 defbrakePedalPosition;
    //private bool mobileHandbrake = false;

    //Finds all buttons when new vehicle spawned. Useful for spawning new vehicles on your scene at runtime.
    //public bool autoFindButtons = true;

    //NGUI Controller Elements.
    //If boost UI is selected, will multiply default engine torque by 1.5.
    public RCCNGUIController gasPedal, brakePedal, leftArrow, rightArrow, handbrakeGui, boostGui;
    //UI Controller Elements.
    public RCCUIController gasPedalUI, brakePedalUI, leftArrowUI, rightArrowUI, handbrakeUI, boostUI;

    // Wheel Transforms Of The Vehicle.
    public Transform FrontLeftWheelTransform;
    public Transform FrontRightWheelTransform;
    public Transform RearLeftWheelTransform;
    public Transform RearRightWheelTransform;

    //Wheel Colliders Of The Vehicle.
    public WheelCollider FrontLeftWheelCollider;
    public WheelCollider FrontRightWheelCollider;
    public WheelCollider RearLeftWheelCollider;
    public WheelCollider RearRightWheelCollider;
    private WheelCollider[] allWheelColliders;

    //Extra Wheels. In case of if your vehicle has extra wheels.
    //public Transform[] ExtraRearWheelsTransform;
    //public WheelCollider[] ExtraRearWheelsCollider;

    ////Applies Engine Torque To Extra Rear Wheels.
    //public bool applyEngineTorqueToExtraRearWheelColliders = true;

    // Driver Steering Wheel. In case of if your vehicle has individual steering wheel model in interior.
    public Transform SteeringWheel;

    // Set wheel drive of the vehicle. If you are using rwd, you have to be careful with your rear wheel collider
    // settings and com of the vehicle. Otherwise, vehicle will behave like a toy. ***My advice is use fwd always***
    public WheelType _wheelTypeChoise;
    public enum WheelType { FWD, RWD, AWD }

    //Center of mass.
    public Transform COM;

    //Enables/Disables controlling the vehicle.
    public bool canControl = true;
    //Enables/Disables auto reversing when player press brake button. Useful for if you are making parking style game.
    public bool autoReverse = false;
    //Enables/Disables automatic gear shifting of the vehicle.
    public bool automaticGear = true;
    private bool canGoReverseNow = false;

    public AnimationCurve[] engineTorqueCurve;
    public float[] gearSpeed;
    public float engineTorque = 2500.0f;
    public float maxEngineRPM = 6000.0f;
    public float minEngineRPM = 1000.0f;

    //Maximum steer angle of your vehicle.
    public float steerAngle = 40.0f;
    //Maximum steer angle at highest speed.
    public float highspeedsteerAngle = 10.0f;
    //Maximum speed for steer angle.
    public float highspeedsteerAngleAtspeed = 100.0f;

    //Anti Roll Force for preventing flip overs.
    public float antiRoll = 10000.0f;

    public float speed;
    public float brake = 4000.0f;
    public float maxspeed = 180.0f;

    //Enables/Disables differential of the vehicle. For more information, google it.
    public bool useDifferential = true;
    private float differentialRatioRight;
    private float differentialRatioLeft;
    private float differentialDifference;

    private float resetTime = 0f;
    private float defSteerAngle = 0f;

    //Gears.
    private int previousGear;
    public int currentGear;
    public int totalGears = 6;
    public bool changingGear = false;

    //If your vehicle shifts your gear too soon, lower shift rate. If shifts too late, higher shift rate.
    public float gearShiftRate = 12.0f;
    float[] transmissionRate = {
        0f,
        13.6f,
        7.7f,
        5.4f,
        4.1f,
        3.3f
    };

    public float idleEngineTorque = 20.0f;
    public AnimationCurve[] idleEngineTorqueCurve;


    // Each Wheel Transform's Rotation Value.
    private float _rotationValueFL, _rotationValueFR, _rotationValueRL, _rotationValueRR;
    private float[] rotationValueExtra;

    //Private Bools.
    private bool reversing = false;
    private bool headLightsOn = false;
    private float acceleration = 0f;
    private float lastVelocity = 0f;

    //Audio.
    private AudioSource skidSound;
    public AudioClip skidClip;
    private AudioSource crashSound;
    public AudioClip[] crashClips;
    private AudioSource engineStartSound;
    public AudioClip engineStartClip;
    private AudioSource engineSound;
    public AudioClip engineClip;
    private AudioSource gearShiftingSound;
    public AudioClip[] gearShiftingClips;

    //Collision Force Limit.
    private int collisionForceLimit = 5;

    //Inputs.
    [HideInInspector]
    public float motorInput = 0f;
    [HideInInspector]
    public float steerInput = 0f;
    [HideInInspector]
    public float boostInput = 1f;
    [HideInInspector]
    public float engineRPM = 0f;

    //UI DashBoard.
    public RCCDashboardInputs UIInputs;
    private RectTransform RPMNeedle;
    private RectTransform KMHNeedle;
    private float RPMNeedleRotation = 0.0f;
    private float KMHNeedleRotation = 0.0f;
    private float smoothedNeedleRotation = 0.0f;

    //NGUI Dashboard.
    public GameObject RPMNeedleNGUI;
    public GameObject KMHNeedleNGUI;
    public float minimumRPMNeedleAngle = 20.0f;
    public float maximumRPMNeedleAngle = 160.0f;
    public float minimumKMHNeedleAngle = -25.0f;
    public float maximumKMHNeedleAngle = 155.0f;

    //Smokes.
    public GameObject wheelSlipPrefab;
    private List<ParticleSystem> _wheelParticles = new List<ParticleSystem>();
    public ParticleSystem[] exhaustGas;

    //Script will simulate chassis movement based on vehicle rigidbody situation.
    public GameObject chassis;
    //Chassis Vertical Lean Sensitivity
    public float chassisVerticalLean = 4.0f;
    //Chassis Horizontal Lean Sensitivity
    public float chassisHorizontalLean = 4.0f;

    private float horizontalLean = 0.0f;
    private float verticalLean = 0.0f;

    //Lights.
    public Light[] headLights;
    public Light[] brakeLights;
    public Light[] reverseLights;

    //Steering Wheel Controller.
    public float steeringWheelMaximumsteerAngle = 180.0f;
    public float steeringWheelGuiScale = 256f;
    private float _steeringWheelGuiScale = 0f;
    public float steeringWheelXOffset = 30.0f;
    public float steeringWheelYOffset = 30.0f;
    public Vector2 steeringWheelPivotPos = Vector2.zero;
    public float steeringWheelResetPosspeed = 200.0f;
    public Texture2D steeringWheelTexture;
    private float steeringWheelsteerAngle;
    private bool steeringWheelIsTouching;
    private Rect steeringWheelTextureRect;
    private Vector2 steeringWheelWheelCenter;
    private float steeringWheelOldAngle;
    private int touchId = -1;
    private Vector2 touchPos;


    void Start()
    {
        //canControl = false;
        rigid = GetComponent<Rigidbody>();
        Time.fixedDeltaTime = .02f;
        rigid.maxAngularVelocity = 5f;
        allWheelColliders = GetComponentsInChildren<WheelCollider>();
        defSteerAngle = steerAngle;

        if (dashBoard)
        {

            UIInputs = GameObject.FindObjectOfType<RCCDashboardInputs>();
            UIInputs.GetNeedles();

            if (_dashboardType == DashboardType.NGUIDashboard)
            {
                RPMNeedleNGUI = UIInputs.RPMNeedleNGUI;
                KMHNeedleNGUI = UIInputs.KMHNeedleNGUI;
            }
            else
            {
                RPMNeedle = UIInputs.RPMNeedleUI;
                KMHNeedle = UIInputs.KMHNeedleUI;
            }

        }

        //init gear
        previousGear = currentGear;

        // init idle engine torque curve
        //idleEngineTorqueCurve = new AnimationCurve(new Keyframe(0, 1)); 
        //idleEngineTorqueCurve.MoveKey(0, new Keyframe(0, 1));
        //idleEngineTorqueCurve.AddKey(8f, 0f);
        //idleEngineTorqueCurve.AddKey(5f, .75f);
        //idleEngineTorqueCurve.postWrapMode = WrapMode.Clamp;

        SoundsInitialize();
        SteeringWheelInit();
        SmokeInit();

    }

    public AudioSource CreateAudioSource(string audioName, float minDistance, float volume, AudioClip audioClip, bool loop, bool playNow, bool destroyAfterFinished)
    {

        GameObject audioSource = new GameObject(audioName);
        audioSource.transform.position = transform.position;
        audioSource.transform.rotation = transform.rotation;
        audioSource.transform.parent = transform;
        audioSource.AddComponent<AudioSource>();
        audioSource.GetComponent<AudioSource>().minDistance = minDistance;
        audioSource.GetComponent<AudioSource>().volume = volume;
        audioSource.GetComponent<AudioSource>().clip = audioClip;
        audioSource.GetComponent<AudioSource>().loop = loop;
        audioSource.GetComponent<AudioSource>().spatialBlend = 1f;

        if (playNow)
            audioSource.GetComponent<AudioSource>().Play();

        if (destroyAfterFinished)
            Destroy(audioSource, audioClip.length);

        return audioSource.GetComponent<AudioSource>();

    }

    public void CreateWheelColliders()
    {

        List<Transform> allWheelTransforms = new List<Transform>();
        allWheelTransforms.Add(FrontLeftWheelTransform); allWheelTransforms.Add(FrontRightWheelTransform); allWheelTransforms.Add(RearLeftWheelTransform); allWheelTransforms.Add(RearRightWheelTransform);

        if (allWheelTransforms[0] == null)
        {
            Debug.LogError("You haven't choose your Wheel Transforms. Please select all of your Wheel Transforms before creating Wheel Colliders. Script needs to know their positions, aye?");
            return;
        }

        transform.rotation = Quaternion.identity;

        GameObject WheelColliders = new GameObject("Wheel Colliders");
        WheelColliders.transform.parent = transform;
        WheelColliders.transform.rotation = transform.rotation;
        WheelColliders.transform.localPosition = Vector3.zero;
        WheelColliders.transform.localScale = Vector3.one;

        foreach (Transform wheel in allWheelTransforms)
        {

            GameObject wheelcollider = new GameObject(wheel.transform.name);

            wheelcollider.transform.position = wheel.transform.position;
            wheelcollider.transform.rotation = transform.rotation;
            wheelcollider.transform.name = wheel.transform.name;
            wheelcollider.transform.parent = WheelColliders.transform;
            wheelcollider.transform.localScale = Vector3.one;
            wheelcollider.layer = LayerMask.NameToLayer("Wheel");
            wheelcollider.AddComponent<WheelCollider>();
            wheelcollider.GetComponent<WheelCollider>().radius = (wheel.GetComponent<MeshRenderer>().bounds.size.y / 2f) / transform.localScale.y;

            wheelcollider.AddComponent<RCCWheelSkidmarks>();
            wheelcollider.GetComponent<RCCWheelSkidmarks>().vehicle = GetComponent<RCCCarControllerV2>();

            JointSpring spring = wheelcollider.GetComponent<WheelCollider>().suspensionSpring;

            spring.spring = 30000f;
            spring.damper = 2000f;

            wheelcollider.GetComponent<WheelCollider>().suspensionSpring = spring;
            wheelcollider.GetComponent<WheelCollider>().suspensionDistance = .2f;
            wheelcollider.GetComponent<WheelCollider>().forceAppPointDistance = .25f;
            wheelcollider.GetComponent<WheelCollider>().mass = 100f;
            wheelcollider.GetComponent<WheelCollider>().wheelDampingRate = .5f;

            wheelcollider.transform.localPosition = new Vector3(wheelcollider.transform.localPosition.x, wheelcollider.transform.localPosition.y + (wheelcollider.GetComponent<WheelCollider>().suspensionDistance / 2f), wheelcollider.transform.localPosition.z);

            WheelFrictionCurve sidewaysFriction = wheelcollider.GetComponent<WheelCollider>().sidewaysFriction;
            WheelFrictionCurve forwardFriction = wheelcollider.GetComponent<WheelCollider>().forwardFriction;

            forwardFriction.extremumSlip = .4f;
            forwardFriction.extremumValue = 1;
            forwardFriction.asymptoteSlip = .8f;
            forwardFriction.asymptoteValue = .75f;
            forwardFriction.stiffness = 1.75f;

            sidewaysFriction.extremumSlip = .25f;
            sidewaysFriction.extremumValue = 1;
            sidewaysFriction.asymptoteSlip = .5f;
            sidewaysFriction.asymptoteValue = .75f;
            sidewaysFriction.stiffness = 2f;

            wheelcollider.GetComponent<WheelCollider>().sidewaysFriction = sidewaysFriction;
            wheelcollider.GetComponent<WheelCollider>().forwardFriction = forwardFriction;

        }

        WheelColliders.layer = LayerMask.NameToLayer("Wheel");

        WheelCollider[] allWheelColliders = new WheelCollider[allWheelTransforms.Count];
        allWheelColliders = GetComponentsInChildren<WheelCollider>();

        FrontLeftWheelCollider = allWheelColliders[0];
        FrontRightWheelCollider = allWheelColliders[1];
        RearLeftWheelCollider = allWheelColliders[2];
        RearRightWheelCollider = allWheelColliders[3];

    }

    public void SoundsInitialize()
    {

        engineSound = CreateAudioSource("Engine Sound AudioSource", 5, 0, engineClip, true, true, false);
        skidSound = CreateAudioSource("Skid Sound AudioSource", 5, 0, skidClip, true, true, false);

    }

    public void KillOrStartEngine(int i)
    {

        if (i == 0)
        {
            canControl = false;
        }
        else
        {
            canControl = true;
            StartEngineSound();
        }

    }

    public void StartEngineSound()
    {

        engineStartSound = CreateAudioSource("Engine Start AudioSource", 5, 1, engineStartClip, false, true, true);

    }

    public void SteeringWheelInit()
    {

        _steeringWheelGuiScale = ((Screen.width * 1.0f) / 2.7f) * (steeringWheelGuiScale / 256f);
        steeringWheelIsTouching = false;
        steeringWheelTextureRect = new Rect(steeringWheelXOffset + (_steeringWheelGuiScale / Screen.width), -steeringWheelYOffset + (Screen.height - (_steeringWheelGuiScale)), _steeringWheelGuiScale, _steeringWheelGuiScale);
        steeringWheelWheelCenter = new Vector2(steeringWheelTextureRect.x + steeringWheelTextureRect.width * 0.5f, Screen.height - steeringWheelTextureRect.y - steeringWheelTextureRect.height * 0.5f);
        steeringWheelsteerAngle = 0f;

    }

    public void SmokeInit()
    {

        if (!wheelSlipPrefab)
            return;

        for (int i = 0; i < allWheelColliders.Length; i++)
        {
            GameObject ps = (GameObject)Instantiate(wheelSlipPrefab, transform.position, transform.rotation) as GameObject;
            _wheelParticles.Add(ps.GetComponent<ParticleSystem>());
            ps.GetComponent<ParticleSystem>().enableEmission = false;
            ps.transform.parent = allWheelColliders[i].transform;
            ps.transform.localPosition = Vector3.zero;
        }

    }

    void Update()
    {

        if (canControl)
        {
            KeyboardControlling();
            Lights();
            ResetCar();
            ShiftGears();
        }

        WheelAlign();
        SkidAudio();
        WheelCamber();

        if (chassis)
            Chassis();
        if (dashBoard && canControl)
            DashboardGUI();

    }

    void FixedUpdate()
    {
        Braking();
        Differential();
        AntiRollBars();
        SmokeWeedEveryday();

        if (canControl)
        {
            Engine();
        }
        else
        {
            RearLeftWheelCollider.motorTorque = 0;
            RearRightWheelCollider.motorTorque = 0;
            FrontLeftWheelCollider.motorTorque = 0;
            FrontRightWheelCollider.motorTorque = 0;
            RearLeftWheelCollider.brakeTorque = brake / 12f;
            RearRightWheelCollider.brakeTorque = brake / 12f;
            FrontLeftWheelCollider.brakeTorque = brake / 12f;
            FrontRightWheelCollider.brakeTorque = brake / 12f;
            engineSound.volume = Mathf.Lerp(engineSound.volume, 0f, Time.deltaTime);
            engineSound.pitch = Mathf.Lerp(engineSound.pitch, 0f, Time.deltaTime);
        }

    }

    void ReleaseBrake()
    {
        reversing = false;

        RearLeftWheelCollider.brakeTorque = 0;
        RearRightWheelCollider.brakeTorque = 0;
        FrontLeftWheelCollider.brakeTorque = 0;
        FrontRightWheelCollider.brakeTorque = 0;
    }

    public void Engine()
    {

        //Speed.
        speed = rigid.velocity.magnitude * 3.6f;

        //Acceleration Calculation.
        acceleration = 0f;
        acceleration = (transform.InverseTransformDirection(rigid.velocity).z - lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = transform.InverseTransformDirection(rigid.velocity).z;

        //Drag Limit Depends On Vehicle Acceleration.
        rigid.drag = Mathf.Clamp((acceleration / 50f), 0f, .4f);

        //Steer Limit.
        steerAngle = Mathf.Lerp(defSteerAngle, highspeedsteerAngle, (speed / highspeedsteerAngleAtspeed));

        //Engine RPM.
        engineRPM = Mathf.Clamp(/*(((Mathf.Abs((RearLeftWheelCollider.rpm + RearRightWheelCollider.rpm)) * gearShiftRate) + minEngineRPM)) / (float)(currentGear + 1), */
            Mathf.Abs((RearLeftWheelCollider.rpm + RearRightWheelCollider.rpm)) / 2 * transmissionRate[currentGear]/* + minEngineRPM*/,
            minEngineRPM, maxEngineRPM);

        //Reversing Bool.
        if (motorInput < 0 && RearLeftWheelCollider.rpm < 20 && canGoReverseNow)
            reversing = true;
        else
            reversing = false;

        //Auto Reverse Bool.
        if (autoReverse)
        {
            canGoReverseNow = true;
        }
        else
        {
            if (motorInput >= -.1f && speed < 5)
                canGoReverseNow = true;
            else if (motorInput < 0 && transform.InverseTransformDirection(rigid.velocity).z > 1)
                canGoReverseNow = false;
        }

        //Engine Audio Volume.
        if (engineSound)
        {

            if (!reversing)
                engineSound.volume = Mathf.Lerp(engineSound.volume, Mathf.Clamp(motorInput, .35f, .85f), Time.deltaTime * 5f);
            else
                engineSound.volume = Mathf.Lerp(engineSound.volume, Mathf.Clamp(Mathf.Abs(motorInput), .35f, .85f), Time.deltaTime * 5f);

            engineSound.pitch = Mathf.Lerp(engineSound.pitch, Mathf.Lerp(1f, 2f, (engineRPM - minEngineRPM / 1.25f) / (maxEngineRPM + minEngineRPM)), Time.deltaTime * 5f);

        }

        #region Wheel Type Motor Torque.

        //Applying WheelCollider Motor Torques Depends On Wheel Type Choice.
        switch (_wheelTypeChoise)
        {

            case WheelType.FWD:
                FrontLeftWheelCollider.motorTorque = ApplyWheelTorque(true);
                FrontRightWheelCollider.motorTorque = ApplyWheelTorque(false);
                break;
            case WheelType.RWD:
                RearLeftWheelCollider.motorTorque = ApplyWheelTorque(true);
                RearRightWheelCollider.motorTorque = ApplyWheelTorque(false);
                break;
            case WheelType.AWD:
                FrontLeftWheelCollider.motorTorque = ApplyWheelTorque(true);
                FrontRightWheelCollider.motorTorque = ApplyWheelTorque(false);
                RearLeftWheelCollider.motorTorque = ApplyWheelTorque(true);
                RearRightWheelCollider.motorTorque = ApplyWheelTorque(false);
                break;

        }

        //if(ExtraRearWheelsCollider.Length > 0 && applyEngineTorqueToExtraRearWheelColliders){
        //	foreach(WheelCollider wc in ExtraRearWheelsCollider)
        //		wc.motorTorque = ApplyWheelTorque(true);
        //}

        #endregion Wheel Type

    }

    public float ApplyWheelTorque(bool leftSide)
    {

        if (speed > maxspeed || Mathf.Abs(FrontLeftWheelCollider.rpm) > 3000 || Mathf.Abs(RearLeftWheelCollider.rpm) > 3000)
            return 0;

        if (reversing && speed > 30)
            return 0;

        float torque = 0;

        if (changingGear)
        {
            torque = 0;
        }
        else
        {
            if (!reversing)
            {
                if (leftSide)
                {
                    torque = (engineTorque) * (Mathf.Clamp(motorInput * differentialRatioLeft, 0f, 1f) * boostInput) * engineTorqueCurve[currentGear].Evaluate(speed);
                }
                else
                    torque = (engineTorque) * (Mathf.Clamp(motorInput * differentialRatioRight, 0f, 1f) * boostInput) * engineTorqueCurve[currentGear].Evaluate(speed);
            }
            else
            {
                torque = engineTorque * motorInput;
            }
        }
        torque += idleEngineTorque * idleEngineTorqueCurve[currentGear].Evaluate(speed) * transmissionRate[currentGear];
        //Debug.Log("torque = " + torque);

        return torque;

    }

    public void Braking()
    {

        //Handbrake
        if (Input.GetButton("Jump") /*|| mobileHandbrake*/)
        {

            //FrontLeftWheelCollider.brakeTorque = (brake / 2.5f);
            //FrontRightWheelCollider.brakeTorque = (brake / 2.5f);
            RearLeftWheelCollider.brakeTorque = (brake);
            RearRightWheelCollider.brakeTorque = (brake);

            //Normal brake
        }
        else
        {
            Debug.Log("motorInput = " + motorInput + " currentGear = " + currentGear + " previousGear = " + previousGear);
            // Deceleration.
            if (Mathf.Abs(motorInput) <= .05f && !changingGear && speed > 20)
            {
                RearLeftWheelCollider.brakeTorque = (brake) / 25f;
                RearRightWheelCollider.brakeTorque = (brake) / 25f;
                FrontLeftWheelCollider.brakeTorque = (brake) / 25f;
                FrontRightWheelCollider.brakeTorque = (brake) / 25f;

                //Debug.Log("motorInput = " + motorInput);
            }
            else if (motorInput < 0 && !reversing)  // Braking
            {
                FrontLeftWheelCollider.brakeTorque = (brake) * (Mathf.Abs(motorInput));
                FrontRightWheelCollider.brakeTorque = (brake) * (Mathf.Abs(motorInput));
                RearLeftWheelCollider.brakeTorque = (brake) * (Mathf.Abs(motorInput / 2f));
                RearRightWheelCollider.brakeTorque = (brake) * (Mathf.Abs(motorInput / 2f));
            }
            else if (currentGear < previousGear)
            {
                AddBrakingForce();
            }
            else
            {
                RearLeftWheelCollider.brakeTorque = 0;
                RearRightWheelCollider.brakeTorque = 0;
                FrontLeftWheelCollider.brakeTorque = 0;
                FrontRightWheelCollider.brakeTorque = 0;
            }

        }

    }

    public void Differential()
    {

        if (useDifferential)
        {

            if (_wheelTypeChoise == WheelType.FWD)
            {
                differentialDifference = Mathf.Clamp(Mathf.Abs(FrontRightWheelCollider.rpm) - Mathf.Abs(FrontLeftWheelCollider.rpm), -50f, 50f);
                differentialRatioRight = Mathf.Lerp(0f, 1f, ((((Mathf.Abs(FrontRightWheelCollider.rpm) + Mathf.Abs(FrontLeftWheelCollider.rpm)) + 10 / 2) + differentialDifference) / (Mathf.Abs(FrontRightWheelCollider.rpm) + Mathf.Abs(FrontLeftWheelCollider.rpm))));
                differentialRatioLeft = Mathf.Lerp(0f, 1f, ((((Mathf.Abs(FrontRightWheelCollider.rpm) + Mathf.Abs(FrontLeftWheelCollider.rpm)) + 10 / 2) - differentialDifference) / (Mathf.Abs(FrontRightWheelCollider.rpm) + Mathf.Abs(FrontLeftWheelCollider.rpm))));
            }
            if (_wheelTypeChoise == WheelType.RWD)
            {
                differentialDifference = Mathf.Clamp(Mathf.Abs(RearRightWheelCollider.rpm) - Mathf.Abs(RearLeftWheelCollider.rpm), -50f, 50f);
                differentialRatioRight = Mathf.Lerp(0f, 1f, ((((Mathf.Abs(RearRightWheelCollider.rpm) + Mathf.Abs(RearLeftWheelCollider.rpm)) + 10 / 2) + differentialDifference) / (Mathf.Abs(RearRightWheelCollider.rpm) + Mathf.Abs(RearLeftWheelCollider.rpm))));
                differentialRatioLeft = Mathf.Lerp(0f, 1f, ((((Mathf.Abs(RearRightWheelCollider.rpm) + Mathf.Abs(RearLeftWheelCollider.rpm)) + 10 / 2) - differentialDifference) / (Mathf.Abs(RearRightWheelCollider.rpm) + Mathf.Abs(RearLeftWheelCollider.rpm))));
            }
            if (_wheelTypeChoise == WheelType.AWD)
            {
                differentialDifference = Mathf.Clamp(Mathf.Abs(RearRightWheelCollider.rpm) - Mathf.Abs(RearLeftWheelCollider.rpm), -50f, 50f);
                differentialRatioRight = Mathf.Lerp(0f, 1f, ((((Mathf.Abs(RearRightWheelCollider.rpm) + Mathf.Abs(RearLeftWheelCollider.rpm)) + 10 / 2) + differentialDifference) / (Mathf.Abs(RearRightWheelCollider.rpm) + Mathf.Abs(RearLeftWheelCollider.rpm))));
                differentialRatioLeft = Mathf.Lerp(0f, 1f, ((((Mathf.Abs(RearRightWheelCollider.rpm) + Mathf.Abs(RearLeftWheelCollider.rpm)) + 10 / 2) - differentialDifference) / (Mathf.Abs(RearRightWheelCollider.rpm) + Mathf.Abs(RearLeftWheelCollider.rpm))));
            }

        }
        else
        {

            differentialRatioRight = 1;
            differentialRatioLeft = 1;

        }

    }

    public void AntiRollBars()
    {

        WheelHit FrontWheelHit;

        float travelFL = 1.0f;
        float travelFR = 1.0f;

        bool groundedFL = FrontLeftWheelCollider.GetGroundHit(out FrontWheelHit);

        if (groundedFL)
            travelFL = (-FrontLeftWheelCollider.transform.InverseTransformPoint(FrontWheelHit.point).y - FrontLeftWheelCollider.radius) / FrontLeftWheelCollider.suspensionDistance;

        bool groundedFR = FrontRightWheelCollider.GetGroundHit(out FrontWheelHit);

        if (groundedFR)
            travelFR = (-FrontRightWheelCollider.transform.InverseTransformPoint(FrontWheelHit.point).y - FrontRightWheelCollider.radius) / FrontRightWheelCollider.suspensionDistance;

        float antiRollForceFront = (travelFL - travelFR) * antiRoll;

        if (groundedFL)
            rigid.AddForceAtPosition(FrontLeftWheelCollider.transform.up * -antiRollForceFront, FrontLeftWheelCollider.transform.position);
        if (groundedFR)
            rigid.AddForceAtPosition(FrontRightWheelCollider.transform.up * antiRollForceFront, FrontRightWheelCollider.transform.position);

        WheelHit RearWheelHit;

        float travelRL = 1.0f;
        float travelRR = 1.0f;

        bool groundedRL = RearLeftWheelCollider.GetGroundHit(out RearWheelHit);

        if (groundedRL)
            travelRL = (-RearLeftWheelCollider.transform.InverseTransformPoint(RearWheelHit.point).y - RearLeftWheelCollider.radius) / RearLeftWheelCollider.suspensionDistance;

        bool groundedRR = RearRightWheelCollider.GetGroundHit(out RearWheelHit);

        if (groundedRR)
            travelRR = (-RearRightWheelCollider.transform.InverseTransformPoint(RearWheelHit.point).y - RearRightWheelCollider.radius) / RearRightWheelCollider.suspensionDistance;

        float antiRollForceRear = (travelRL - travelRR) * antiRoll;

        if (groundedRL)
            rigid.AddForceAtPosition(RearLeftWheelCollider.transform.up * -antiRollForceRear, RearLeftWheelCollider.transform.position);
        if (groundedRR)
            rigid.AddForceAtPosition(RearRightWheelCollider.transform.up * antiRollForceRear, RearRightWheelCollider.transform.position);

        if (groundedRR && groundedRL)
            rigid.AddRelativeTorque((Vector3.up * (steerInput)) * 1000f);

    }

    public void SteeringWheelControlling()
    {

        if (steeringWheelIsTouching)
        {

            foreach (Touch touch in Input.touches)
            {
                if (touch.fingerId == touchId)
                {
                    touchPos = touch.position;

                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        steeringWheelIsTouching = false;
                    }
                }
            }

            float newsteerAngle = Vector2.Angle(Vector2.up, touchPos - steeringWheelWheelCenter);

            if (Vector2.Distance(touchPos, steeringWheelWheelCenter) > 20f)
            {
                if (touchPos.x > steeringWheelWheelCenter.x)
                    steeringWheelsteerAngle -= newsteerAngle - steeringWheelOldAngle;
                else
                    steeringWheelsteerAngle += newsteerAngle - steeringWheelOldAngle;
            }

            if (steeringWheelsteerAngle > steeringWheelMaximumsteerAngle)
                steeringWheelsteerAngle = steeringWheelMaximumsteerAngle;
            else if (steeringWheelsteerAngle < -steeringWheelMaximumsteerAngle)
                steeringWheelsteerAngle = -steeringWheelMaximumsteerAngle;

            steeringWheelOldAngle = newsteerAngle;

        }
        else
        {

            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    if (steeringWheelTextureRect.Contains(new Vector2(touch.position.x, Screen.height - touch.position.y)))
                    {
                        steeringWheelIsTouching = true;
                        steeringWheelOldAngle = Vector2.Angle(Vector2.up, touch.position - steeringWheelWheelCenter);
                        touchId = touch.fingerId;
                    }
                }
            }

            if (!Mathf.Approximately(0f, steeringWheelsteerAngle))
            {
                float deltaAngle = steeringWheelResetPosspeed * Time.deltaTime;

                if (Mathf.Abs(deltaAngle) > Mathf.Abs(steeringWheelsteerAngle))
                {
                    steeringWheelsteerAngle = 0f;
                    return;
                }

                if (steeringWheelsteerAngle > 0f)
                    steeringWheelsteerAngle -= deltaAngle;
                else
                    steeringWheelsteerAngle += deltaAngle;
            }

        }

    }

    public void KeyboardControlling()
    {

        //Motor Input.
        if (!changingGear)
            motorInput = (Input.GetAxis("Vertical"));
        else
            motorInput = Mathf.Clamp(Input.GetAxis("Vertical"), -1f, 0f);

        //Steering Input.
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > .05f)
            steerInput = Mathf.Lerp(steerInput, Input.GetAxis("Horizontal"), Time.deltaTime * 10);
        else
            steerInput = Mathf.Lerp(steerInput, Input.GetAxis("Horizontal"), Time.deltaTime * 10);

        //Boost Input.
        if (Input.GetButton("Fire2"))
            boostInput = 1.5f;
        else
            boostInput = 1f;

        //Release brake
        if (Input.GetKeyDown(KeyCode.K))
        {
            ReleaseBrake();
        }

        FrontLeftWheelCollider.steerAngle = (steerAngle * steerInput);
        FrontRightWheelCollider.steerAngle = (steerAngle * steerInput);

    }

    public void ShiftGears()
    {
        if (automaticGear)
        {
            if (currentGear < totalGears - 1 && !changingGear)
            {
                if (speed > gearSpeed[currentGear + 1] && RearLeftWheelCollider.rpm >= 0)
                {
                    StartCoroutine("ChangingGear", currentGear + 1);
                }
            }

            if (currentGear > 0)
            {
                if (engineRPM < minEngineRPM + 1000 && !changingGear)
                {

                    for (int i = 0; i < gearSpeed.Length; i++)
                    {
                        if (speed > gearSpeed[i])
                            StartCoroutine("ChangingGear", i);
                    }

                }
            }
        }
        else
        {
            if (currentGear < totalGears - 1 && !changingGear)
            {
                if (Input.GetButtonDown("RCCShiftUp"))
                {
                    previousGear = currentGear;
                    StartCoroutine("ChangingGear", currentGear + 1);
                }
            }

            if (currentGear > 0)
            {
                if (Input.GetButtonDown("RCCShiftDown"))
                {
                    previousGear = currentGear;
                    StartCoroutine("ChangingGear", currentGear - 1);
                }
            }
        }
    }

    IEnumerator ChangingGear(int gear)
    {

        changingGear = true;

        if (gearShiftingClips.Length > 0)
        {

            gearShiftingSound = CreateAudioSource("Gear Shifting AudioSource", 5f, .3f, gearShiftingClips[UnityEngine.Random.Range(0, gearShiftingClips.Length)], false, true, true);

        }

        yield return new WaitForSeconds(.35f);
        changingGear = false;
        currentGear = gear;
        Debug.Log("previousGear = " + previousGear + " currentGear = " + currentGear);
    }

    // add braking force util speed down to reasonable range
    void AddBrakingForce()
    {
        Debug.Log("AddBrakingForce");
        RearLeftWheelCollider.brakeTorque = (brake) / 25f;
        RearRightWheelCollider.brakeTorque = (brake) / 25f;
        FrontLeftWheelCollider.brakeTorque = (brake) / 25f;
        FrontRightWheelCollider.brakeTorque = (brake) / 25f;
    }

    public void WheelAlign()
    {

        RaycastHit hit;
        WheelHit CorrespondingGroundHit;


        //Front Left Wheel Transform.
        Vector3 ColliderCenterPointFL = FrontLeftWheelCollider.transform.TransformPoint(FrontLeftWheelCollider.center);
        FrontLeftWheelCollider.GetGroundHit(out CorrespondingGroundHit);

        if (Physics.Raycast(ColliderCenterPointFL, -FrontLeftWheelCollider.transform.up, out hit, (FrontLeftWheelCollider.suspensionDistance + FrontLeftWheelCollider.radius) * transform.localScale.y))
        {
            if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Vehicle"))
            {
                FrontLeftWheelTransform.transform.position = hit.point + (FrontLeftWheelCollider.transform.up * FrontLeftWheelCollider.radius) * transform.localScale.y;
                float extension = (-FrontLeftWheelCollider.transform.InverseTransformPoint(CorrespondingGroundHit.point).y - FrontLeftWheelCollider.radius) / FrontLeftWheelCollider.suspensionDistance;
                Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point + FrontLeftWheelCollider.transform.up * (CorrespondingGroundHit.force / rigid.mass), extension <= 0.0 ? Color.magenta : Color.white);
                Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - FrontLeftWheelCollider.transform.forward * CorrespondingGroundHit.forwardSlip, Color.green);
                Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - FrontLeftWheelCollider.transform.right * CorrespondingGroundHit.sidewaysSlip, Color.red);
            }
        }
        else
        {
            FrontLeftWheelTransform.transform.position = ColliderCenterPointFL - (FrontLeftWheelCollider.transform.up * FrontLeftWheelCollider.suspensionDistance) * transform.localScale.y;
        }

        _rotationValueFL += FrontLeftWheelCollider.rpm * (6) * Time.deltaTime;
        FrontLeftWheelTransform.transform.rotation = FrontLeftWheelCollider.transform.rotation * Quaternion.Euler(_rotationValueFL, FrontLeftWheelCollider.steerAngle, FrontLeftWheelCollider.transform.rotation.z);


        //Front Right Wheel Transform.
        Vector3 ColliderCenterPointFR = FrontRightWheelCollider.transform.TransformPoint(FrontRightWheelCollider.center);
        FrontRightWheelCollider.GetGroundHit(out CorrespondingGroundHit);

        if (Physics.Raycast(ColliderCenterPointFR, -FrontRightWheelCollider.transform.up, out hit, (FrontRightWheelCollider.suspensionDistance + FrontRightWheelCollider.radius) * transform.localScale.y))
        {
            if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Vehicle"))
            {
                FrontRightWheelTransform.transform.position = hit.point + (FrontRightWheelCollider.transform.up * FrontRightWheelCollider.radius) * transform.localScale.y;
                float extension = (-FrontRightWheelCollider.transform.InverseTransformPoint(CorrespondingGroundHit.point).y - FrontRightWheelCollider.radius) / FrontRightWheelCollider.suspensionDistance;
                Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point + FrontRightWheelCollider.transform.up * (CorrespondingGroundHit.force / rigid.mass), extension <= 0.0 ? Color.magenta : Color.white);
                Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - FrontRightWheelCollider.transform.forward * CorrespondingGroundHit.forwardSlip, Color.green);
                Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - FrontRightWheelCollider.transform.right * CorrespondingGroundHit.sidewaysSlip, Color.red);
            }
        }
        else
        {
            FrontRightWheelTransform.transform.position = ColliderCenterPointFR - (FrontRightWheelCollider.transform.up * FrontRightWheelCollider.suspensionDistance) * transform.localScale.y;
        }

        _rotationValueFR += FrontRightWheelCollider.rpm * (6) * Time.deltaTime;
        FrontRightWheelTransform.transform.rotation = FrontRightWheelCollider.transform.rotation * Quaternion.Euler(_rotationValueFR, FrontRightWheelCollider.steerAngle, FrontRightWheelCollider.transform.rotation.z);


        //Rear Left Wheel Transform.
        Vector3 ColliderCenterPointRL = RearLeftWheelCollider.transform.TransformPoint(RearLeftWheelCollider.center);
        RearLeftWheelCollider.GetGroundHit(out CorrespondingGroundHit);

        if (Physics.Raycast(ColliderCenterPointRL, -RearLeftWheelCollider.transform.up, out hit, (RearLeftWheelCollider.suspensionDistance + RearLeftWheelCollider.radius) * transform.localScale.y))
        {
            if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Vehicle"))
            {
                RearLeftWheelTransform.transform.position = hit.point + (RearLeftWheelCollider.transform.up * RearLeftWheelCollider.radius) * transform.localScale.y;
                float extension = (-RearLeftWheelCollider.transform.InverseTransformPoint(CorrespondingGroundHit.point).y - RearLeftWheelCollider.radius) / RearLeftWheelCollider.suspensionDistance;
                Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point + RearLeftWheelCollider.transform.up * (CorrespondingGroundHit.force / rigid.mass), extension <= 0.0 ? Color.magenta : Color.white);
                Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - RearLeftWheelCollider.transform.forward * CorrespondingGroundHit.forwardSlip, Color.green);
                Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - RearLeftWheelCollider.transform.right * CorrespondingGroundHit.sidewaysSlip, Color.red);
            }
        }
        else
        {
            RearLeftWheelTransform.transform.position = ColliderCenterPointRL - (RearLeftWheelCollider.transform.up * RearLeftWheelCollider.suspensionDistance) * transform.localScale.y;
        }

        RearLeftWheelTransform.transform.rotation = RearLeftWheelCollider.transform.rotation * Quaternion.Euler(_rotationValueRL, 0, RearLeftWheelCollider.transform.rotation.z);
        _rotationValueRL += RearLeftWheelCollider.rpm * (6) * Time.deltaTime;


        //Rear Right Wheel Transform.
        Vector3 ColliderCenterPointRR = RearRightWheelCollider.transform.TransformPoint(RearRightWheelCollider.center);
        RearRightWheelCollider.GetGroundHit(out CorrespondingGroundHit);

        if (Physics.Raycast(ColliderCenterPointRR, -RearRightWheelCollider.transform.up, out hit, (RearRightWheelCollider.suspensionDistance + RearRightWheelCollider.radius) * transform.localScale.y))
        {
            if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Vehicle"))
            {
                RearRightWheelTransform.transform.position = hit.point + (RearRightWheelCollider.transform.up * RearRightWheelCollider.radius) * transform.localScale.y;
                float extension = (-RearRightWheelCollider.transform.InverseTransformPoint(CorrespondingGroundHit.point).y - RearRightWheelCollider.radius) / RearRightWheelCollider.suspensionDistance;
                Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point + RearRightWheelCollider.transform.up * (CorrespondingGroundHit.force / rigid.mass), extension <= 0.0 ? Color.magenta : Color.white);
                Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - RearRightWheelCollider.transform.forward * CorrespondingGroundHit.forwardSlip, Color.green);
                Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - RearRightWheelCollider.transform.right * CorrespondingGroundHit.sidewaysSlip, Color.red);
            }
        }
        else
        {
            RearRightWheelTransform.transform.position = ColliderCenterPointRR - (RearRightWheelCollider.transform.up * RearRightWheelCollider.suspensionDistance) * transform.localScale.y;
        }

        RearRightWheelTransform.transform.rotation = RearRightWheelCollider.transform.rotation * Quaternion.Euler(_rotationValueRR, 0, RearRightWheelCollider.transform.rotation.z);
        _rotationValueRR += RearRightWheelCollider.rpm * (6) * Time.deltaTime;

        //Driver SteeringWheel Transform.
        if (SteeringWheel)
            SteeringWheel.transform.rotation = transform.rotation * Quaternion.Euler(20, 0, (FrontLeftWheelCollider.steerAngle) * -6);

    }

    public void WheelCamber()
    {

        WheelHit CorrespondingGroundHit;

        FrontLeftWheelCollider.GetGroundHit(out CorrespondingGroundHit);
        float FLHandling = Mathf.Lerp(-1, 1, CorrespondingGroundHit.force / 8000f);
        FrontRightWheelCollider.GetGroundHit(out CorrespondingGroundHit);
        float FRHandling = Mathf.Lerp(-1, 1, CorrespondingGroundHit.force / 8000f);
        RearLeftWheelCollider.GetGroundHit(out CorrespondingGroundHit);
        float RLHandling = Mathf.Lerp(-1, 1, CorrespondingGroundHit.force / 8000f);
        RearRightWheelCollider.GetGroundHit(out CorrespondingGroundHit);
        float RRHandling = Mathf.Lerp(-1, 1, CorrespondingGroundHit.force / 8000f);

        FrontLeftWheelCollider.transform.localEulerAngles = new Vector3(FrontLeftWheelCollider.transform.localEulerAngles.x, FrontLeftWheelCollider.transform.localEulerAngles.y, (-FLHandling));
        FrontRightWheelCollider.transform.localEulerAngles = new Vector3(FrontRightWheelCollider.transform.localEulerAngles.x, FrontRightWheelCollider.transform.localEulerAngles.y, (FRHandling));
        RearLeftWheelCollider.transform.localEulerAngles = new Vector3(RearLeftWheelCollider.transform.localEulerAngles.x, RearLeftWheelCollider.transform.localEulerAngles.y, (-RLHandling));
        RearRightWheelCollider.transform.localEulerAngles = new Vector3(RearRightWheelCollider.transform.localEulerAngles.x, RearRightWheelCollider.transform.localEulerAngles.y, (RRHandling));

    }

    public void DashboardGUI()
    {

        if (_dashboardType == DashboardType.NGUIDashboard)
        {

            if (!UIInputs)
            {
                Debug.LogError("If you gonna use NGUI Dashboard, your NGUI Root must have ''RCCNGUIDashboardInputs''. First be sure your NGUI Root has ''RCCNGUIDashboardInputs.cs''.");
                dashBoard = false;
                return;
            }

            UIInputs.RPM = engineRPM;
            UIInputs.KMH = speed;
            UIInputs.Gear = FrontLeftWheelCollider.rpm > -10 ? currentGear : -1f;

            RPMNeedleRotation = Mathf.Lerp(minimumRPMNeedleAngle, maximumRPMNeedleAngle, (engineRPM - minEngineRPM / 1.5f) / (maxEngineRPM + minEngineRPM));
            KMHNeedleRotation = Mathf.Lerp(minimumKMHNeedleAngle, maximumKMHNeedleAngle, speed / maxspeed);
            smoothedNeedleRotation = Mathf.Lerp(smoothedNeedleRotation, RPMNeedleRotation, Time.deltaTime * 5);

            RPMNeedleNGUI.transform.eulerAngles = new Vector3(RPMNeedleNGUI.transform.eulerAngles.x, RPMNeedleNGUI.transform.eulerAngles.y, -smoothedNeedleRotation);
            KMHNeedleNGUI.transform.eulerAngles = new Vector3(KMHNeedleNGUI.transform.eulerAngles.x, KMHNeedleNGUI.transform.eulerAngles.y, -KMHNeedleRotation);

        }

        if (_dashboardType == DashboardType.UIDashboard)
        {

            if (!UIInputs)
            {
                Debug.LogError("If you gonna use UI Dashboard, your Canvas Root must have ''RCCUIDashboardInputs''. First be sure your Canvas Root has ''RCCUIDashboardInputs.cs''.");
                dashBoard = false;
                return;
            }

            UIInputs.RPM = engineRPM;
            UIInputs.KMH = speed;
            UIInputs.Gear = FrontLeftWheelCollider.rpm > -10 ? currentGear : -1f;

            RPMNeedleRotation = Mathf.Lerp(minimumRPMNeedleAngle, maximumRPMNeedleAngle, (engineRPM - minEngineRPM / 1.5f) / (maxEngineRPM + minEngineRPM));
            KMHNeedleRotation = Mathf.Lerp(minimumKMHNeedleAngle, maximumKMHNeedleAngle, speed / maxspeed);
            smoothedNeedleRotation = Mathf.Lerp(smoothedNeedleRotation, RPMNeedleRotation, Time.deltaTime * 5);

            RPMNeedle.transform.eulerAngles = new Vector3(RPMNeedle.transform.eulerAngles.x, RPMNeedle.transform.eulerAngles.y, -smoothedNeedleRotation);
            KMHNeedle.transform.eulerAngles = new Vector3(KMHNeedle.transform.eulerAngles.x, KMHNeedle.transform.eulerAngles.y, -KMHNeedleRotation);

        }

    }

    public void SmokeWeedEveryday()
    {

        for (int i = 0; i < allWheelColliders.Length; i++)
        {

            WheelHit CorrespondingGroundHit;
            allWheelColliders[i].GetGroundHit(out CorrespondingGroundHit);

            if (_wheelParticles.Count > 0)
            {

                if (Mathf.Abs(CorrespondingGroundHit.sidewaysSlip) > .25f || Mathf.Abs(CorrespondingGroundHit.forwardSlip) > .5f)
                {
                    if (!_wheelParticles[i].enableEmission && speed > 1)
                        _wheelParticles[i].enableEmission = true;
                }
                else
                {
                    if (_wheelParticles[i].enableEmission)
                        _wheelParticles[i].enableEmission = false;
                }

            }

        }

        if (exhaustGas.Length > 0 && canControl)
        {
            foreach (ParticleSystem p in exhaustGas)
            {
                if (speed < 20)
                {
                    if (!p.enableEmission)
                        p.enableEmission = true;
                    if (motorInput > .05f)
                        p.emissionRate = 10;
                    else
                        p.emissionRate = 4;
                }
                else
                {
                    if (p.enableEmission)
                        p.enableEmission = false;
                }
            }
        }
        else if (exhaustGas.Length > 0)
        {
            foreach (ParticleSystem p in exhaustGas)
            {
                if (p.enableEmission)
                    p.enableEmission = false;
            }
        }

    }

    public void SkidAudio()
    {

        if (!skidSound)
            return;

        WheelHit CorrespondingGroundHitF;
        FrontRightWheelCollider.GetGroundHit(out CorrespondingGroundHitF);

        WheelHit CorrespondingGroundHitR;
        RearRightWheelCollider.GetGroundHit(out CorrespondingGroundHitR);

        if (Mathf.Abs(CorrespondingGroundHitF.sidewaysSlip) > .25f || Mathf.Abs(CorrespondingGroundHitR.sidewaysSlip) > .25f || Mathf.Abs(CorrespondingGroundHitF.forwardSlip) > .7f || Mathf.Abs(CorrespondingGroundHitR.forwardSlip) > .7f)
        {
            if (rigid.velocity.magnitude > 1f)
                skidSound.volume = Mathf.Clamp((Mathf.Abs(CorrespondingGroundHitF.sidewaysSlip) + Mathf.Abs(CorrespondingGroundHitR.sidewaysSlip) / 5f) + Mathf.Abs(CorrespondingGroundHitF.forwardSlip) + Mathf.Abs(CorrespondingGroundHitR.forwardSlip), 0, 1f);
            else
                skidSound.volume -= Time.deltaTime;
        }
        else
        {
            skidSound.volume -= Time.deltaTime;
        }

    }

    public void ResetCar()
    {

        if (speed < 5 && !rigid.isKinematic)
        {

            if (transform.eulerAngles.z < 300 && transform.eulerAngles.z > 60)
            {
                resetTime += Time.deltaTime;
                if (resetTime > 3)
                {
                    transform.rotation = Quaternion.identity;
                    transform.position = new Vector3(transform.position.x, transform.position.y + 3, transform.position.z);
                    resetTime = 0f;
                }
            }

            if (transform.eulerAngles.x < 300 && transform.eulerAngles.x > 60)
            {
                resetTime += Time.deltaTime;
                if (resetTime > 3)
                {
                    transform.rotation = Quaternion.identity;
                    transform.position = new Vector3(transform.position.x, transform.position.y + 3, transform.position.z);
                    resetTime = 0f;
                }
            }

        }

    }

    void OnCollisionEnter(Collision collision)
    {

        if (collision.contacts.Length > 0)
        {

            if (collision.relativeVelocity.magnitude > collisionForceLimit && crashClips.Length > 0)
            {

                if (collision.contacts[0].thisCollider.gameObject.transform != transform.parent)
                {

                    crashSound = CreateAudioSource("Crash Sound AudioSource", 5, 1, crashClips[UnityEngine.Random.Range(0, crashClips.Length)], false, true, true);

                }

            }

        }

    }

    public void Chassis()
    {

        rigid.centerOfMass = new Vector3((COM.localPosition.x) * transform.localScale.x, (COM.localPosition.y) * transform.localScale.y, (COM.localPosition.z) * transform.localScale.z);

        verticalLean = Mathf.Clamp(Mathf.Lerp(verticalLean, rigid.angularVelocity.x * chassisVerticalLean, Time.deltaTime * 3f), -3.0f, 3.0f);

        WheelHit CorrespondingGroundHit;
        RearRightWheelCollider.GetGroundHit(out CorrespondingGroundHit);

        float normalizedLeanAngle = Mathf.Clamp(CorrespondingGroundHit.sidewaysSlip, -1f, 1f);

        if (normalizedLeanAngle > 0f)
            normalizedLeanAngle = 1;
        else
            normalizedLeanAngle = -1;

        if (transform.InverseTransformDirection(rigid.velocity).z >= 0)
            horizontalLean = Mathf.Clamp(Mathf.Lerp(horizontalLean, (transform.InverseTransformDirection(rigid.angularVelocity).y) * chassisHorizontalLean, Time.deltaTime * 3f), -3.0f, 3.0f);
        else
            horizontalLean = Mathf.Clamp(Mathf.Lerp(horizontalLean, (Mathf.Abs(transform.InverseTransformDirection(rigid.angularVelocity).y) * -normalizedLeanAngle) * chassisHorizontalLean, Time.deltaTime * 3f), -3.0f, 3.0f);

        if (float.IsNaN(verticalLean) || float.IsNaN(horizontalLean) || float.IsInfinity(verticalLean) || float.IsInfinity(horizontalLean))
            return;

        Quaternion target = Quaternion.Euler(verticalLean, chassis.transform.localRotation.y + (rigid.angularVelocity.z), horizontalLean);
        chassis.transform.localRotation = target;

    }

    public void Lights()
    {

        float brakeLightInput;
        brakeLightInput = Mathf.Clamp(-motorInput * 2, 0.0f, 1.0f);

        if (Input.GetKeyDown(KeyCode.L))
        {
            headLightsOn = !headLightsOn;
        }

        for (int i = 0; i < brakeLights.Length; i++)
        {

            if (!reversing)
                brakeLights[i].intensity = brakeLightInput;
            else
                brakeLights[i].intensity = 0f;

        }

        for (int i = 0; i < headLights.Length; i++)
        {

            if (headLightsOn)
                headLights[i].enabled = true;
            else
                headLights[i].enabled = false;

        }

        for (int i = 0; i < reverseLights.Length; i++)
        {

            if (!reversing)
                reverseLights[i].intensity = Mathf.Lerp(reverseLights[i].intensity, 0.0f, Time.deltaTime * 5);
            else
                reverseLights[i].intensity = brakeLightInput;

        }

    }

    void OnGUI()
    {

        GUI.skin.label.fontSize = 12;
        GUI.skin.box.fontSize = 12;
        Matrix4x4 orgRotation = GUI.matrix;

        if (canControl)
        {
            if (demoGUI)
            {

                GUI.backgroundColor = Color.black;
                float guiWidth = Screen.width / 2 - 200;

                GUI.Box(new Rect(Screen.width - 410 - guiWidth, 10, 400, 220), "");
                GUI.Label(new Rect(Screen.width - 400 - guiWidth, 10, 400, 150), "Engine RPM : " + Mathf.CeilToInt(engineRPM));
                GUI.Label(new Rect(Screen.width - 400 - guiWidth, 30, 400, 150), "speed : " + Mathf.CeilToInt(speed));
                GUI.Label(new Rect(Screen.width - 400 - guiWidth, 190, 400, 150), "Horizontal Tilt : " + Input.acceleration.x);
                GUI.Label(new Rect(Screen.width - 400 - guiWidth, 210, 400, 150), "Vertical Tilt : " + Input.acceleration.y);
                if (_wheelTypeChoise == WheelType.FWD)
                {
                    GUI.Label(new Rect(Screen.width - 400 - guiWidth, 50, 400, 150), "Left Wheel RPM : " + Mathf.CeilToInt(FrontLeftWheelCollider.rpm));
                    GUI.Label(new Rect(Screen.width - 400 - guiWidth, 70, 400, 150), "Right Wheel RPM : " + Mathf.CeilToInt(FrontRightWheelCollider.rpm));
                    GUI.Label(new Rect(Screen.width - 400 - guiWidth, 90, 400, 150), "Left Wheel Torque : " + Mathf.CeilToInt(FrontLeftWheelCollider.motorTorque));
                    GUI.Label(new Rect(Screen.width - 400 - guiWidth, 110, 400, 150), "Right Wheel Torque : " + Mathf.CeilToInt(FrontRightWheelCollider.motorTorque));
                    GUI.Label(new Rect(Screen.width - 400 - guiWidth, 130, 400, 150), "Left Wheel brake : " + Mathf.CeilToInt(FrontLeftWheelCollider.brakeTorque));
                    GUI.Label(new Rect(Screen.width - 400 - guiWidth, 150, 400, 150), "Right Wheel brake : " + Mathf.CeilToInt(FrontRightWheelCollider.brakeTorque));
                    GUI.Label(new Rect(Screen.width - 400 - guiWidth, 170, 400, 150), "Steer Angle : " + Mathf.CeilToInt(FrontLeftWheelCollider.steerAngle));
                }
                if (_wheelTypeChoise == WheelType.RWD || _wheelTypeChoise == WheelType.AWD)
                {
                    GUI.Label(new Rect(Screen.width - 400 - guiWidth, 50, 400, 150), "Left Wheel RPM : " + Mathf.CeilToInt(RearLeftWheelCollider.rpm));
                    GUI.Label(new Rect(Screen.width - 400 - guiWidth, 70, 400, 150), "Right Wheel RPM : " + Mathf.CeilToInt(RearRightWheelCollider.rpm));
                    GUI.Label(new Rect(Screen.width - 400 - guiWidth, 90, 400, 150), "Left Wheel Torque : " + Mathf.CeilToInt(RearLeftWheelCollider.motorTorque));
                    GUI.Label(new Rect(Screen.width - 400 - guiWidth, 110, 400, 150), "Right Wheel Torque : " + Mathf.CeilToInt(RearRightWheelCollider.motorTorque));
                    GUI.Label(new Rect(Screen.width - 400 - guiWidth, 130, 400, 150), "Left Wheel brake : " + Mathf.CeilToInt(RearLeftWheelCollider.brakeTorque));
                    GUI.Label(new Rect(Screen.width - 400 - guiWidth, 150, 400, 150), "Right Wheel brake : " + Mathf.CeilToInt(RearRightWheelCollider.brakeTorque));
                    GUI.Label(new Rect(Screen.width - 400 - guiWidth, 170, 400, 150), "Steer Angle : " + Mathf.CeilToInt(FrontLeftWheelCollider.steerAngle));
                }

                GUI.backgroundColor = Color.blue;
                GUI.Button(new Rect(Screen.width - 30 - guiWidth, 165, 10, Mathf.Clamp((-motorInput * 100), -100, 0)), "");

                GUI.backgroundColor = Color.red;
                GUI.Button(new Rect(Screen.width - 45 - guiWidth, 165, 10, Mathf.Clamp((motorInput * 100), -100, 0)), "");

            }

        }

    }

}