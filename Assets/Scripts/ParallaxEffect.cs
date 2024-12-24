using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public Camera cam;
    public Transform followTarget;

    private Vector2 _startingPosition;
    private float _startingZ;

    private Vector2 CamMoveSinceStart => (Vector2)cam.transform.position - _startingPosition;

    private float ZDistanceFromTarget => transform.position.z - followTarget.transform.position.z;

    private float ClippingPlane => cam.transform.position.z + (ZDistanceFromTarget > 0 ? cam.farClipPlane : cam.nearClipPlane);
    private float ParallaxFactor => Mathf.Abs(ZDistanceFromTarget) / ClippingPlane;

    public void Start()
    {
        _startingPosition = transform.position;
        _startingZ = transform.position.z;
    }

    public void Update()
    {
        if (followTarget == null)
            return;

        var newPosition = _startingPosition + CamMoveSinceStart * ParallaxFactor;
        transform.position = new Vector3(newPosition.x, _startingPosition.y, _startingZ);
    }
}
