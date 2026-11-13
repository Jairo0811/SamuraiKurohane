using UnityEngine;

namespace JMGameStudios.SamuraiKurohane.Prototype
{
    /// <summary>
    /// Side-scroller camera: smooth follow with look-ahead, vertical dead zone,
    /// level bounds, trauma-style shake and zoom punch on heavy impacts.
    /// </summary>
    [DefaultExecutionOrder(50)]
    public sealed class PrototypeCameraRig : MonoBehaviour
    {
        public static PrototypeCameraRig Current { get; private set; }

        private Camera cam;
        private AkioPrototypeController target;
        private Rigidbody targetBody;

        private float minX = -10f;
        private float maxX = 10f;
        private float baseY = 2.2f;
        private float baseSize = 8.7f;
        private float groundY = -1.6f;

        private float velocityX;
        private float velocityY;
        private float lookAhead;
        private float lookAheadVelocity;

        private float shakeAmplitude;
        private float shakeDuration;
        private float shakeAge;
        private float zoomPunch;
        private float zoomPunchVelocity;
        private float seed;

        private void Awake()
        {
            Current = this;
            cam = GetComponent<Camera>();
            seed = Random.value * 100f;
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        public void Configure(AkioPrototypeController follow, float boundsMinX, float boundsMaxX, float orthoSize, float height, float ground)
        {
            target = follow;
            targetBody = follow != null ? follow.GetComponent<Rigidbody>() : null;
            minX = boundsMinX;
            maxX = boundsMaxX;
            baseSize = orthoSize;
            baseY = height;
            groundY = ground;

            if (follow != null)
            {
                Vector3 p = transform.position;
                p.x = Mathf.Clamp(follow.transform.position.x, minX, maxX);
                transform.position = p;
            }
        }

        public void AddShake(float amplitude, float duration)
        {
            float remaining = shakeAmplitude * (1f - shakeAge / Mathf.Max(0.01f, shakeDuration));
            if (amplitude >= remaining)
            {
                shakeAmplitude = amplitude;
                shakeDuration = Mathf.Max(0.02f, duration);
                shakeAge = 0f;
            }
        }

        public void AddZoomPunch(float amount)
        {
            zoomPunch = Mathf.Max(zoomPunch, amount);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            float dt = Time.unscaledDeltaTime;
            Vector3 position = transform.position;
            Vector3 targetPosition = target.transform.position;

            float vx = targetBody != null ? targetBody.linearVelocity.x : 0f;
            float desiredLook = Mathf.Clamp(vx * 0.22f, -2.6f, 2.6f) + target.Facing * 0.8f;
            lookAhead = Mathf.SmoothDamp(lookAhead, desiredLook, ref lookAheadVelocity, 0.35f, Mathf.Infinity, dt);

            float desiredX = Mathf.Clamp(targetPosition.x + lookAhead, minX, maxX);
            float desiredY = baseY + Mathf.Clamp((targetPosition.y - groundY) * 0.55f, -2.5f, 5f);

            position.x = Mathf.SmoothDamp(position.x, desiredX, ref velocityX, 0.16f, Mathf.Infinity, dt);
            position.y = Mathf.SmoothDamp(position.y, desiredY, ref velocityY, 0.32f, Mathf.Infinity, dt);

            // Shake (Perlin noise, quadratic falloff). The follow position is kept
            // un-shaken so the shake never accumulates.
            Vector3 shakeOffset = Vector3.zero;
            if (shakeAge < shakeDuration)
            {
                shakeAge += dt;
                float falloff = 1f - Mathf.Clamp01(shakeAge / shakeDuration);
                float power = shakeAmplitude * falloff * falloff;
                float t = Time.unscaledTime * 55f;
                shakeOffset = new Vector3(
                    (Mathf.PerlinNoise(seed, t) - 0.5f) * 2f * power,
                    (Mathf.PerlinNoise(seed + 17f, t) - 0.5f) * 2f * power,
                    0f);
            }

            transform.position = position + shakeOffset;

            // Zoom punch springs back to zero.
            zoomPunch = Mathf.SmoothDamp(zoomPunch, 0f, ref zoomPunchVelocity, 0.18f, Mathf.Infinity, dt);
            if (cam != null && cam.orthographic)
            {
                cam.orthographicSize = baseSize - zoomPunch;
            }
        }
    }

    /// <summary>
    /// Moves a generated group relative to the camera to fake depth in an orthographic view.
    /// factor 0 = locked to the world, 1 = locked to the camera (infinitely far), negative = foreground.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class PrototypeParallaxLayer : MonoBehaviour
    {
        private Transform cameraTransform;
        private float factorX;
        private float factorY;
        private Vector3 basePosition;
        private float cameraStartX;
        private float cameraStartY;

        /// <summary>
        /// referenceX/referenceY are the camera coordinates at which the layer sits exactly where it
        /// was authored, so layers are authored as if the camera were centred on the level origin.
        /// </summary>
        public void Configure(Transform camera, float horizontalFactor, float verticalFactor, float referenceX = 0f, float referenceY = 2.2f)
        {
            cameraTransform = camera;
            factorX = horizontalFactor;
            factorY = verticalFactor;
            basePosition = transform.position;
            cameraStartX = referenceX;
            cameraStartY = referenceY;
        }

        private void LateUpdate()
        {
            if (cameraTransform == null)
            {
                return;
            }

            Vector3 offset = new(
                (cameraTransform.position.x - cameraStartX) * factorX,
                (cameraTransform.position.y - cameraStartY) * factorY,
                0f);

            transform.position = basePosition + offset;
        }
    }

    /// <summary>Keeps a generated object horizontally aligned with the camera (rain splashes, ground mist).</summary>
    [DefaultExecutionOrder(100)]
    public sealed class PrototypeFollowCameraX : MonoBehaviour
    {
        private Transform cameraTransform;

        public void Configure(Transform camera)
        {
            cameraTransform = camera;
        }

        private void LateUpdate()
        {
            if (cameraTransform == null)
            {
                return;
            }

            Vector3 p = transform.position;
            p.x = cameraTransform.position.x;
            transform.position = p;
        }
    }

    /// <summary>Slow horizontal drift for mist cards, clouds and light shafts.</summary>
    public sealed class PrototypeDrift : MonoBehaviour
    {
        private Vector3 origin;
        private float amplitude;
        private float speed;
        private float phase;

        public void Configure(float driftAmplitude, float driftSpeed, float startPhase)
        {
            origin = transform.localPosition;
            amplitude = driftAmplitude;
            speed = driftSpeed;
            phase = startPhase;
        }

        private void Update()
        {
            Vector3 p = origin;
            p.x += Mathf.Sin(Time.time * speed + phase) * amplitude;
            transform.localPosition = p;
        }
    }
}
