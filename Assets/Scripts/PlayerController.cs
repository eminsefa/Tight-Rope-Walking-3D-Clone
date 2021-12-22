using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    [SerializeField] private GameEngine ge;
    [SerializeField] private Transform stick;
    [SerializeField] private List<Transform> baseTransforms;
    [SerializeField] private List<TextMeshProUGUI> pointTexts;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxRot;
    [SerializeField] private float fallTime;
    [SerializeField] private Animator anim;
    [SerializeField] private List<Rigidbody> ragdollRigidbodies;
    [SerializeField] private Transform fallerTr;
    private bool _levelPlaying;
    private bool _isLeftHeavier;
    public bool InputIsLeft { get; private set; }
    private float _timer;
    private Quaternion _stickDefaultLRot;
    private List<Transform> _leftTransforms = new List<Transform>();
    private List<Transform> _rightTransforms = new List<Transform>();
    public List<Transform> GetBaseTransforms => baseTransforms;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        ge.LevelStarted += LevelStarted;
        _leftTransforms.Add(baseTransforms[0]);
        _rightTransforms.Add(baseTransforms[1]);
        _stickDefaultLRot = stick.localRotation;
    }
    private void OnDisable()
    {
        ge.LevelStarted -= LevelStarted;
    }
    private void Update()
    {
        if(!_levelPlaying) return;
        Move();
        GetInputSide();
        AlignBoxes();
        Rotate();
    }
    private void Move()
    {
        var pos = transform.position;
        pos.z += moveSpeed * Time.deltaTime;
        transform.position = pos;
    }
    private void Rotate()
    {
        var isLeft = _leftTransforms.Count > _rightTransforms.Count;
        if (isLeft)
        {
            if (!_isLeftHeavier)
            {
                _isLeftHeavier = true;
                anim.SetTrigger($"LeftSlide");
            }
        }
        else if (_isLeftHeavier)
        {
            _isLeftHeavier = false;
            anim.SetTrigger($"RightSlide");
        }
        var i = isLeft ? 1 : -1;
        var dif = Mathf.Abs(_leftTransforms.Count - _rightTransforms.Count);
        var lerpRotX = Mathf.Lerp(0, 35 * i, dif / 30f);
        stick.localRotation=Quaternion.AngleAxis(lerpRotX,Vector3.forward) *_stickDefaultLRot;
        
        var sRot = stick.localRotation.eulerAngles;
        var relX = sRot.x - 90;
        var rot = transform.rotation.eulerAngles;
        rot.z = relX/2f;
        if (sRot.z > 0) rot.z = -relX/2f;
        transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.Euler(rot),5*Time.deltaTime); 
        if (Mathf.Abs(lerpRotX) > maxRot)
        {
            _timer += Time.deltaTime;
            if (_timer > fallTime)
            {
                LevelFailed();
                ge.LevelEnded();
            }
        }
        else _timer = 0;
    }
    private void GetInputSide()
    {
        if (Input.GetMouseButton(0))
        {
            var mousePos = Input.mousePosition;
            if (mousePos.x >= Screen.width/2f) InputIsLeft = false;
            else InputIsLeft = true;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag($"Finish"))
        {
            _levelPlaying = false;
            anim.enabled = false;
            Destroy(stick.gameObject);
            moveSpeed = 0;
            transform.position = other.transform.position + Vector3.up*0.5f;
            ge.LevelEnded();
        }
    }
    private void LevelFailed()
    {
        _levelPlaying = false;
        stick.GetComponent<Rigidbody>().isKinematic = false;
        anim.enabled = false;
        foreach (var r in ragdollRigidbodies)
        {
            r.isKinematic = false;
        }
    }
    private void LevelStarted()
    {
        var pos = transform.position;
        pos.z = 1;
        transform.position = pos;
        _levelPlaying = true;
        anim.SetTrigger($"RightSlide");
    }
    private void AlignBoxes()
    {
        for (int i = 0; i < _leftTransforms.Count; i++)
        {
            if(i==0) continue;
            var tr = _leftTransforms[i];
            var prevTr = _leftTransforms[i - 1];
            tr.SetParent(prevTr);
            tr.position = prevTr.position + prevTr.up * 0.075f;
            tr.rotation = Quaternion.Slerp(prevTr.rotation, Quaternion.identity, (float)i / 30);
        }
        for (int i = 0; i < _rightTransforms.Count; i++)
        {
            if(i==0) continue;
            var tr = _rightTransforms[i];
            var prevTr = _rightTransforms[i - 1];
            tr.SetParent(prevTr);
            tr.position = prevTr.position + prevTr.up * 0.075f;
            tr.rotation = Quaternion.Slerp(prevTr.rotation, Quaternion.identity, (float)i / 30);
        }
    }
    public void OnPointsAdded(bool isMinus,Transform t)
    {
        var leftSide = t.position.x < 0;
        if (isMinus)
        {
            if (_leftTransforms.Count == 1)
            {
                leftSide = false;
                if(_rightTransforms.Count==1) return;
            }
            if (leftSide)
            {
                var tr = _leftTransforms[_leftTransforms.Count - 1];
                _leftTransforms.Remove(tr);
                tr.SetParent(fallerTr);
                Destroy(tr.gameObject,5f);
            }
            else
            {
                var tr = _rightTransforms[_rightTransforms.Count - 1];
                _rightTransforms.Remove(tr);
                tr.SetParent(fallerTr);
                Destroy(tr.gameObject,5f);
            }
        }
        else
        {
            if (leftSide) _leftTransforms.Add(t);
            else _rightTransforms.Add(t);
        }
        pointTexts[0].text = (_leftTransforms.Count-1).ToString();
        pointTexts[1].text = (_rightTransforms.Count-1).ToString();
    }
}
