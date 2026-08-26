#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WrestlingUniverse.UI;

namespace WrestlingUniverse.Editor
{
    public static class UniverseWorkspaceBuilder
    {
        private const string ScenePath = "Assets/WrestlingUniverse/Scenes/UniverseWorkspace.unity";
        private static Font font;

        [InitializeOnLoadMethod]
        private static void BuildWhenMissing()
        {
            if (!File.Exists(ScenePath) || !File.ReadAllText(ScenePath).Contains("m_Name: WorkspaceNavigation"))
                EditorApplication.delayCall += Build;
        }

        [MenuItem("Wrestling Universe/Build Universe Workspace")]
        public static void Build()
        {
            Directory.CreateDirectory("Assets/WrestlingUniverse/Scenes");
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var previousScene = SceneManager.GetActiveScene().path;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = new GameObject("Main Camera", typeof(Camera));
            camera.tag = "MainCamera";
            camera.GetComponent<Camera>().backgroundColor = new Color32(5, 9, 20, 255);
            camera.GetComponent<Camera>().orthographic = true;
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

            var canvasObject = new GameObject("UniverseWorkspaceCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;

            var background = Image("Background", canvasObject.transform, new Color32(5, 9, 20, 255), Vector2.zero, Vector2.one);
            var header = Image("Header", background.transform, new Color32(2, 4, 9, 255), new Vector2(0, .88f), Vector2.one);
            var initials = Text("PromotionInitials", header.transform, "WU", 30, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft, new Vector2(.03f, 0), new Vector2(.13f, 1), FontStyle.Bold);
            var promotion = Text("PromotionName", header.transform, "UNIVERSE", 25, Color.white, TextAnchor.MiddleLeft, new Vector2(.13f, 0), new Vector2(.58f, 1), FontStyle.Bold);
            var status = Text("Status", header.transform, "UNIVERSE LOADED", 14, new Color32(240, 190, 42, 255), TextAnchor.MiddleRight, new Vector2(.60f, 0), new Vector2(.82f, 1), FontStyle.Bold);
            var back = Button("BackButton", header.transform, "BACK TO UNIVERSES", new Vector2(.84f, .18f), new Vector2(.97f, .82f));

            var navigation = Image("WorkspaceNavigation", background.transform, new Color32(2, 4, 9, 255), new Vector2(0, .79f), new Vector2(1, .88f));
            var myUniverse = NavigationButton("MyUniverseButton", navigation.transform, "MY UNIVERSE", .18f, .31f);
            var roster = NavigationButton("RosterButton", navigation.transform, "ROSTER", .31f, .43f);
            var booking = NavigationButton("BookingButton", navigation.transform, "BOOKING", .43f, .55f);
            var results = NavigationButton("ResultsButton", navigation.transform, "RESULTS", .55f, .67f);
            var analytics = NavigationButton("AnalyticsButton", navigation.transform, "ANALYTICS", .67f, .80f);

            var section = Text("Section", background.transform, "UNIVERSE DASHBOARD", 16, new Color32(45, 190, 230, 255), TextAnchor.LowerLeft, new Vector2(.04f, .68f), new Vector2(.6f, .76f), FontStyle.Bold);
            Text("Title", background.transform, "WELCOME TO YOUR UNIVERSE", 48, Color.white, TextAnchor.MiddleLeft, new Vector2(.04f, .55f), new Vector2(.85f, .68f), FontStyle.Bold);
            var owner = Text("Owner", background.transform, "OWNER  /", 18, new Color32(142, 160, 181, 255), TextAnchor.MiddleLeft, new Vector2(.04f, .47f), new Vector2(.46f, .55f));
            var startDate = Text("StartDate", background.transform, "UNIVERSE START  /", 18, new Color32(142, 160, 181, 255), TextAnchor.MiddleLeft, new Vector2(.48f, .47f), new Vector2(.9f, .55f));
            var content = Image("FeatureWorkspace", background.transform, new Color32(9, 15, 29, 255), new Vector2(.04f, .07f), new Vector2(.96f, .43f));
            var placeholder = Text("Placeholder", content.transform, "YOUR UNIVERSE FEATURES WILL LIVE HERE\n\nRoster  /  Shows  /  Championships  /  Teams  /  Stables  /  Match History", 22, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter, new Vector2(.05f, .1f), new Vector2(.95f, .9f), FontStyle.Bold);

            var controllerObject = new GameObject("UniverseWorkspaceController");
            var controller = controllerObject.AddComponent<UniverseWorkspaceController>();
            Set(controller, "promotionNameText", promotion); Set(controller, "promotionInitialsText", initials);
            Set(controller, "ownerText", owner); Set(controller, "startDateText", startDate); Set(controller, "statusText", status);
            Set(controller, "sectionTitleText", section); Set(controller, "sectionContentText", placeholder);
            UnityEventTools.AddPersistentListener(back.onClick, controller.ReturnToLandingPage);
            UnityEventTools.AddPersistentListener(myUniverse.onClick, controller.ShowMyUniverse);
            UnityEventTools.AddPersistentListener(roster.onClick, controller.ShowRoster);
            UnityEventTools.AddPersistentListener(booking.onClick, controller.ShowBooking);
            UnityEventTools.AddPersistentListener(results.onClick, controller.ShowResults);
            UnityEventTools.AddPersistentListener(analytics.onClick, controller.ShowAnalytics);

            EditorSceneManager.SaveScene(scene, ScenePath);
            var landingPath = "Assets/WrestlingUniverse/Scenes/LandingPage.unity";
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(landingPath, true), new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            if (!string.IsNullOrEmpty(previousScene) && previousScene != ScenePath)
                EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
            Debug.Log("Universe workspace built at " + ScenePath);
        }

        private static GameObject Image(string name, Transform parent, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(parent, false);
            Rect(go, min, max); go.GetComponent<Image>().color = color; return go;
        }

        private static Text Text(string name, Transform parent, string value, int size, Color color, TextAnchor anchor, Vector2 min, Vector2 max, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false); Rect(go, min, max);
            var text = go.GetComponent<Text>(); text.text = value; text.font = font; text.fontSize = size; text.color = color; text.alignment = anchor; text.fontStyle = style; return text;
        }

        private static Button Button(string name, Transform parent, string label, Vector2 min, Vector2 max)
        {
            var go = Image(name, parent, new Color32(25, 45, 65, 255), min, max); var button = go.AddComponent<Button>(); button.targetGraphic = go.GetComponent<Image>();
            Text("Label", go.transform, label, 14, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, FontStyle.Bold); return button;
        }

        private static Button NavigationButton(string name, Transform parent, string label, float minX, float maxX)
        {
            var button = Button(name, parent, label, new Vector2(minX, 0), new Vector2(maxX, 1));
            button.GetComponent<Image>().color = new Color32(2, 4, 9, 255);
            var underline = Image("ActiveUnderline", button.transform, new Color32(45, 190, 230, 255), new Vector2(.15f, 0), new Vector2(.85f, .045f));
            underline.SetActive(name == "MyUniverseButton");
            return button;
        }

        private static void Rect(GameObject go, Vector2 min, Vector2 max)
        {
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        }

        private static void Set(Object target, string property, Object value)
        {
            var serialized = new SerializedObject(target); serialized.FindProperty(property).objectReferenceValue = value; serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
