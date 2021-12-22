using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PointsParent : MonoBehaviour
{
    private PlayerController _player;
    private bool _isMinus;
    [SerializeField] private int point;
    [SerializeField] private TextMeshProUGUI pointText;
    [SerializeField] private List<Point> points;
    private void Awake()
    {
        if(point<0)
        {
            _isMinus = true;
            foreach (var p in points) p.SetIfMinus();
        }
        SetPointsText(point);
    }
    private void SetPointsText(int p)
    {
        if (!_isMinus) pointText.text = "+" + p;
        else pointText.text = p.ToString();
    }
    private void Start()
    {
        _player=PlayerController.Instance;
    }
    private void OnTriggerEnter(Collider other)
    {
        GetComponent<Collider>().enabled = false;
        var tr = _player.InputIsLeft ? _player.GetBaseTransforms[0] : _player.GetBaseTransforms[1];
        StartCoroutine(SendToPlayer(tr));
    }
    private IEnumerator SendToPlayer(Transform tr)
    {
        var absP = Mathf.Abs(point);
        var count = absP;
        for (int i = 0; i < count; i++)
        {
            yield return new WaitForSeconds(0.01f);
            points[points.Count-i-1].SendToPlayer(tr);
            yield return new WaitForSeconds(0.01f);
            absP -= 1;
            if (absP == 0)
            {
                Destroy(gameObject,1f);
            }
            SetPointsText(absP);
        }
    }
}
