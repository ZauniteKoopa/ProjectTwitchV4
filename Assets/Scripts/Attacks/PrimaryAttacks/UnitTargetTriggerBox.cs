using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class UnitTargetDelegate : UnityEvent<IUnitStatus> {}

public class UnitTargetTriggerBox : MonoBehaviour
{
    public UnitTargetDelegate unitEnterEvent;

    private void OnTriggerEnter(Collider collider) {
        IUnitStatus tgt = collider.GetComponent<IUnitStatus>();

        if (tgt != null) {
            unitEnterEvent.Invoke(tgt);
        }
    }
}
