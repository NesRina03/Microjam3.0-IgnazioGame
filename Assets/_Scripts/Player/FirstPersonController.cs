using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
    using UnityEditor;
    using System.Net;
#endif

public class FirstPersonController : MonoBehaviour
{
    private Rigidbody rb;

    #region Camera Movement Variables
    public Camera playerCamera;
    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    #endregion

    #region Zoom Variables
    public bool enableZoom = true;
    public bool holdToZoom = false;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;
    private bool isZoomed = false;
    #endregion

    #region Movement Variables
    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;
    private bool isWalking = false;
    #endregion

    #region Sprint Variables
    public bool enableSprint = true;
    public bool unlimitedSprint = true;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintSpeed = 7f;
    public float sprintDuration = 5f;
    public float sprintCooldown = .5f;
    public float sprintFOV = 80f;
    public float sprintFOVStepTime = 10f;
    private bool isSprinting = false;
    private float sprintRemaining;
    private bool isSprintCooldown = false;
    private float sprintCooldownReset;
    #endregion

    #region Jump Variables
    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;
    private bool isGrounded = false;
    #endregion

    #region Crouch Variables
    public bool enableCrouch = true;
    public bool holdToCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = .75f;
    public float speedReduction = .5f;
    private bool isCrouched = false;
    private Vector3 originalScale;
    #endregion

    #region Head Bob Variables
    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(.15f, .05f, 0f);
    private Vector3 jointOriginalPos;
    private float timer = 0;
    #endregion

    #region Footstep Variables
    public bool enableFootsteps = true;
    public AudioClip[] footstepSounds;
    public float footstepInterval = 0.5f;
    public float sprintFootstepInterval = 0.3f;
    private AudioSource audioSource;
    private float footstepTimer = 0f;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        playerCamera.fieldOfView = fov;
        originalScale = transform.localScale;

        if(joint != null)
            jointOriginalPos = joint.localPosition;

        if (!unlimitedSprint)
        {
            sprintRemaining = sprintDuration;
            sprintCooldownReset = sprintCooldown;
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        #region Camera
        if(cameraCanMove)
        {
            yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;
            if (!invertCamera)
                pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");
            else
                pitch += mouseSensitivity * Input.GetAxis("Mouse Y");

            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
            transform.localEulerAngles = new Vector3(0, yaw, 0);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }
        #endregion

        #region Zoom
        if (enableZoom)
        {
            if(Input.GetKeyDown(zoomKey) && !holdToZoom && !isSprinting)
                isZoomed = !isZoomed;

            if(holdToZoom && !isSprinting)
            {
                if(Input.GetKeyDown(zoomKey)) isZoomed = true;
                else if(Input.GetKeyUp(zoomKey)) isZoomed = false;
            }

            if(isZoomed)
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, zoomFOV, zoomStepTime * Time.deltaTime);
            else if(!isSprinting)
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, zoomStepTime * Time.deltaTime);
        }
        #endregion

        #region Sprint
        if(enableSprint)
        {
            if(isSprinting)
            {
                isZoomed = false;
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, sprintFOVStepTime * Time.deltaTime);
                if(!unlimitedSprint)
                {
                    sprintRemaining -= 1 * Time.deltaTime;
                    if (sprintRemaining <= 0)
                    {
                        isSprinting = false;
                        isSprintCooldown = true;
                    }
                }
            }
            else
            {
                sprintRemaining = Mathf.Clamp(sprintRemaining += 1 * Time.deltaTime, 0, sprintDuration);
            }

            if(isSprintCooldown)
            {
                sprintCooldown -= 1 * Time.deltaTime;
                if (sprintCooldown <= 0) isSprintCooldown = false;
            }
            else
            {
                sprintCooldown = sprintCooldownReset;
            }
        }
        #endregion

        #region Jump
        if(enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
            Jump();
        #endregion

        #region Crouch
        if (enableCrouch)
        {
            if(Input.GetKeyDown(crouchKey) && !holdToCrouch)
                Crouch();
            
            if(Input.GetKeyDown(crouchKey) && holdToCrouch)
            {
                isCrouched = false;
                Crouch();
            }
            else if(Input.GetKeyUp(crouchKey) && holdToCrouch)
            {
                isCrouched = true;
                Crouch();
            }
        }
        #endregion

        CheckGround();

        if(enableHeadBob && joint != null)
            HeadBob();

        #region Footsteps
        if(enableFootsteps && isWalking && isGrounded && footstepSounds.Length > 0 && audioSource != null)
        {
            float interval = isSprinting ? sprintFootstepInterval : footstepInterval;
            footstepTimer -= Time.deltaTime;
            if(footstepTimer <= 0)
            {
                int randomIndex = Random.Range(0, footstepSounds.Length);
                // SFX loudness is controlled by the mixer group; keep one-shot at unity.
                audioSource.PlayOneShot(footstepSounds[randomIndex], 1f);
                footstepTimer = interval;
            }
        }
        #endregion
    }

    void FixedUpdate()
    {
        if (playerCanMove)
        {
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            isWalking = (targetVelocity.x != 0 || targetVelocity.z != 0);

            if (enableSprint && Input.GetKey(sprintKey) && sprintRemaining > 0f && !isSprintCooldown)
            {
                targetVelocity = transform.TransformDirection(targetVelocity) * sprintSpeed;
                Vector3 velocity = rb.linearVelocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;
                if (velocityChange.x != 0 || velocityChange.z != 0)
                    isSprinting = true;
                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
            else
            {
                isSprinting = false;
                targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;
                Vector3 velocity = rb.linearVelocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;
                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
        }
    }

    private void CheckGround()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * .5f), transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = .75f;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
            isGrounded = true;
        else
            isGrounded = false;
    }

    private void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
            isGrounded = false;
        }
        if(isCrouched && !holdToCrouch)
            Crouch();
    }

    private void Crouch()
    {
        if(isCrouched)
        {
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
            walkSpeed /= speedReduction;
            isCrouched = false;
        }
        else
        {
            transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
            walkSpeed *= speedReduction;
            isCrouched = true;
        }
    }

    private void HeadBob()
    {
        if(isWalking)
        {
            if(isSprinting)
                timer += Time.deltaTime * (bobSpeed + sprintSpeed);
            else if (isCrouched)
                timer += Time.deltaTime * (bobSpeed * speedReduction);
            else
                timer += Time.deltaTime * bobSpeed;

            joint.localPosition = new Vector3(
                jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x,
                jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y,
                jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z);
        }
        else
        {
            timer = 0;
            joint.localPosition = new Vector3(
                Mathf.Lerp(joint.localPosition.x, jointOriginalPos.x, Time.deltaTime * bobSpeed),
                Mathf.Lerp(joint.localPosition.y, jointOriginalPos.y, Time.deltaTime * bobSpeed),
                Mathf.Lerp(joint.localPosition.z, jointOriginalPos.z, Time.deltaTime * bobSpeed));
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FirstPersonController)), InitializeOnLoadAttribute]
public class FirstPersonControllerEditor : Editor
{
    FirstPersonController fpc;
    SerializedObject SerFPC;

