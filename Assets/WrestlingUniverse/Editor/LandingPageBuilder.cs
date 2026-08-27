#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using WrestlingUniverse.UI;

namespace WrestlingUniverse.Editor
{
    public static class LandingPageBuilder
    {
        private const string Root = "Assets/WrestlingUniverse";
        private const string ScenePath = Root + "/Scenes/LandingPage.unity";
        private static readonly Color Bg = new Color32(5, 9, 20, 255);
        private static readonly Color Panel = new Color32(9, 15, 29, 250);
        private static readonly Color Raised = new Color32(14, 23, 40, 255);
        private static readonly Color Cyan = new Color32(45, 190, 230, 255);
        private static readonly Color Gold = new Color32(240, 190, 42, 255);
        private static readonly Color White = new Color32(242, 246, 250, 255);
        private static readonly Color Muted = new Color32(142, 160, 181, 255);
        private static Font font;

        [InitializeOnLoadMethod]
        private static void BuildFirstLandingPageWhenReady()
        {
            if (!File.Exists(ScenePath))
                EditorApplication.delayCall += Build;
        }

        [MenuItem("Wrestling Universe/Build Landing Page")]
        public static void Build()
        {
            Directory.CreateDirectory(Root + "/Scenes");
            Directory.CreateDirectory(Root + "/Settings");
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            CreateThemeAsset();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera", typeof(Camera));
            camera.tag = "MainCamera";
            camera.GetComponent<Camera>().backgroundColor = Bg;
            camera.GetComponent<Camera>().orthographic = true;

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            var canvas = NewUI("LandingPageCanvas", null, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvasComponent = canvas.GetComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var background = Image("Background", canvas.transform, Bg, Vector2.zero, Vector2.one);
            var header = Image("Header", background.transform, new Color32(2, 4, 9, 255), new Vector2(0, .89f), Vector2.one);
            Text("Brand", header.transform, "WRESTLING  UNIVERSE", 32, White, TextAnchor.MiddleLeft, new Vector2(.025f, 0), new Vector2(.45f, 1));
            Text("Version", header.transform, "STAT TRACKER  /  PC", 16, Cyan, TextAnchor.MiddleRight, new Vector2(.65f, 0), new Vector2(.97f, 1));

            var hero = Image("Hero", background.transform, Panel, new Vector2(0, .53f), new Vector2(1, .89f));
            Text("Eyebrow", hero.transform, "YOUR BOOKING. YOUR HISTORY. YOUR UNIVERSE.", 16, Cyan, TextAnchor.LowerLeft, new Vector2(.04f, .72f), new Vector2(.7f, .86f));
            Text("Title", hero.transform, "MY UNIVERSES", 68, White, TextAnchor.MiddleLeft, new Vector2(.04f, .38f), new Vector2(.65f, .75f), FontStyle.Bold);
            Text("Subtitle", hero.transform, "Create and manage every promotion, roster, championship, and show.", 22, Muted, TextAnchor.UpperLeft, new Vector2(.042f, .19f), new Vector2(.7f, .39f));
            var countPanel = Image("UniverseCountPanel", hero.transform, Raised, new Vector2(.76f, .23f), new Vector2(.94f, .72f));
            var countText = Text("Count", countPanel.transform, "0", 58, White, TextAnchor.MiddleCenter, new Vector2(0, .3f), Vector2.one, FontStyle.Bold);
            Text("Label", countPanel.transform, "UNIVERSES", 15, Cyan, TextAnchor.UpperCenter, Vector2.zero, new Vector2(1, .32f));

            Text("SectionTitle", background.transform, "MY UNIVERSES", 20, Cyan, TextAnchor.MiddleLeft, new Vector2(.04f, .465f), new Vector2(.5f, .53f), FontStyle.Bold);
            var createTop = Button("CreateUniverseTop", background.transform, "+  CREATE UNIVERSE", new Vector2(.73f, .455f), new Vector2(.96f, .525f), Gold, Bg);
            var exitApp = Button("ExitApplicationButton", background.transform, "EXIT APP", new Vector2(.73f, .395f), new Vector2(.96f, .445f), new Color32(44, 57, 74, 255), White);
            var list = NewUI("UniverseList", background.transform, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            Rect(list, new Vector2(.04f, .07f), new Vector2(.96f, .385f));
            var layout = list.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 24;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            var empty = Text("EmptyState", list.transform, "NO UNIVERSES YET\n\nCreate your first universe to begin tracking wrestling history.", 24, Muted, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, FontStyle.Bold);
            var emptyLayout = empty.gameObject.AddComponent<LayoutElement>();
            emptyLayout.minWidth = 550;
            emptyLayout.preferredWidth = 1760;
            emptyLayout.flexibleWidth = 1;
            var template = CreateUniverseCard(list.transform);

            var modal = BuildModal(canvas.transform, out var owner, out var promotion, out var initials, out var date,
                out var validation, out var ownerImage, out var promotionImage, out var cancel, out var submit,
                out var ownerImageButton, out var promotionImageButton);

            var controllerObject = new GameObject("LandingPageController");
            var controller = controllerObject.AddComponent<LandingPageController>();
            Set(controller, "modalOverlay", modal); Set(controller, "universeList", list.transform);
            Set(controller, "universeCardTemplate", template); Set(controller, "emptyStateText", empty);
            Set(controller, "universeCountText", countText); Set(controller, "ownerNameInput", owner);
            Set(controller, "promotionNameInput", promotion); Set(controller, "initialsInput", initials);
            Set(controller, "startDateInput", date); Set(controller, "validationText", validation);
            Set(controller, "ownerImageStatus", ownerImage); Set(controller, "promotionImageStatus", promotionImage);
            UnityEventTools.AddPersistentListener(createTop.onClick, controller.OpenCreateUniverse);
            UnityEventTools.AddPersistentListener(exitApp.onClick, controller.ExitApplication);
            UnityEventTools.AddPersistentListener(cancel.onClick, controller.CloseCreateUniverse);
            UnityEventTools.AddPersistentListener(submit.onClick, controller.CreateUniverse);
            UnityEventTools.AddPersistentListener(ownerImageButton.onClick, controller.UseOwnerImagePlaceholder);
            UnityEventTools.AddPersistentListener(promotionImageButton.onClick, controller.UsePromotionImagePlaceholder);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Wrestling Universe landing page built at " + ScenePath);
        }

        private static GameObject BuildModal(Transform parent, out InputField owner, out InputField promotion,
            out InputField initials, out InputField date, out Text validation, out Text ownerImage,
            out Text promotionImage, out Button cancel, out Button submit, out Button ownerImageButton,
            out Button promotionImageButton)
        {
            var overlay = Image("CreateUniverseModal", parent, new Color(0, 0, 0, .78f), Vector2.zero, Vector2.one);
            var panel = Image("Dialog", overlay.transform, Raised, new Vector2(.2f, .08f), new Vector2(.8f, .92f));
            Text("Title", panel.transform, "CREATE UNIVERSE", 42, White, TextAnchor.MiddleLeft, new Vector2(.06f, .86f), new Vector2(.94f, .97f), FontStyle.Bold);
            Text("Intro", panel.transform, "Set the foundation for your new wrestling world.", 18, Muted, TextAnchor.MiddleLeft, new Vector2(.06f, .80f), new Vector2(.94f, .87f));
            owner = Input("OwnerName", panel.transform, "OWNER NAME", "e.g. Paul Levesque", .69f);
            promotion = Input("PromotionName", panel.transform, "PROMOTION NAME", "e.g. Redemption Pro Wrestling", .56f);
            initials = Input("PromotionInitials", panel.transform, "PROMOTION INITIALS", "e.g. RPW", .43f);
            initials.characterLimit = 8;
            date = Input("StartDate", panel.transform, "START DATE  (YYYY-MM-DD)", "2026-08-25", .30f);
            ownerImageButton = Button("OwnerImageButton", panel.transform, "+  OWNER IMAGE", new Vector2(.06f, .18f), new Vector2(.47f, .27f), new Color32(25, 45, 65, 255), White);
            promotionImageButton = Button("PromotionImageButton", panel.transform, "+  PROMOTION IMAGE", new Vector2(.53f, .18f), new Vector2(.94f, .27f), new Color32(25, 45, 65, 255), White);
            ownerImage = Text("OwnerImageStatus", panel.transform, "No image selected", 13, Muted, TextAnchor.UpperLeft, new Vector2(.06f, .145f), new Vector2(.47f, .18f));
            promotionImage = Text("PromotionImageStatus", panel.transform, "No image selected", 13, Muted, TextAnchor.UpperLeft, new Vector2(.53f, .145f), new Vector2(.94f, .18f));
            validation = Text("Validation", panel.transform, "", 15, new Color32(255, 105, 105, 255), TextAnchor.MiddleLeft, new Vector2(.06f, .095f), new Vector2(.65f, .145f));
            cancel = Button("Cancel", panel.transform, "CANCEL", new Vector2(.59f, .035f), new Vector2(.76f, .11f), new Color32(35, 45, 58, 255), White);
            submit = Button("Create", panel.transform, "CREATE UNIVERSE", new Vector2(.77f, .035f), new Vector2(.94f, .11f), Gold, Bg);
            return overlay;
        }

        private static GameObject CreateUniverseCard(Transform parent)
        {
            var card = Image("UniverseCardTemplate", parent, Raised, Vector2.zero, Vector2.one);
            var cardLayout = card.AddComponent<LayoutElement>();
            cardLayout.minWidth = 420;
            cardLayout.preferredWidth = 550;
            cardLayout.flexibleWidth = 0;
            Text("Slot", card.transform, "SLOT 1", 16, Cyan, TextAnchor.MiddleLeft, new Vector2(.06f, .82f), new Vector2(.94f, .96f), FontStyle.Bold);
            Text("Initials", card.transform, "RPW", 46, White, TextAnchor.MiddleLeft, new Vector2(.06f, .60f), new Vector2(.94f, .84f), FontStyle.Bold);
            Text("Promotion", card.transform, "Redemption Pro Wrestling", 20, White, TextAnchor.MiddleLeft, new Vector2(.06f, .47f), new Vector2(.94f, .62f));
            Text("Owner", card.transform, "OWNER  /  Paul Levesque", 15, Muted, TextAnchor.MiddleLeft, new Vector2(.06f, .33f), new Vector2(.94f, .48f));
            Text("StartDate", card.transform, "START DATE  /  2026-08-25", 15, Muted, TextAnchor.MiddleLeft, new Vector2(.06f, .20f), new Vector2(.94f, .34f));
            Text("ComingSoon", card.transform, "SAVE PERSISTENCE COMING WITH SQLITE", 13, Gold, TextAnchor.MiddleLeft, new Vector2(.06f, .04f), new Vector2(.94f, .20f));
            return card;
        }

        private static InputField Input(string name, Transform parent, string label, string placeholder, float y)
        {
            Text(name + "Label", parent, label, 14, Cyan, TextAnchor.LowerLeft, new Vector2(.06f, y + .075f), new Vector2(.94f, y + .12f), FontStyle.Bold);
            var root = Image(name, parent, new Color32(5, 11, 23, 255), new Vector2(.06f, y), new Vector2(.94f, y + .075f));
            var value = Text("Text", root.transform, "", 19, White, TextAnchor.MiddleLeft, new Vector2(.025f, .08f), new Vector2(.975f, .92f));
            var hint = Text("Placeholder", root.transform, placeholder, 19, Muted, TextAnchor.MiddleLeft, new Vector2(.025f, .08f), new Vector2(.975f, .92f));
            var field = root.AddComponent<InputField>(); field.textComponent = value; field.placeholder = hint;
            return field;
        }

        private static Button Button(string name, Transform parent, string label, Vector2 min, Vector2 max, Color color, Color textColor)
        {
            var root = Image(name, parent, color, min, max);
            var button = root.AddComponent<Button>(); button.targetGraphic = root.GetComponent<Image>();
            Text("Label", root.transform, label, 17, textColor, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, FontStyle.Bold);
            return button;
        }

        private static GameObject Image(string name, Transform parent, Color color, Vector2 min, Vector2 max)
        {
            var go = NewUI(name, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Rect(go, min, max); go.GetComponent<Image>().color = color; return go;
        }

        private static Text Text(string name, Transform parent, string value, int size, Color color,
            TextAnchor anchor, Vector2 min, Vector2 max, FontStyle style = FontStyle.Normal)
        {
            var go = NewUI(name, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); Rect(go, min, max);
            var text = go.GetComponent<Text>(); text.text = value; text.font = font; text.fontSize = size;
            text.color = color; text.alignment = anchor; text.fontStyle = style; text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate; return text;
        }

        private static GameObject NewUI(string name, Transform parent, params System.Type[] components)
        {
            var go = new GameObject(name, components); if (parent != null) go.transform.SetParent(parent, false); return go;
        }

        private static void Rect(GameObject go, Vector2 min, Vector2 max)
        {
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        }

        private static void Set(Object target, string property, Object value)
        {
            var serialized = new SerializedObject(target); serialized.FindProperty(property).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateThemeAsset()
        {
            var path = Root + "/Settings/UniverseTheme.asset";
            if (AssetDatabase.LoadAssetAtPath<UniverseTheme>(path) != null) return;
            var theme = ScriptableObject.CreateInstance<UniverseTheme>(); AssetDatabase.CreateAsset(theme, path);
        }
    }
}
#endif
