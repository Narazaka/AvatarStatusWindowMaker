using nadena.dev.modular_avatar.core;
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
            return WindowMaker.statuses.Where(status => status.menu).SelectMany(AvatarStatusWindowMakerPlugin.StatusParameterConfigs).Select(p => new ProvidedParameter(p.remapTo, ParameterNamespace.Animator, WindowMaker, AvatarStatusWindowMakerPlugin.Instance, p.syncType == ParameterSyncType.Bool ? AnimatorControllerParameterType.Bool : AnimatorControllerParameterType.Float)
            {
                WantSynced = !p.localOnly,
            });
        }

        public void RemapParameters(ref ImmutableDictionary<(ParameterNamespace, string), ParameterMapping> nameMap,
            BuildContext context)
        {
            // no-op
        }
    }
}
