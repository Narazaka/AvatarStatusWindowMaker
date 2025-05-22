using nadena.dev.ndmf;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;

namespace Narazaka.VRChat.AvatarStatusWindowMaker.Editor
{
    [ParameterProviderFor(typeof(AvatarStatusWindowMaker))]
    public class AvatarStatusWindowMakerParameterProvider : IParameterProvider
    {
        readonly AvatarStatusWindowMaker WindowMaker;

        public AvatarStatusWindowMakerParameterProvider(AvatarStatusWindowMaker c)
        {
            WindowMaker = c;
        }

        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null)
        {
            return WindowMaker.statuses.Where(status => status.menu).Select(status => new ProvidedParameter($"NumberRate_{status.name}", ParameterNamespace.Animator, WindowMaker, AvatarStatusWindowMakerPlugin.Instance, AnimatorControllerParameterType.Float)
            {
                WantSynced = true,
            });
        }

        public void RemapParameters(ref ImmutableDictionary<(ParameterNamespace, string), ParameterMapping> nameMap,
            BuildContext context)
        {
            // no-op
        }
    }
}
