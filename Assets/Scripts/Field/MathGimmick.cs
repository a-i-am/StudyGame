using System.Collections;
using UnityEngine;

public enum MathSkill { Limit, Logarithm, Derivative, Integral, Substitute }

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class MathGimmick : MonoBehaviour
{
    [Header("기믹 데이터 프로파일")]
    public MathGimmickProfile profile;

    [Header("물리 기믹 설정 (장난감 효과용)")]
    public float minScaleLimit = 0.2f;
    public Transform targetPosition;
    public GameObject fractionPrefab;

    private Rigidbody _rb;
    private Collider _collider;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    public bool IsCorrectSkill(MathSkill skill)
    {
        if (profile != null && !string.IsNullOrEmpty(profile.operationType))
        {
            return profile.operationType.Contains(skill.ToString());
        }
        return skill == MathSkill.Logarithm || skill == MathSkill.Derivative;
    }

    public void ApplySkill(MathSkill usedSkill)
    {
        if (IsCorrectSkill(usedSkill))
        {
            switch (usedSkill)
            {
                case MathSkill.Logarithm:
                    ApplyLogarithmicCompression();
                    break;
                case MathSkill.Derivative:
                    ShatterToPieces();
                    break;
                case MathSkill.Limit:
                    StartCoroutine(ApplyLimitDampingRoutine());
                    break;
                case MathSkill.Integral:
                    MaterializePath();
                    break;
                case MathSkill.Substitute:
                    ApplySubstituteCoordinateShift();
                    break;
                default:
                    ExecuteSuccessEffect();
                    break;
            }
        }
        else
        {
            StartCoroutine(TemporaryFailEffectAndRevert(usedSkill));
        }
    }

    private void ExecuteSuccessEffect()
    {
        if (profile != null && profile.effectType != null)
        {
            if (profile.effectType.Contains("ScaleTransform"))
            {
                ApplyLogarithmicCompression();
            }
            else if (profile.effectType.Contains("DampVelocity") || profile.effectType.Contains("Converge"))
            {
                StartCoroutine(ApplyLimitDampingRoutine());
            }
            else if (profile.effectType.Contains("Shatter"))
            {
                ShatterToPieces();
            }
            else if (profile.effectType.Contains("Materialize"))
            {
                MaterializePath();
            }
        }
    }

    public void ApplyLogarithmicCompression()
    {
        Vector3 currentScale = transform.localScale;
        float currentMagnitude = currentScale.x;
        float compressedMagnitude = Mathf.Max(minScaleLimit, Mathf.Log(currentMagnitude + 1.0f, 2.0f));
        transform.localScale = new Vector3(compressedMagnitude, compressedMagnitude, compressedMagnitude);

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null && mr.material != null)
        {
            mr.material.color = new Color(0.2f, 0.7f, 1.0f);
        }

        StartCoroutine(CameraShakeRoutine(0.35f, 0.35f));
    }

    public void ShatterToPieces()
    {
        for (int i = 0; i < 8; i++)
        {
            if (fractionPrefab != null)
            {
                Instantiate(fractionPrefab, transform.position + Random.insideUnitSphere * 0.5f, Quaternion.identity);
            }
            else
            {
                GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
                piece.name = "CubeFraction";
                piece.transform.position = transform.position + Random.insideUnitSphere * 0.5f;
                piece.transform.localScale = Vector3.one * 0.4f;
                Rigidbody rb = piece.AddComponent<Rigidbody>();
                rb.mass = 0.5f;
                rb.AddForce(Random.onUnitSphere * 6f, ForceMode.Impulse);
            }
        }
        StartCoroutine(CameraShakeRoutine(0.4f, 0.45f));
        Destroy(gameObject);
    }

    public void ApplySubstituteCoordinateShift()
    {
        Vector3 origScale = transform.localScale;
        transform.rotation = Quaternion.Euler(30f, 180f, 45f);
        transform.localScale = new Vector3(origScale.x * 2.0f, 0.4f, origScale.z * 2.0f);

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null && mr.material != null)
        {
            mr.material.color = new Color(0.75f, 0.25f, 1.0f);
        }

        StartCoroutine(CameraShakeRoutine(0.35f, 0.35f));
    }

    private IEnumerator CameraShakeRoutine(float duration, float magnitude)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;

        Transform camTrans = mainCam.transform;
        Vector3 originalPos = camTrans.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            camTrans.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        camTrans.localPosition = originalPos;
    }

    private IEnumerator ApplyLimitDampingRoutine()
    {
        Vector3 origScale = transform.localScale;
        Vector3 startPos = transform.position;
        Vector3 dest = targetPosition != null ? targetPosition.position : startPos + Vector3.down * 1.2f;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            transform.position = Vector3.Lerp(startPos, dest, t);
            transform.localScale = Vector3.Lerp(origScale, new Vector3(origScale.x * 1.6f, 0.2f, origScale.z * 1.6f), t);

            if (_rb != null)
                _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, Vector3.zero, t);

            yield return null;
        }

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null && mr.material != null)
        {
            mr.material.color = new Color(0.1f, 0.9f, 0.9f);
        }

        if (_rb != null) _rb.isKinematic = true;
        StartCoroutine(CameraShakeRoutine(0.35f, 0.35f));
    }

    public void MaterializePath()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
            if (meshRenderer.material != null)
            {
                meshRenderer.material.color = new Color(1.0f, 0.85f, 0.2f);
            }
        }
        if (_collider != null) _collider.isTrigger = false;
        transform.localScale = transform.localScale * 2.2f;
        StartCoroutine(CameraShakeRoutine(0.35f, 0.35f));
    }

    private IEnumerator TemporaryFailEffectAndRevert(MathSkill failedSkill)
    {
        Vector3 origScale = transform.localScale;
        Vector3 origPos = transform.position;
        Quaternion origRot = transform.rotation;
        Color origColor = Color.white;

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null && mr.material != null)
        {
            origColor = mr.material.color;
        }

        switch (failedSkill)
        {
            case MathSkill.Logarithm:
                float compressed = Mathf.Max(minScaleLimit, Mathf.Log(origScale.x + 1.0f, 2.0f));
                transform.localScale = new Vector3(compressed, compressed, compressed);
                if (mr != null && mr.material != null) mr.material.color = new Color(0.2f, 0.7f, 1.0f);
                break;
            case MathSkill.Derivative:
                transform.localScale = origScale * 0.5f;
                if (mr != null && mr.material != null) mr.material.color = new Color(1.0f, 0.3f, 0.3f);
                break;
            case MathSkill.Limit:
                transform.position = origPos + Vector3.down * 1.2f;
                transform.localScale = new Vector3(origScale.x * 1.6f, 0.2f, origScale.z * 1.6f);
                if (mr != null && mr.material != null) mr.material.color = new Color(0.1f, 0.9f, 0.9f);
                break;
            case MathSkill.Integral:
                transform.localScale = origScale * 2.2f;
                if (mr != null && mr.material != null) mr.material.color = new Color(1.0f, 0.85f, 0.2f);
                break;
            case MathSkill.Substitute:
                transform.rotation = Quaternion.Euler(30f, 180f, 45f);
                transform.localScale = new Vector3(origScale.x * 2.0f, 0.4f, origScale.z * 2.0f);
                if (mr != null && mr.material != null) mr.material.color = new Color(0.75f, 0.25f, 1.0f);
                break;
            default:
                transform.localScale = origScale * 1.4f;
                break;
        }

        StartCoroutine(CameraShakeRoutine(0.35f, 0.35f));

        yield return new WaitForSeconds(2.0f);

        transform.localScale = origScale;
        transform.position = origPos;
        transform.rotation = origRot;

        if (mr != null && mr.material != null)
        {
            mr.material.color = origColor;
        }
    }
}