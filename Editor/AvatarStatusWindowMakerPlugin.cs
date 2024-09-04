using System.Collections;
using System.Linq;
using UnityEngine;
using nadena.dev.ndmf;
using UnityEngine.UI;
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
            InPhase(BuildPhase.Generating).BeforePlugin("nadena.dev.modular-avatar").Run("Avatar Status Window Maker", (context) =>
            {
                var avatarStatusWindowMakers = context.AvatarRootTransform.GetComponentsInChildren<AvatarStatusWindowMaker>();
                foreach (var avatarStatusWindowMaker in avatarStatusWindowMakers)
                {
                    var canvas = avatarStatusWindowMaker.transform.Find("Canvas");
                    var size = canvas.GetComponent<RectTransform>().sizeDelta;
                    var camera = avatarStatusWindowMaker.transform.Find("Camera").GetComponent<Camera>();
                    var mesh = avatarStatusWindowMaker.transform.Find("Mesh").GetComponent<MeshRenderer>();
                    /*
                    var active = RenderTexture.active;
                    // camera.targetTexture = new RenderTexture(Mathf.RoundToInt(1024 * size.x / 1000), Mathf.RoundToInt(1024 * size.y / 1000), 24);
                    RenderTexture.active = camera.targetTexture;
                    camera.Render();
                    var resizedRenderTexture = new RenderTexture(1024, 1024, 24);
                    RenderTexture.active = resizedRenderTexture;
                    Graphics.Blit(camera.targetTexture, resizedRenderTexture);
                    var texture = new Texture2D(resizedRenderTexture.width, resizedRenderTexture.height);
                    texture.ReadPixels(new Rect(0, 0, resizedRenderTexture.width, resizedRenderTexture.height), 0, 0);
                    texture.Apply();
                    RenderTexture.active = active;
                    */
                    var texture = mesh.sharedMaterial.mainTexture as Texture2D;
                    EditorUtility.CompressTexture(texture, TextureFormat.DXT1, TextureCompressionQuality.Normal);
                    texture.Apply(true, true);
                    var textureSo = new SerializedObject(texture);
                    textureSo.FindProperty("m_StreamingMipmaps").boolValue = true;
                    textureSo.ApplyModifiedPropertiesWithoutUndo();
                    UnityEngine.Object.DestroyImmediate(camera.gameObject);
                    //UnityEngine.Object.DestroyImmediate(texture);

                    var statusLinesContainer = canvas.transform.Find("Container/StatusLines").gameObject;

                    Object.DestroyImmediate(canvas.GetComponent<CanvasScaler>());
                    Object.DestroyImmediate(canvas.GetComponent<GraphicRaycaster>());
                    Object.DestroyImmediate(canvas.GetComponent<Canvas>());
                    var container = canvas.Find("Container");
                    Object.DestroyImmediate(container.GetComponent<VerticalLayoutGroup>());
                    Object.DestroyImmediate(statusLinesContainer.GetComponent<VerticalLayoutGroup>());
                    Object.DestroyImmediate(statusLinesContainer.GetComponent<LayoutElement>());
                    /*
                    var fakeCanvas = new GameObject("Canvas");
                    fakeCanvas.transform.SetParent(avatarStatusWindowMaker.transform);
                    fakeCanvas.transform.position = canvas.position;
                    fakeCanvas.transform.rotation = canvas.rotation;
                    fakeCanvas.transform.localScale = canvas.localScale * 1000;
                    var fakeContainer = new GameObject("Container");
                    fakeContainer.transform.SetParent(fakeCanvas.transform);
                    fakeContainer.transform.position = container.position;
                    fakeContainer.transform.rotation = container.rotation;
                    var statusLines = new GameObject("StatusLines");
                    statusLines.transform.SetParent(fakeContainer.transform);
                    statusLines.transform.localPosition = PositionOfRectTransform(statusLinesContainer.transform);
                    statusLines.transform.rotation = statusLinesContainer.transform.rotation;
                    foreach (Transform statusLine in statusLinesContainer.transform)
                    {
                        var fakeStatusLine = new GameObject(statusLine.name);
                        fakeStatusLine.transform.SetParent(statusLines.transform);
                        fakeStatusLine.transform.localPosition = PositionOfRectTransform(statusLine);
                        fakeStatusLine.transform.rotation = statusLine.rotation;
                        var value = statusLine.Find("Value");
                        var fakeStatusValue = new GameObject("Value");
                        fakeStatusValue.transform.SetParent(fakeStatusLine.transform);
                        fakeStatusValue.transform.localPosition = PositionOfRectTransform(value);
                        fakeStatusValue.transform.rotation = value.rotation;
                        var valueModel = statusLine.Find("Value/ValueModel");
                        valueModel.SetParent(fakeStatusValue.transform);
                        valueModel.localPosition = Vector3.zero;
                        valueModel.localRotation = Quaternion.identity;
                    }
                    UnityEngine.Object.DestroyImmediate(canvas.gameObject);
                    */

                    var meshRenderer = mesh.GetComponent<MeshRenderer>();
                    meshRenderer.sharedMaterial.mainTexture = texture;
                    mesh.gameObject.SetActive(true);

                    var allMenuGroup = avatarStatusWindowMaker.transform.Find("Menu");
                    for (var i = 0; i < avatarStatusWindowMaker.statuses.Count; ++i)
                    {
                        var status = avatarStatusWindowMaker.statuses[i];
                        if (!status.menu) continue;
                        var statusLine = statusLinesContainer.transform.GetChild(i);
                        var valueModel = statusLine.Find("Value/ValueModel").gameObject;
                        var mergeAnimator = valueModel.AddComponent<ModularAvatarMergeAnimator>();
                        mergeAnimator.animator = Util.animator;
                        mergeAnimator.layerType = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.FX;
                        mergeAnimator.matchAvatarWriteDefaults = true;
                        mergeAnimator.pathMode = MergeAnimatorPathMode.Relative;
                        var parameters = valueModel.AddComponent<ModularAvatarParameters>();
                        parameters.parameters = new()
                        {
                            new ()
                            {
                                nameOrPrefix = "NumberRate",
                                syncType = ParameterSyncType.Float,
                                defaultValue = status.valueRate,
                                internalParameter = true,
                            },
                        };
                        var menu = valueModel.AddComponent<ModularAvatarMenuItem>();
                        menu.Control.name = status.name;
                        menu.Control.type = VRCExpressionsMenu.Control.ControlType.RadialPuppet;
                        menu.Control.subParameters = new VRCExpressionsMenu.Control.Parameter[]
                        {
                            new ()
                            {
                                name = "NumberRate",
                            },
                        };
                        var menuGroup = new GameObject(status.name);
                        menuGroup.transform.SetParent(allMenuGroup);
                        menuGroup.AddComponent<ModularAvatarMenuGroup>().targetObject = valueModel.transform.parent.gameObject;
                    }
                }
            });
        }

        Vector3 PositionOfRectTransform(Transform transform)
        {
            var rectTransform = transform.GetComponent<RectTransform>();
            return (rectTransform.localPosition + new Vector3(rectTransform.sizeDelta.x / 2f, -rectTransform.sizeDelta.y / 2f, 0)) / 1000f;
            // return rectTransform.localPosition / 1000f;
        }
    }
}
