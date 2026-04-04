using UnityEngine;

public class CreatureStageSwitch : MonoBehaviour
{
    public GameObject stage1;
    public GameObject stage2;

    private bool stage2Active = false;
    private float lockedX;
    private float lockedZ;
    private Quaternion lockedRotation;

    void Start()
    {
        stage2.SetActive(false);
        Invoke("SwitchToStage2", 15f);
    }

    void SwitchToStage2()
    {
        // Save only X and Z position
        lockedX = stage1.transform.position.x;
        lockedZ = stage1.transform.position.z;
        lockedRotation = stage1.transform.rotation;

        // Disable root motion
        Animator anim = stage2.GetComponent<Animator>();
        if(anim != null) anim.applyRootMotion = false;

        stage1.SetActive(false);
        stage2.transform.position = new Vector3(lockedX, stage1.transform.position.y, lockedZ);
        stage2.transform.rotation = lockedRotation;
        stage2.SetActive(true);

        stage2Active = true;
    }

    void LateUpdate()
    {
        // Only lock X and Z — let Y animate freely
        if(stage2Active)
        {
            stage2.transform.position = new Vector3(
                lockedX,
                stage2.transform.position.y,  // Y is free for animation
                lockedZ
            );
            stage2.transform.rotation = lockedRotation;
        }
    }
}