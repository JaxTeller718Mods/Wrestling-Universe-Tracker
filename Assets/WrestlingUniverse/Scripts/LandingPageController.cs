using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using WrestlingUniverse.Persistence;
using WrestlingUniverse.Platform;

namespace WrestlingUniverse.UI
{
    public sealed class LandingPageController : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private GameObject modalOverlay;
        [SerializeField] private Transform universeList;
        [SerializeField] private GameObject universeCardTemplate;
        [SerializeField] private Text emptyStateText;
        [SerializeField] private Text universeCountText;

        [Header("Create Universe Form")]
        [SerializeField] private InputField ownerNameInput;
        [SerializeField] private InputField promotionNameInput;
        [SerializeField] private InputField initialsInput;
        [SerializeField] private InputField startDateInput;
        [SerializeField] private Text validationText;
        [SerializeField] private Text ownerImageStatus;
        [SerializeField] private Text promotionImageStatus;

        private readonly List<UniverseDraft> universes = new List<UniverseDraft>();
        private UniverseSaveRepository repository;
        private readonly List<Texture2D> loadedTextures = new List<Texture2D>();
        private readonly Dictionary<string, GameObject> cardsByUniverseId = new Dictionary<string, GameObject>();
        private string selectedOwnerImagePath;
        private string selectedPromotionImagePath;
        private UniverseDraft editingUniverse;

        private void Awake()
        {
            EnsureExitButton();
            modalOverlay.SetActive(false);
            universeCardTemplate.SetActive(false);
            LoadSavedUniverses();
            RefreshSummary();
        }

        public void ExitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void EnsureExitButton()
        {
            var root = universeCountText != null ? universeCountText.transform.root : transform.root;
            var background = root.Find("Background");
            if (background == null || background.Find("ExitApplicationButton") != null) return;
            var create = background.Find("CreateUniverseTop");
            if (create == null) return;

            var exitObject = Instantiate(create.gameObject, background);
            exitObject.name = "ExitApplicationButton";
            var rect = exitObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(.73f, .395f); rect.anchorMax = new Vector2(.96f, .445f);
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var image = exitObject.GetComponent<Image>(); if (image != null) image.color = new Color32(44, 57, 74, 255);
            var label = exitObject.transform.Find("Label"); if (label != null) label.GetComponent<Text>().text = "EXIT APP";
            var button = exitObject.GetComponent<Button>(); button.onClick.RemoveAllListeners(); button.onClick.AddListener(ExitApplication);

            var list = background.Find("UniverseList");
            if (list != null)
            {
                var listRect = list.GetComponent<RectTransform>();
                listRect.anchorMax = new Vector2(.96f, .385f); listRect.offsetMax = Vector2.zero;
            }
        }

        public void OpenCreateUniverse()
        {
            editingUniverse = null;
            SetFormMode(false);
            validationText.text = string.Empty;
            ownerNameInput.text = string.Empty;
            promotionNameInput.text = string.Empty;
            initialsInput.text = string.Empty;
            startDateInput.text = DateTime.Today.ToString("yyyy-MM-dd");
            ownerImageStatus.text = "No image selected";
            promotionImageStatus.text = "No image selected";
            selectedOwnerImagePath = string.Empty;
            selectedPromotionImagePath = string.Empty;
            ClearPreview(FindImageButton("OwnerImageButton"));
            ClearPreview(FindImageButton("PromotionImageButton"));
            modalOverlay.SetActive(true);
            ownerNameInput.Select();
        }

        public void CloseCreateUniverse()
        {
            modalOverlay.SetActive(false);
            editingUniverse = null;
        }