    private void OnEnable()
    {
        fpc = (FirstPersonController)target;
        SerFPC = new SerializedObject(fpc);
    }

    public override void OnInspectorGUI()
    {
        SerFPC.Update();

        EditorGUILayout.Space();
        GUILayout.Label("Modular First Person Controller", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 16 });
        GUILayout.Label("By Jess Case", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
        GUILayout.Label("version 1.0.2", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Camera Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
        fpc.playerCamera = (Camera)EditorGUILayout.ObjectField(new GUIContent("Camera"), fpc.playerCamera, typeof(Camera), true);
        fpc.fov = EditorGUILayout.Slider(new GUIContent("Field of View"), fpc.fov, fpc.zoomFOV, 179f);
        fpc.cameraCanMove = EditorGUILayout.ToggleLeft(new GUIContent("Enable Camera Rotation"), fpc.cameraCanMove);
        GUI.enabled = fpc.cameraCanMove;
        fpc.invertCamera = EditorGUILayout.ToggleLeft(new GUIContent("Invert Camera"), fpc.invertCamera);
        fpc.mouseSensitivity = EditorGUILayout.Slider(new GUIContent("Sensitivity"), fpc.mouseSensitivity, .1f, 10f);
        fpc.maxLookAngle = EditorGUILayout.Slider(new GUIContent("Max Look Angle"), fpc.maxLookAngle, 40, 90);
        GUI.enabled = true;
        EditorGUILayout.Space();

        GUILayout.Label("Zoom", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 13 });
        fpc.enableZoom = EditorGUILayout.ToggleLeft(new GUIContent("Enable Zoom"), fpc.enableZoom);
        GUI.enabled = fpc.enableZoom;
        fpc.holdToZoom = EditorGUILayout.ToggleLeft(new GUIContent("Hold to Zoom"), fpc.holdToZoom);
        fpc.zoomKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Zoom Key"), fpc.zoomKey);
        fpc.zoomFOV = EditorGUILayout.Slider(new GUIContent("Zoom FOV"), fpc.zoomFOV, .1f, fpc.fov);
        fpc.zoomStepTime = EditorGUILayout.Slider(new GUIContent("Step Time"), fpc.zoomStepTime, .1f, 10f);
        GUI.enabled = true;

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Movement Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
        fpc.playerCanMove = EditorGUILayout.ToggleLeft(new GUIContent("Enable Movement"), fpc.playerCanMove);
        GUI.enabled = fpc.playerCanMove;
        fpc.walkSpeed = EditorGUILayout.Slider(new GUIContent("Walk Speed"), fpc.walkSpeed, .1f, fpc.sprintSpeed);
        GUI.enabled = true;
        EditorGUILayout.Space();

        GUILayout.Label("Sprint", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 13 });
        fpc.enableSprint = EditorGUILayout.ToggleLeft(new GUIContent("Enable Sprint"), fpc.enableSprint);
        GUI.enabled = fpc.enableSprint;
        fpc.unlimitedSprint = EditorGUILayout.ToggleLeft(new GUIContent("Unlimited Sprint"), fpc.unlimitedSprint);
        fpc.sprintKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Sprint Key"), fpc.sprintKey);
        fpc.sprintSpeed = EditorGUILayout.Slider(new GUIContent("Sprint Speed"), fpc.sprintSpeed, fpc.walkSpeed, 20f);
        fpc.sprintDuration = EditorGUILayout.Slider(new GUIContent("Sprint Duration"), fpc.sprintDuration, 1f, 20f);
        fpc.sprintCooldown = EditorGUILayout.Slider(new GUIContent("Sprint Cooldown"), fpc.sprintCooldown, .1f, fpc.sprintDuration);
        fpc.sprintFOV = EditorGUILayout.Slider(new GUIContent("Sprint FOV"), fpc.sprintFOV, fpc.fov, 179f);
        fpc.sprintFOVStepTime = EditorGUILayout.Slider(new GUIContent("Sprint FOV Step Time"), fpc.sprintFOVStepTime, .1f, 20f);
        GUI.enabled = true;
        EditorGUILayout.Space();

