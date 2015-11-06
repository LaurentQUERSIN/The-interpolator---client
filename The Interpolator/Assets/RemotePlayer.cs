using UnityEngine;
using System.Collections;

public class RemotePlayer : MonoBehaviour {

    private Vector3 _lastPos = Vector3.zero;
    private Vector3 _lastVect = Vector3.zero;
    private Quaternion _lastRot = Quaternion.identity;

    private Vector3 _targetPos = Vector3.zero;
    private Vector3 _targetVect = Vector3.zero;
    private Quaternion _targetRot = Quaternion.identity;

    private float _currentSpan;
    private float _targetSpan = 0.1f;

    public void SetNextPos(Vector3 pos, Vector3 vect, Quaternion rot)
    {
        Debug.Log(pos + "  " + vect + "  " + rot);
        _lastPos = transform.position;
        _lastVect = _targetVect;
        _lastRot = transform.rotation;

        _targetPos = pos;
        _targetVect = vect;
        _targetRot = rot;

        _currentSpan = 0;
    }

    void Update ()
    {
        _currentSpan += Time.deltaTime;
        if (_currentSpan < _targetSpan)
        {
            float perc = _currentSpan / _targetSpan;
            transform.position = Vector3.Lerp(_lastPos, _targetPos, perc);
            transform.rotation = Quaternion.Lerp(_lastRot, _targetRot, perc);
        }
        else
        {
            _currentSpan = 0;
            this.SetNextPos(_targetPos + _targetVect, _targetVect, _targetRot);
        }
	}
}
