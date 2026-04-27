using System.Collections;
using UnityEngine;

public class TelescopicShield : MonoBehaviour
{
    [Header("Rings  (smallest to largest)")]
    [SerializeField] private Transform[] rings;

    [Header("Starting State")]
    [Tooltip("Если включено щит стартует закрытым (кольца в позициях редактора). Иначе открыт (сложен в большое кольцо).")]
    [SerializeField] private bool startDeployed = false;

    [Header("Animation")]
    [SerializeField] private float phaseDuration = 0.25f;
    [Tooltip("Пауза между фазами. 0 непрерывно.")]
    [SerializeField] private float phaseGap      = 0f;
    [SerializeField] private AnimationCurve phaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Direction")]
    [Tooltip("Включить, если rings[] назначены от большего к меньшему.")]
    [SerializeField] private bool reverseRingOrder = false;

    [Header("Debug")]
    [Tooltip("Закрыто = батарея защищена, Открыто = батарея не защищена.")]
    [SerializeField] private bool debugToggle;
    [SerializeField] private bool debugReset;

    private Vector3[]   _deployedPos;
    private Vector3[]   _visualOffset;
    private Renderer[]  _renderers;
    private Coroutine   _anim;

    public bool IsDeployed { get; private set; }
    public bool IsAnimating => _anim != null;
    public event System.Action OnDeployed;
    public event System.Action OnRetracted;

    private void Awake()
    {
        RecordDeployedPositions();

        if (startDeployed)
        {
            IsDeployed = true;
        }
        else
        {
            CollapseInstant();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (debugToggle)   { Open();        }
        if (!debugToggle)  { Close();       }
        if (debugReset)  { ResetShield(); }
    }
#endif

    public void Open()
    {
        StopAnim();
        _anim = StartCoroutine(AnimateDeploy());
    }

    public void Close()
    {
        StopAnim();
        _anim = StartCoroutine(AnimateCollapse());
    }

    public void Toggle()
    {
        if (IsDeployed) Close(); else Open();
    }

    public void ResetShield()
    {
        StopAnim();
        CollapseInstant();
    }

    private void RecordDeployedPositions()
    {
        int n = rings.Length;
        _deployedPos  = new Vector3[n];
        _visualOffset = new Vector3[n];
        _renderers    = new Renderer[n];
        for (int i = 0; i < n; i++)
        {
            _deployedPos[i] = rings[i].position;
            Renderer r = rings[i].GetComponentInChildren<Renderer>();
            _renderers[i] = r;
            Vector3 vc = r != null ? r.bounds.center : rings[i].position;
            _visualOffset[i] = vc - rings[i].position;
        }
    }

    private void SetRingVisible(int ringIdx, bool visible)
    {
        if (_renderers[ringIdx] != null)
            _renderers[ringIdx].enabled = visible;
    }

    private float DeployedVisualY(int idx) => _deployedPos[idx].y + _visualOffset[idx].y;

    private int[] CascadeOrder()
    {
        int n = rings.Length;
        int[] order = new int[n];
        for (int i = 0; i < n; i++)
            order[i] = reverseRingOrder ? n - 1 - i : i;
        return order;
    }

    private void CollapseInstant()
    {
        if (_deployedPos == null) RecordDeployedPositions();

        int[] order      = CascadeOrder();
        int   outerIdx   = order[order.Length - 1];
        float baseVisualY = DeployedVisualY(outerIdx);

        for (int i = 0; i < rings.Length; i++)
        {
            if (i == outerIdx) continue;
            Vector3 p = rings[i].position;
            rings[i].position = new Vector3(p.x, baseVisualY - _visualOffset[i].y, p.z);
            SetRingVisible(i, false);
        }

        IsDeployed = false;
    }

    private IEnumerator AnimateDeploy()
    {
        int[]   order  = CascadeOrder();
        int     n      = order.Length;
        float[] deltas = BuildDeltas(order, n);

        Vector3[] cur = new Vector3[n];
        for (int i = 0; i < n; i++)
            cur[i] = rings[order[i]].position;

        for (int phase = 0; phase < n - 1; phase++)
        {
            SetRingVisible(order[phase], true);

            float     delta = deltas[phase];
            Vector3[] from  = new Vector3[phase + 1];
            Vector3[] to    = new Vector3[phase + 1];

            for (int i = 0; i <= phase; i++)
            {
                from[i] = cur[i];
                to[i]   = new Vector3(cur[i].x, cur[i].y + delta, cur[i].z);
            }

            yield return LerpPhase(order, from, to, phase);

            for (int i = 0; i <= phase; i++)
            {
                cur[i] = to[i];
                rings[order[i]].position = cur[i];
            }

            if (phaseGap > 0f)
                yield return new WaitForSeconds(phaseGap);
        }

        // Snap to exact deployed positions to eliminate floating-point drift.
        for (int i = 0; i < n; i++)
            rings[order[i]].position = _deployedPos[order[i]];

        IsDeployed = true;
        _anim = null;
        OnDeployed?.Invoke();
    }

    private IEnumerator AnimateCollapse()
    {
        int[]   order  = CascadeOrder();
        int     n      = order.Length;
        float[] deltas = BuildDeltas(order, n);

        Vector3[] cur = new Vector3[n];
        for (int i = 0; i < n; i++)
            cur[i] = rings[order[i]].position;

        for (int phase = n - 2; phase >= 0; phase--)
        {
            float     delta = deltas[phase];
            Vector3[] from  = new Vector3[phase + 1];
            Vector3[] to    = new Vector3[phase + 1];

            for (int i = 0; i <= phase; i++)
            {
                from[i] = cur[i];
                to[i]   = new Vector3(cur[i].x, cur[i].y - delta, cur[i].z);
            }

            yield return LerpPhase(order, from, to, phase);

            for (int i = 0; i <= phase; i++)
            {
                cur[i] = to[i];
                rings[order[i]].position = cur[i];
            }

            SetRingVisible(order[phase], false);

            if (phaseGap > 0f)
                yield return new WaitForSeconds(phaseGap);
        }

        CollapseInstant();
        _anim = null;
        OnRetracted?.Invoke();
    }

    private float[] BuildDeltas(int[] order, int n)
    {
        float[] d = new float[n - 1];
        for (int i = 0; i < n - 1; i++)
            d[i] = DeployedVisualY(order[i]) - DeployedVisualY(order[i + 1]);
        return d;
    }

    private IEnumerator LerpPhase(int[] order, Vector3[] from, Vector3[] to, int phase)
    {
        float elapsed = 0f;
        while (elapsed < phaseDuration)
        {
            elapsed += Time.deltaTime;
            float s = phaseCurve.Evaluate(Mathf.Clamp01(elapsed / phaseDuration));
            for (int i = 0; i <= phase; i++)
                rings[order[i]].position = Vector3.Lerp(from[i], to[i], s);
            yield return null;
        }
    }

    private void StopAnim()
    {
        if (_anim == null) return;
        StopCoroutine(_anim);
        _anim = null;
    }
}
