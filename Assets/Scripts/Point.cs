using UnityEngine;

public class Point : MonoBehaviour
{
    private PlayerController _player;
    private Transform _moveTr;
    private bool _moving;
    private bool _isMinus;
    private void Start()
    {
        _player=PlayerController.Instance;
    }
    public void SetIfMinus()
    {
        GetComponentInChildren<MeshRenderer>().enabled = false;
        _isMinus = true;
    }
    private void Update()
    {
        if (_moving)
        {
            var dist = Vector3.Distance(transform.position, _moveTr.position);
            transform.position = Vector3.MoveTowards(transform.position, _moveTr.position, 20*Time.deltaTime);
            if (dist < 0.1f)
            {
                _moving = false;
                _player.OnPointsAdded(_isMinus,transform);
                Destroy(this);
            }
        }
    }
    public void SendToPlayer(Transform tr)
    {
        _moveTr = tr;
        _moving = true;
    }
}