        GUILayout.Label("Jump", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 13 });
        fpc.enableJump = EditorGUILayout.ToggleLeft(new GUIContent("Enable Jump"), fpc.enableJump);
        GUI.enabled = fpc.enableJump;
        fpc.jumpKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Jump Key"), fpc.jumpKey);
        fpc.jumpPower = EditorGUILayout.Slider(new GUIContent("Jump Power"), fpc.jumpPower, .1f, 20f);
        GUI.enabled = true;
        EditorGUILayout.Space();

        GUILayout.Label("Crouch", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 13 });
        fpc.enableCrouch = EditorGUILayout.ToggleLeft(new GUIContent("Enable Crouch"), fpc.enableCrouch);
        GUI.enabled = fpc.enableCrouch;
        fpc.holdToCrouch = EditorGUILayout.ToggleLeft(new GUIContent("Hold To Crouch"), fpc.holdToCrouch);
        fpc.crouchKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Crouch Key"), fpc.crouchKey);
        fpc.crouchHeight = EditorGUILayout.Slider(new GUIContent("Crouch Height"), fpc.crouchHeight, .1f, 1);
        fpc.speedReduction = EditorGUILayout.Slider(new GUIContent("Speed Reduction"), fpc.speedReduction, .1f, 1);
        GUI.enabled = true;

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Head Bob Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
        fpc.enableHeadBob = EditorGUILayout.ToggleLeft(new GUIContent("Enable Head Bob"), fpc.enableHeadBob);
        GUI.enabled = fpc.enableHeadBob;
        fpc.joint = (Transform)EditorGUILayout.ObjectField(new GUIContent("Camera Joint"), fpc.joint, typeof(Transform), true);
        fpc.bobSpeed = EditorGUILayout.Slider(new GUIContent("Speed"), fpc.bobSpeed, 1, 20);
        fpc.bobAmount = EditorGUILayout.Vector3Field(new GUIContent("Bob Amount"), fpc.bobAmount);
        GUI.enabled = true;

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Footstep Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
        fpc.enableFootsteps = EditorGUILayout.ToggleLeft(new GUIContent("Enable Footsteps"), fpc.enableFootsteps);
        GUI.enabled = fpc.enableFootsteps;
        SerializedProperty footstepProp = SerFPC.FindProperty("footstepSounds");
        EditorGUILayout.PropertyField(footstepProp, new GUIContent("Footstep Sounds"), true);
        fpc.footstepInterval = EditorGUILayout.Slider(new GUIContent("Walk Interval"), fpc.footstepInterval, 0.1f, 2f);
        fpc.sprintFootstepInterval = EditorGUILayout.Slider(new GUIContent("Sprint Interval"), fpc.sprintFootstepInterval, 0.1f, 1f);
        GUI.enabled = true;

        if(GUI.changed)
        {
            EditorUtility.SetDirty(fpc);
            Undo.RecordObject(fpc, "FPC Change");
            SerFPC.ApplyModifiedProperties();
        }
    }
}
#endif