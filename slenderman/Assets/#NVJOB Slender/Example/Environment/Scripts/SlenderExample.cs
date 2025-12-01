// #NVJOB Nicholas Veselov - https://nvjob.github.io


using UnityEngine;

public class SlenderExample : MonoBehaviour
{
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public SkinnedMeshRenderer body;
    public Transform target;
    public Transform head;
    public float blendTime = 0.4f;
    public float towards = 5.0f;
    public float weightMul = 1;
    public float clampWeight = 0.5f;
    public Vector3 weight = new Vector3(0.4f, 0.8f, 0.9f);
    public bool yTargetHeadSynk;

    //--------------

    Transform tr;
    Animator anim;
    AudioSource music;
    Vector3 lookAtTargetPosition, lookAtPosition;
    float lookAtWeight;
    float timeFaceCh, facepWeight = 100, timeFace = 10;
    bool faceCh = true;


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void Start()
    {
        //--------------

        tr = transform;
        anim = GetComponent<Animator>();
        music = GetComponent<AudioSource>();
        lookAtTargetPosition = target.position + tr.forward;
        lookAtPosition = head.position + tr.forward;

        //--------------
    }


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    

    void Update()
    {
        //--------------

        lookAtTargetPosition = target.position + tr.forward;

        //--------------

        if (faceCh == true && timeFace < Time.time)
        {
            timeFaceCh += Time.deltaTime * 80;
            if (timeFaceCh >= facepWeight * 2)
            {
                timeFaceCh = 0;
                faceCh = true;
                timeFace = Time.time + Random.Range(3.0f, 6.0f);
                music.pitch = Random.Range(0.8f, 1.0f);
            }
            float var0 = Mathf.PingPong(timeFaceCh, facepWeight);
            body.SetBlendShapeWeight(0, var0);
            music.volume = var0 * 0.1f;
        }

        //--------------
    }


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void OnAnimatorIK()
    {
        //--------------

        if (yTargetHeadSynk == false) lookAtTargetPosition.y = head.position.y;
        Vector3 curDir = lookAtPosition - head.position;
        curDir = Vector3.RotateTowards(curDir, lookAtTargetPosition - head.position, towards * Time.deltaTime, float.PositiveInfinity);
        lookAtPosition = head.position + curDir;
        lookAtWeight = Mathf.MoveTowards(lookAtWeight, 1, Time.deltaTime / blendTime);
        anim.SetLookAtWeight(lookAtWeight * weightMul, weight.x, weight.y, weight.z, clampWeight);
        anim.SetLookAtPosition(lookAtPosition);

        //--------------
    }


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}