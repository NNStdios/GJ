using UnityEngine;
using NnUtils.Scripts;
using System.Collections;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.Core
{
    [RequireComponent(typeof(DecalProjector))]
    public class DecalFade : MonoBehaviour
    {
        [ReadOnly][SerializeField] private DecalProjector _decalProjector;
        [SerializeField] private float _lifeTime = 10;
        [SerializeField] private float _fadeTime = 5;
        [SerializeField] private Easings.Type _fadeEasing = Easings.Type.SineIn;

        private void Reset()
        {
            _decalProjector = GetComponent<DecalProjector>();
        }

        private IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(_lifeTime);

            float lerpPos = 1;
            while (lerpPos > 0)
            {
                var t = Misc.ReverseTween(ref lerpPos, _fadeTime, _fadeEasing, false, true);
                _decalProjector.fadeFactor = t;
                yield return null;
            }
            
            Destroy(gameObject);
        }
    }
}