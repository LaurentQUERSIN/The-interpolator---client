using UnityEngine;
using System.Collections;

public class RemotePlayer : MonoBehaviour {
    
    private Vector3 _lastPos = Vector3.zero;
    private Vector3 _lastVect = Vector3.zero;
    private Quaternion _lastRot = Quaternion.identity;

    private Vector3 _accel = Vector3.zero;

    private Vector3 _targetPos = Vector3.zero;
    private Vector3 _targetVect = Vector3.zero;
    private Quaternion _targetRot = Quaternion.identity;

    private Vector3 P1 = Vector3.zero;
    private Vector3 P2 = Vector3.zero;
    private float _currentSpan;
    private float _targetSpan = 0.2f;

    public bool testExtrapol = false;
    public bool _bezier = true;

    public void SetNextPos(Vector3 pos, Vector3 vect, Quaternion rot)
    {
        if (testExtrapol == true)
            return;
        Debug.Log("<- " + pos + "  " + vect + "  " + rot);
        _lastPos = transform.position;
        _lastVect = _targetVect;
        _lastRot = transform.rotation;

        _targetPos = pos;
        _targetVect = vect;
        _targetRot = rot;

        P1 = _lastPos + _lastVect * _targetSpan / 3;
        P2 = _targetPos - _targetVect * _targetSpan / 3;
        _currentSpan = 0;

       Debug.DrawLine(_lastPos, P1, Color.gray, 1.0f);
       Debug.DrawLine(P1, P2, Color.gray, 1.0f);
       Debug.DrawLine(P2, _targetPos, Color.gray, 1.0f);
    }

    void Start()
    {
        Debug.DrawLine(_lastPos, _lastPos + _lastVect * _targetSpan / 3, Color.gray, 1.0f);
        Debug.DrawLine(_lastPos + _lastVect * _targetSpan / 3, _targetPos - _targetVect * _targetSpan / 3, Color.gray, 1.0f);
        Debug.DrawLine(_targetPos - _targetVect * _targetSpan / 3, _targetPos, Color.gray, 1.0f);
        P1 = _lastPos + _lastVect * _targetSpan / 3;
        P2 = _targetPos - _targetVect * _targetSpan / 3;
    }

    void Update ()
    {
        _currentSpan += Time.deltaTime;
        if (_currentSpan < _targetSpan)
        {
            float perc = _currentSpan / _targetSpan;
            if (_bezier == false)
                transform.position = Vector3.Lerp(_lastPos, _targetPos, perc);
            else
            {
                var l1 = Vector3.Lerp(_lastPos, P1 , perc);
                var l2 = Vector3.Lerp(P1, P2, perc);
                var l3 = Vector3.Lerp(P2, _targetPos, perc);

                Debug.DrawLine(l1, l2, Color.green, Time.deltaTime);
                Debug.DrawLine(l2, l3, Color.green, Time.deltaTime);

                var f1 = Vector3.Lerp(l1, l2, perc);
                var f2 = Vector3.Lerp(l2, l3, perc);

                Debug.DrawLine(f1, f2, Color.blue, Time.deltaTime);

                transform.position = Vector3.Lerp(f1, f2, perc);
            }
            transform.rotation = Quaternion.Lerp(_lastRot, _targetRot, perc);
        }
        else
        {
            _lastPos = transform.position;
            _lastVect = _targetVect;
            _lastRot = transform.rotation;

            _targetPos = _targetPos + (_targetVect * _targetSpan);

            P1 = _lastPos + _lastVect * _targetSpan / 3;
            P2 = _targetPos - _targetVect * _targetSpan / 3;
            _currentSpan = 0;

            //_currentSpan = 0;
            //this.SetNextPos(_targetPos + _targetVect, _targetVect, _targetRot);

        }
	}
}
