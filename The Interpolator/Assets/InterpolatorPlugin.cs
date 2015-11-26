using UnityEngine;
using System.IO;
using System.Collections;
using System;

public class InterpolatorPlugin : Stormancer.SynchBehaviourBase
{
    private Vector3 _lastPos = Vector3.zero;
    private Vector3 _lastVect = Vector3.zero;
    private Quaternion _lastRot = Quaternion.identity;

    private Vector3 _targetPos = Vector3.zero;
    private Vector3 _targetVect = Vector3.zero;
    private Quaternion _targetRot = Quaternion.identity;

    private Vector3 P1 = Vector3.zero;
    private Vector3 P2 = Vector3.zero;
    private float _currentSpan;
    private float _targetSpan = 0.2f;
    
    private bool _bezier = true;

    public void UseBezierSpline(bool b)
    {
        _bezier = b;
    }

    private void SetTimeSpan(float t)
    {
        _targetSpan = t;
    }

    public override void SendChanges(Stream stream)
    {
        return;
    }

    public override void ApplyChanges(Stream stream)
    {
        using (var reader = new BinaryReader(stream))
        {
            var stamp = reader.ReadInt64();
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();

            var vx = reader.ReadSingle();
            var vy = reader.ReadSingle();
            var vz = reader.ReadSingle();

            var rx = reader.ReadSingle();
            var ry = reader.ReadSingle();
            var rz = reader.ReadSingle();
            var rw = reader.ReadSingle();
            
            if (LastChanged < stamp)
            {
                LastChanged = stamp;
                    SetNextPos(new Vector3(x, y, z), new Vector3(vx, vy, vz), new Quaternion(rx, ry, rz, rw));
            }
        }
    }

    public void SetNextPos(Vector3 pos, Vector3 vect, Quaternion rot)
    {
        
        _lastPos = transform.position;
        _lastVect = _targetVect;
        _lastRot = transform.rotation;

        _targetPos = pos;
        _targetVect = vect;
        _targetRot = rot;

        P1 = _lastPos + _lastVect * _targetSpan / 3;
        P2 = _targetPos - _targetVect * _targetSpan / 3;
        _currentSpan = 0;

       //Debug.DrawLine(_lastPos, P1, Color.gray, 1.0f);
       //Debug.DrawLine(P1, P2, Color.gray, 1.0f);
       //Debug.DrawLine(P2, _targetPos, Color.gray, 1.0f);
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
        }
	}
}
