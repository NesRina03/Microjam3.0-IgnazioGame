using UnityEngine;

public class CreatureStageSwitch : MonoBehaviour
{
    public GameObject stage1;   // glisse FINAL STAGE 01 DOUDA ici
    public GameObject stage2;   // glisse FINAL STAGE 02 DOUDA ici

    private Vector3 creaturePosition = new Vector3(440.687f, -5.3848f, -14.5104f);

    void Start()
    {
        stage2.SetActive(false);  // cache stage 2 au départ
        Invoke("SwitchToStage2", 10f);  // switch après 10 secondes
    }

    void SwitchToStage2()
    {
        stage1.SetActive(false);  // cache stage 1
        stage2.SetActive(true);   // montre stage 2
        stage2.transform.position = creaturePosition;  // même position
    }
}