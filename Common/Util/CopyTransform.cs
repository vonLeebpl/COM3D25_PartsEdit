using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

internal class CopyTransform : MonoBehaviour {
    public bool global = true;

    public bool bPos = true;
    public bool bRot = true;
    public bool bScl = true;

    public Transform targetTrs = null;

    void Update() {
            FollowTarget();
    }

    public void SetTarget(Transform trs) {
        targetTrs = trs;
        FollowTarget();
    }

    public void FollowTarget() {
        if (targetTrs) {
            if (global) {
                FollowTargetGlobal();
            } else {
                FollowTargetLocal();
            }
        }
    }

    void FollowTargetGlobal() {
        if (bPos) {
            transform.position = targetTrs.position;
        }
        if (bRot) {
            transform.rotation = targetTrs.rotation;
        }
        if (bScl) {
            transform.localScale = Vector3.one;
            transform.localScale = transform.InverseTransformPoint(targetTrs.lossyScale);
        }
    }

    void FollowTargetLocal() {
        if (bPos) {
            transform.localPosition = targetTrs.localPosition;
        }
        if (bRot) {
            transform.localRotation = targetTrs.localRotation;
        }
        if (bScl) {
            transform.localScale = targetTrs.localScale;
        }
    }
}
