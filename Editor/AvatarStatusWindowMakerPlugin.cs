using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using nadena.dev.ndmf;
using UnityEditor;
using System.Threading.Tasks;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.ScriptableObjects;

[assembly: ExportsPlugin(typeof(Narazaka.VRChat.AvatarStatusWindowMaker.Editor.AvatarStatusWindowMakerPlugin))]

namespace Narazaka.VRChat.AvatarStatusWindowMaker.Editor
{
    public class AvatarStatusWindowMakerPlugin : Plugin<AvatarStatusWindowMakerPlugin>
    {
        public override string DisplayName => "Avatar Status Window Maker";
        public override string QualifiedName => "net.narazaka.vrchat.avatar-status-window-maker";

        protected override void Configure()
        {
            InPhase(BuildPhase.FirstChance).Run("Avatar Status Window Maker: Move Values", (context) =>
            {
                var avatarStatusWindowMakers = context.AvatarRootTransform.GetComponentsInChildren<AvatarStatusWindowMaker>(true);
                foreach (var avatarStatusWindowMaker in avatarStatusWindowMakers)
                {
                    var valuesContainer = new GameObject("Values").transform;
                    valuesContainer.SetParent(avatarStatusWindowMaker.transform, false);
                    foreach (Transform statusLine in avatarStatusWindowMaker.transform.Find("Canvas/Container/StatusLines"))
                    {
                        var valueModel = statusLine.Find("Value/ValueModel");
                        valueModel.name = statusLine.name;
                        valueModel.SetParent(valuesContainer, true);
                        var valueRenderer = valueModel.GetComponent<Renderer>();
                        valueRenderer.sharedMaterial = new Material(valueRenderer.sharedMaterial);
                    }
                }
            });
            InPhase(BuildPhase.Generating).BeforePlugin("nadena.dev.modular-avatar").BeforePlugin("net.narazaka.vrchat.compressed-int-parameters").Run("Avatar Status Window Maker", (context) =>
            {
                var avatarStatusWindowMakers = context.AvatarRootTransform.GetComponentsInChildren<AvatarStatusWindowMaker>(true);
                foreach (var avatarStatusWindowMaker in avatarStatusWindowMakers)
                {
                    var mesh = avatarStatusWindowMaker.transform.Find("Mesh").GetComponent<MeshRenderer>();
                    var valuesContainer = avatarStatusWindowMaker.transform.Find("Values");
                    var texture = Object.Instantiate(mesh.sharedMaterial.mainTexture) as Texture2D;
                    EditorUtility.CompressTexture(texture, TextureFormat.DXT1, TextureCompressionQuality.Normal);
                    texture.Apply(true, true);
                    var textureSo = new SerializedObject(texture);
                    textureSo.FindProperty("m_StreamingMipmaps").boolValue = true;
                    textureSo.ApplyModifiedPropertiesWithoutUndo();
                    //UnityEngine.Object.DestroyImmediate(texture);

                    var meshRenderer = mesh.GetComponent<MeshRenderer>();
                    meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial) { mainTexture = texture };
                    mesh.gameObject.SetActive(true);

                    var allMenuGroup = avatarStatusWindowMaker.transform.Find("Menu");
                    for (var i = 0; i < avatarStatusWindowMaker.statuses.Count; ++i)
                    {
                        var status = avatarStatusWindowMaker.statuses[i];
                        if (!status.menu) continue;
                        var valueModel = valuesContainer.GetChild(i).gameObject;
                        var mergeAnimator = valueModel.AddComponent<ModularAvatarMergeAnimator>();
                        mergeAnimator.animator = Util.animator;
                        mergeAnimator.layerType = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.FX;
                        mergeAnimator.matchAvatarWriteDefaults = true;
                        mergeAnimator.pathMode = MergeAnimatorPathMode.Relative;
#if HAS_COMPRESSED_INT_PARAMETERS
                        var compressed = StatusCompressedParameterConfig(status);
                        if (compressed != null)
                        {
                            valueModel.AddComponent<Narazaka.VRChat.CompressedIntParameters.CompressedIntParameters>().parameters = new() { compressed };
                            // 暫定対処: シェーダーは切り捨て表示、圧縮は最寄りの刻みへの丸めなので、min/maxを+0.5して表示を四捨五入に揃える。本来はシェーダー側で丸め方を選べるようにすべき
                            var valueRenderer = valueModel.GetComponent<Renderer>();
                            valueRenderer.sharedMaterial.SetFloat("_MinNumber", status.min + 0.5f);
                            valueRenderer.sharedMaterial.SetFloat("_MaxNumber", status.max + 0.5f);
                        }
                        else
#endif
                        {
                            valueModel.AddComponent<ModularAvatarParameters>().parameters = new() { StatusParameterConfig(status) };
                        }
                        var menuObj = new GameObject(status.name);
                        menuObj.transform.SetParent(allMenuGroup);
                        var menu = menuObj.AddComponent<ModularAvatarMenuItem>();
                        menu.Control.name = status.name;
                        menu.Control.type = VRCExpressionsMenu.Control.ControlType.RadialPuppet;
                        menu.Control.subParameters = new VRCExpressionsMenu.Control.Parameter[]
                        {
                            new ()
                            {
                                name = $"NumberRate_{status.name}",
                            },
                        };
                    }
                    avatarStatusWindowMaker.gameObject.SetActive(avatarStatusWindowMaker.defaultActive);
                    Object.DestroyImmediate(avatarStatusWindowMaker);
                }
            });
        }

        internal static ParameterConfig StatusParameterConfig(AvatarStatus status) => new()
        {
            nameOrPrefix = "NumberRate",
            syncType = ParameterSyncType.Float,
            defaultValue = status.valueRate,
            remapTo = $"NumberRate_{status.name}",
            saved = status.saved,
        };

#if HAS_COMPRESSED_INT_PARAMETERS
        internal static bool CanCompress(int stepCount) => stepCount >= 2 && stepCount <= 128;

        internal static Narazaka.VRChat.CompressedIntParameters.CompressedParameterConfig StatusCompressedParameterConfig(AvatarStatus status)
        {
            if (status.noCompress || !CanCompress(status.stepCount)) return null;
            return Narazaka.VRChat.CompressedIntParameters.CompressedParameterConfig.From(StatusParameterConfig(status), status.stepCount, 0f, 1f);
        }
#endif

        internal static IEnumerable<ParameterConfig> StatusParameterConfigs(AvatarStatus status)
        {
#if HAS_COMPRESSED_INT_PARAMETERS
            var compressed = StatusCompressedParameterConfig(status);
            if (compressed != null) return compressed.ToParameterConfigs();
#endif
            return new[] { StatusParameterConfig(status) };
        }
    }
}