        private void OpenManageUniverse(UniverseDraft universe)
        {
            editingUniverse = universe;
            SetFormMode(true);
            validationText.text = string.Empty;
            ownerNameInput.text = universe.ownerName;
            promotionNameInput.text = universe.promotionName;
            initialsInput.text = universe.promotionInitials;
            startDateInput.text = universe.startDate;
            selectedOwnerImagePath = string.Empty;
            selectedPromotionImagePath = string.Empty;
            ShowExistingPreview(FindImageButton("OwnerImageButton"), universe.ownerImagePath, ownerImageStatus);
            ShowExistingPreview(FindImageButton("PromotionImageButton"), universe.promotionImagePath, promotionImageStatus);
            modalOverlay.SetActive(true);
            ownerNameInput.Select();
        }

        public void UseOwnerImagePlaceholder()
        {
            PickImage(true);
        }

        public void UsePromotionImagePlaceholder()
        {
            PickImage(false);
        }

        public void CreateUniverse()
        {
            DateTime parsedDate;
            if (string.IsNullOrWhiteSpace(ownerNameInput.text) ||
                string.IsNullOrWhiteSpace(promotionNameInput.text) ||
                string.IsNullOrWhiteSpace(initialsInput.text))
            {
                validationText.text = "Owner, promotion, and initials are required.";
                return;
            }

            if (!DateTime.TryParseExact(startDateInput.text.Trim(), "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out parsedDate))
            {
                validationText.text = "Use YYYY-MM-DD for the start date.";
                return;
            }

            var initials = initialsInput.text.Trim().ToUpperInvariant();
            if (initials.Length > 8)
            {
                validationText.text = "Promotion initials must be 8 characters or fewer.";
                return;
            }

            var isEditing = editingUniverse != null;
            var draft = new UniverseDraft
            {
                id = isEditing ? editingUniverse.id : Guid.NewGuid().ToString("N"),
                ownerName = ownerNameInput.text.Trim(),
                promotionName = promotionNameInput.text.Trim(),
                promotionInitials = initials,
                startDate = parsedDate.ToString("yyyy-MM-dd"),
                createdUtc = isEditing ? editingUniverse.createdUtc : DateTime.UtcNow.ToString("O"),
                ownerImagePath = isEditing ? editingUniverse.ownerImagePath : string.Empty,
                promotionImagePath = isEditing ? editingUniverse.promotionImagePath : string.Empty
            };

            try
            {
                if (!string.IsNullOrEmpty(selectedOwnerImagePath))
                    draft.ownerImagePath = UniverseImageStorage.Import(draft.id, selectedOwnerImagePath, "owner");
                if (!string.IsNullOrEmpty(selectedPromotionImagePath))
                    draft.promotionImagePath = UniverseImageStorage.Import(draft.id, selectedPromotionImagePath, "promotion");
                repository.Save(draft);
                if (isEditing)
                {
                    var index = universes.FindIndex(item => item.id == draft.id);
                    if (index >= 0) universes[index] = draft;
                    GameObject existingCard;
                    if (cardsByUniverseId.TryGetValue(draft.id, out existingCard))
                        PopulateUniverseCard(existingCard, draft, index + 1);
                }
                else
                {
                    universes.Add(draft);
                    AddUniverseCard(draft, universes.Count);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                validationText.text = "The universe could not be saved. Check the Console for details.";
                return;
            }
            CloseCreateUniverse();
            RefreshSummary();
        }

        private void LoadSavedUniverses()
        {
            try
            {
                repository = new UniverseSaveRepository();
                repository.Initialize();
                universes.Clear();
                universes.AddRange(repository.LoadAll());
                for (var index = 0; index < universes.Count; index++)
                    AddUniverseCard(universes[index], index + 1);

                Debug.Log("Universe saves loaded from: " + repository.DatabasePath);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                universes.Clear();
                emptyStateText.text = "SAVE DATABASE COULD NOT BE OPENED\n\nCheck the Console for details.";
            }
        }

        private void AddUniverseCard(UniverseDraft draft, int slot)
        {
            var card = Instantiate(universeCardTemplate, universeList);
            card.name = "UniverseCard_" + slot;
            card.SetActive(true);
            cardsByUniverseId[draft.id] = card;
            PopulateUniverseCard(card, draft, slot);
        }

        private void PopulateUniverseCard(GameObject card, UniverseDraft draft, int slot)
        {
            SetChildText(card.transform, "Slot", "SLOT " + slot);
            SetChildText(card.transform, "Initials", draft.promotionInitials);
            SetChildText(card.transform, "Promotion", draft.promotionName);
            SetChildText(card.transform, "Owner", "OWNER  /  " + draft.ownerName);
            SetChildText(card.transform, "StartDate", "START DATE  /  " + draft.startDate);
            AddCardImage(card.transform, "OwnerPortrait", draft.ownerImagePath, new Vector2(.70f, .55f), new Vector2(.82f, .92f));
            AddCardImage(card.transform, "PromotionLogo", draft.promotionImagePath, new Vector2(.83f, .55f), new Vector2(.95f, .92f));
            AddManageButton(card.transform, draft);
        }

        private void AddManageButton(Transform card, UniverseDraft draft)
        {
            var legacy = card.Find("ComingSoon");
            if (legacy != null) legacy.gameObject.SetActive(false);

            var existing = card.Find("ManageButton");
            GameObject buttonObject;
            if (existing == null)
            {
                buttonObject = new GameObject("ManageButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                buttonObject.transform.SetParent(card, false);
                var rect = buttonObject.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(.06f, .04f);
                rect.anchorMax = new Vector2(.57f, .19f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                buttonObject.GetComponent<Image>().color = new Color32(25, 45, 65, 255);

                var labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                labelObject.transform.SetParent(buttonObject.transform, false);
                var labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                var label = labelObject.GetComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.fontSize = 16;
                label.fontStyle = FontStyle.Bold;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = Color.white;
                label.text = "MANAGE / EDIT";
            }
            else
            {
                buttonObject = existing.gameObject;
            }

            var button = buttonObject.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OpenManageUniverse(draft));
            AddLoadButton(card, draft);
        }

        private void AddLoadButton(Transform card, UniverseDraft draft)
        {
            var existing = card.Find("LoadButton");
            GameObject buttonObject;
            if (existing == null)
            {
                buttonObject = new GameObject("LoadButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                buttonObject.transform.SetParent(card, false);
                var rect = buttonObject.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(.59f, .04f);
                rect.anchorMax = new Vector2(.94f, .19f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                buttonObject.GetComponent<Image>().color = new Color32(240, 190, 42, 255);

                var labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                labelObject.transform.SetParent(buttonObject.transform, false);
                var labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                var label = labelObject.GetComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.fontSize = 16;
                label.fontStyle = FontStyle.Bold;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = new Color32(5, 9, 20, 255);
                label.text = "LOAD UNIVERSE";
            }
            else
            {
                buttonObject = existing.gameObject;
            }

            var button = buttonObject.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => LoadUniverse(draft.id));
        }

        private static void LoadUniverse(string universeId)
        {
            ActiveUniverseSession.Select(universeId);
            SceneManager.LoadScene("UniverseWorkspace");
        }

        private void PickImage(bool ownerImage)
        {
            string path;
            if (!WindowsImageFilePicker.TryPickImage(out path)) return;

            try
            {
                var texture = UniverseImageStorage.LoadTexture(path);
                if (texture == null) throw new InvalidOperationException("Unity could not decode the selected image.");
                loadedTextures.Add(texture);

                if (ownerImage)
                {
                    selectedOwnerImagePath = path;
                    ownerImageStatus.text = System.IO.Path.GetFileName(path);
                    SetPreview(FindImageButton("OwnerImageButton"), texture);
                }
                else
                {
                    selectedPromotionImagePath = path;
                    promotionImageStatus.text = System.IO.Path.GetFileName(path);
                    SetPreview(FindImageButton("PromotionImageButton"), texture);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                validationText.text = exception.Message;
            }
        }

        private GameObject FindImageButton(string objectName)
        {
            var found = modalOverlay.transform.Find("Dialog/" + objectName);
            return found == null ? null : found.gameObject;
        }

        private void AddCardImage(Transform card, string name, string path, Vector2 anchorMin, Vector2 anchorMax)
        {
            var oldPreview = card.Find(name);
            if (oldPreview != null) Destroy(oldPreview.gameObject);
            var texture = UniverseImageStorage.LoadTexture(path);
            if (texture == null) return;
            loadedTextures.Add(texture);
            var preview = CreatePreview(card, name, anchorMin, anchorMax);
            ApplyTexture(preview, texture);
        }

        private void ShowExistingPreview(GameObject host, string path, Text status)
        {
            ClearPreview(host);
            var texture = UniverseImageStorage.LoadTexture(path);
            if (texture == null)
            {
                status.text = "No image selected";
                return;
            }
            loadedTextures.Add(texture);
            status.text = System.IO.Path.GetFileName(path);
            SetPreview(host, texture);
        }

        private void SetFormMode(bool manageMode)
        {
            var title = modalOverlay.transform.Find("Dialog/Title");
            if (title != null) title.GetComponent<Text>().text = manageMode ? "MANAGE UNIVERSE" : "CREATE UNIVERSE";
            var submitLabel = modalOverlay.transform.Find("Dialog/Create/Label");
            if (submitLabel != null) submitLabel.GetComponent<Text>().text = manageMode ? "SAVE CHANGES" : "CREATE UNIVERSE";
        }

        private static void SetPreview(GameObject host, Texture2D texture)
        {
            if (host == null) return;
            var existing = host.transform.Find("SelectedImagePreview");
            var preview = existing != null ? existing.GetComponentInChildren<RawImage>() :
                CreatePreview(host.transform, "SelectedImagePreview", new Vector2(.02f, .08f), new Vector2(.98f, .92f));
            ApplyTexture(preview, texture);
            preview.transform.parent.SetAsFirstSibling();
        }

        private static void ClearPreview(GameObject host)
        {
            if (host == null) return;
            var preview = host.transform.Find("SelectedImagePreview");
            if (preview != null) Destroy(preview.gameObject);
        }

        private static RawImage CreatePreview(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            // Keep the anchor region on a container. AspectRatioFitter changes its own
            // RectTransform, so placing it on the anchored object makes it fill the card.
            var container = new GameObject(name, typeof(RectTransform));
            container.transform.SetParent(parent, false);
            var containerRect = container.GetComponent<RectTransform>();
            containerRect.anchorMin = anchorMin;
            containerRect.anchorMax = anchorMax;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            var previewObject = new GameObject("Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(AspectRatioFitter));
            previewObject.transform.SetParent(container.transform, false);
            var imageRect = previewObject.GetComponent<RectTransform>();
            imageRect.anchorMin = new Vector2(.5f, .5f);
            imageRect.anchorMax = new Vector2(.5f, .5f);
            imageRect.anchoredPosition = Vector2.zero;
            imageRect.sizeDelta = Vector2.zero;
            var image = previewObject.GetComponent<RawImage>();
            image.raycastTarget = false;
            previewObject.GetComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            return image;
        }

        private static void ApplyTexture(RawImage preview, Texture2D texture)
        {
            preview.texture = texture;
            preview.color = Color.white;
            preview.GetComponent<AspectRatioFitter>().aspectRatio = (float)texture.width / texture.height;
        }

        private void OnDestroy()
        {
            foreach (var texture in loadedTextures)
                if (texture != null) Destroy(texture);
            loadedTextures.Clear();
        }

        private void RefreshSummary()
        {
            universeCountText.text = universes.Count.ToString();
            emptyStateText.gameObject.SetActive(universes.Count == 0);
        }

        private static void SetChildText(Transform root, string childName, string value)
        {
            var child = root.Find(childName);
            if (child != null) child.GetComponent<Text>().text = value;
        }
    }
}
