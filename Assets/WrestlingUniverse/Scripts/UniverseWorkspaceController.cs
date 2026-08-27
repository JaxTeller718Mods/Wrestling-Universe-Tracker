using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WrestlingUniverse.Persistence;
using WrestlingUniverse.Platform;

namespace WrestlingUniverse.UI
{
    public sealed class UniverseWorkspaceController : MonoBehaviour
    {
        [SerializeField] private Text promotionNameText;
        [SerializeField] private Text promotionInitialsText;
        [SerializeField] private Text ownerText;
        [SerializeField] private Text startDateText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text sectionTitleText;
        [SerializeField] private Text sectionContentText;
        [SerializeField] private GameObject rosterView;
        [SerializeField] private GameObject wrestlerCreationPanel;
        [SerializeField] private GameObject teamsView;

        private readonly Color activeNavigation = new Color32(25, 45, 65, 255);
        private readonly Color inactiveNavigation = new Color32(2, 4, 9, 255);
        private readonly List<Texture2D> loadedTextures = new List<Texture2D>();
        private UniverseSaveRepository repository;
        private Transform wrestlerGrid;
        private Text emptyRosterText;
        private Text rosterCountText;
        private InputField wrestlerNameInput;
        private InputField overallInput;
        private Dropdown brandDropdown;
        private Dropdown dispositionDropdown;
        private Dropdown genderDropdown;
        private Dropdown tierDropdown;
        private Text wrestlerPhotoStatus;
        private GameObject wrestlerPhotoHost;
        private Text wrestlerValidationText;
        private string selectedWrestlerPhotoPath;
        private WrestlerRecord editingWrestler;
        private Text wrestlerFormTitle;
        private Text wrestlerSaveLabel;
        private GameObject teamCreationPanel;
        private Transform teamGrid;
        private Text emptyTeamsText;
        private Text teamCountText;
        private InputField teamNameInput;
        private Dropdown teamBrandDropdown;
        private Dropdown teamDispositionDropdown;
        private Text memberSelectionCaption;
        private GameObject memberDropdownMenu;
        private Text teamValidationText;
        private Text teamFormTitle;
        private Text teamSaveLabel;
        private TeamRecord editingTeam;
        private readonly List<string> selectedTeamMemberIds = new List<string>();
        private List<WrestlerRecord> availableTeamMembers = new List<WrestlerRecord>();
        private GameObject teamPhotoHost;
        private Text teamPhotoStatus;
        private string selectedTeamPhotoPath;

        private static readonly string[] Dispositions = { "Face", "Heel", "Neutral" };
        private static readonly string[] Genders = { "Male", "Female", "Neutral" };
        private static readonly string[] Tiers = { "Lower Card", "Mid-Card", "Upper Card", "Main Event" };

        private void Awake()
        {
            EnsureNavigationBar();
            EnsureRosterViews();
            if (string.IsNullOrEmpty(ActiveUniverseSession.UniverseId))
            {
                SceneManager.LoadScene("LandingPage");
                return;
            }

            try
            {
                repository = new UniverseSaveRepository();
                repository.Initialize();
                var universe = repository.LoadById(ActiveUniverseSession.UniverseId);
                if (universe == null)
                {
                    ActiveUniverseSession.Clear();
                    SceneManager.LoadScene("LandingPage");
                    return;
                }

                promotionNameText.text = universe.promotionName;
                promotionInitialsText.text = universe.promotionInitials;
                ownerText.text = "OWNER  /  " + universe.ownerName;
                startDateText.text = "UNIVERSE START  /  " + universe.startDate;
                statusText.text = "UNIVERSE LOADED  /  " + universe.id.Substring(0, 8).ToUpperInvariant();
                ShowMyUniverse();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                statusText.text = "UNIVERSE COULD NOT BE LOADED";
            }
        }

        public void ShowMyUniverse() => SelectSection("MyUniverseButton", "UNIVERSE DASHBOARD",
            "YOUR UNIVERSE FEATURES WILL LIVE HERE\n\nRoster  /  Shows  /  Championships  /  Teams  /  Stables  /  Match History");

        public void ShowRoster()
        {
            SelectSection("RosterButton", "ROSTER", string.Empty);
            sectionContentText.gameObject.SetActive(false);
            rosterView.SetActive(true);
            RefreshRosterCards();
        }

        public void ShowTeams()
        {
            SelectSection("RosterButton", "TEAMS", string.Empty);
            sectionContentText.gameObject.SetActive(false);
            teamsView.SetActive(true);
            RefreshTeamCards();
        }

        public void ShowBooking() => SelectSection("BookingButton", "BOOKING",
            "BOOKING CENTER\n\nShows, events, matches, and segments will be created here.");

        public void ShowResults() => SelectSection("ResultsButton", "RESULTS",
            "RESULTS AND HISTORY\n\nCompleted shows and match histories will appear here.");

        public void ShowAnalytics() => SelectSection("AnalyticsButton", "ANALYTICS",
            "UNIVERSE ANALYTICS\n\nRecords, trends, rankings, and statistics will appear here.");

        private void SelectSection(string activeButton, string title, string content)
        {
            if (sectionTitleText == null || sectionContentText == null) return;
            sectionTitleText.text = title;
            sectionContentText.text = content;
            sectionContentText.gameObject.SetActive(true);
            if (rosterView != null) rosterView.SetActive(false);
            if (wrestlerCreationPanel != null) wrestlerCreationPanel.SetActive(false);
            if (teamsView != null) teamsView.SetActive(false);
            if (teamCreationPanel != null) teamCreationPanel.SetActive(false);
            var navigation = sectionTitleText.transform.root.Find("Background/WorkspaceNavigation");
            if (navigation == null) return;
            foreach (Transform child in navigation)
            {
                var image = child.GetComponent<Image>();
                if (image != null) image.color = child.name == activeButton ? activeNavigation : inactiveNavigation;
                var underline = child.Find("ActiveUnderline");
                if (underline != null) underline.gameObject.SetActive(child.name == activeButton);
            }
        }

        public void ShowWrestlerCreation()
        {
            SelectSection("RosterButton", "SIGN WRESTLER", string.Empty);
            sectionContentText.gameObject.SetActive(false);
            wrestlerCreationPanel.SetActive(true);
            ResetWrestlerForm();
        }

        private void EnsureRosterViews()
        {
            var root = promotionNameText != null ? promotionNameText.transform.root : transform.root;
            var workspace = root.Find("Background/FeatureWorkspace");
            if (workspace == null) return;

            var existingRoster = workspace.Find("RosterView");
            rosterView = existingRoster != null ? existingRoster.gameObject : CreateRosterView(workspace);
            var existingCreation = workspace.Find("WrestlerCreationPanel");
            wrestlerCreationPanel = existingCreation != null ? existingCreation.gameObject : CreateWrestlerCreationPanel(workspace);
            var existingTeams = workspace.Find("TeamsView");
            teamsView = existingTeams != null ? existingTeams.gameObject : CreateTeamsView(workspace);
            var existingTeamCreation = workspace.Find("TeamCreationPanel");
            teamCreationPanel = existingTeamCreation != null ? existingTeamCreation.gameObject : CreateTeamCreationPanel(workspace);
            rosterView.SetActive(false);
            wrestlerCreationPanel.SetActive(false);
            teamsView.SetActive(false);
            teamCreationPanel.SetActive(false);
        }

        private GameObject CreateTeamsView(Transform workspace)
        {
            var view = CreateRuntimePanel("TeamsView", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            var toolbar = CreateRuntimePanel("TeamsToolbar", view.transform, new Color32(12, 21, 37, 255), new Vector2(.02f, .79f), new Vector2(.98f, .96f));
            teamCountText = CreateRuntimeText("TeamCount", toolbar.transform, "TEAMS  /  0", 17, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                new Vector2(.025f, 0), new Vector2(.65f, 1), FontStyle.Bold);
            var create = CreateRuntimeButton("CreateTeamButton", toolbar.transform, "+  CREATE TEAM", new Vector2(.76f, .16f), new Vector2(.975f, .84f),
                new Color32(240, 190, 42, 255), new Color32(5, 9, 20, 255));
            create.onClick.AddListener(ShowTeamCreation);
            var table = CreateRuntimePanel("TeamTable", view.transform, new Color32(5, 11, 23, 255), new Vector2(.02f, .05f), new Vector2(.98f, .74f));
            table.AddComponent<RectMask2D>();
            var teamScroll = table.AddComponent<ScrollRect>();
            teamScroll.horizontal = false; teamScroll.vertical = true;
            teamScroll.movementType = ScrollRect.MovementType.Clamped; teamScroll.scrollSensitivity = 4f;
            teamGrid = new GameObject("TeamCardGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter)).transform;
            teamGrid.SetParent(table.transform, false);
            var teamGridRect = teamGrid.GetComponent<RectTransform>();
            teamGridRect.anchorMin = new Vector2(.02f, 1); teamGridRect.anchorMax = new Vector2(.98f, 1);
            teamGridRect.pivot = new Vector2(.5f, 1); teamGridRect.anchoredPosition = new Vector2(0, -20); teamGridRect.sizeDelta = new Vector2(0, 180);
            var grid = teamGrid.GetComponent<GridLayoutGroup>(); grid.cellSize = new Vector2(330, 180); grid.spacing = new Vector2(18, 18);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 4; grid.childAlignment = TextAnchor.UpperCenter;
            teamGrid.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            teamScroll.viewport = table.GetComponent<RectTransform>(); teamScroll.content = teamGridRect;
            emptyTeamsText = CreateRuntimeText("EmptyTeams", table.transform, "NO TEAMS CREATED\n\nUse CREATE TEAM to assemble up to five roster members.", 20,
                new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter, new Vector2(.08f, .12f), new Vector2(.92f, .88f), FontStyle.Bold);
            return view;
        }

        private GameObject CreateTeamCreationPanel(Transform workspace)
        {
            var panel = CreateRuntimePanel("TeamCreationPanel", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            teamFormTitle = CreateRuntimeText("Title", panel.transform, "CREATE TEAM", 27, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.035f, .84f), new Vector2(.5f, .98f), FontStyle.Bold);
            var back = CreateRuntimeButton("BackToTeamsButton", panel.transform, "BACK TO TEAMS", new Vector2(.80f, .86f), new Vector2(.965f, .96f),
                new Color32(25, 45, 65, 255), Color.white); back.onClick.AddListener(ShowTeams);
            teamNameInput = CreateRuntimeInput("TeamName", panel.transform, "TEAM NAME", "Team name", new Vector2(.035f, .62f), new Vector2(.55f, .80f));
            teamBrandDropdown = CreateRuntimeDropdown("TeamBrand", panel.transform, "BRAND", new[] { "Unassigned" }, 0, new Vector2(.58f, .62f), new Vector2(.965f, .80f));
            teamDispositionDropdown = CreateRuntimeDropdown("TeamDisposition", panel.transform, "DISPOSITION", Dispositions, 0, new Vector2(.035f, .38f), new Vector2(.34f, .57f));

            var memberField = CreateRuntimeButton("MemberSelector", panel.transform, string.Empty, new Vector2(.37f, .38f), new Vector2(.965f, .57f),
                new Color32(5, 11, 23, 255), Color.white);
            var defaultLabel = memberField.transform.Find("Label"); if (defaultLabel != null) Destroy(defaultLabel.gameObject);
            CreateRuntimeText("FieldLabel", memberField.transform, "MEMBERS  (UP TO 5)", 12, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                new Vector2(.04f, .58f), new Vector2(.96f, .94f), FontStyle.Bold);
            memberSelectionCaption = CreateRuntimeText("Value", memberField.transform, "Select roster members", 17, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.04f, .06f), new Vector2(.9f, .62f));
            CreateRuntimeText("Arrow", memberField.transform, "▼", 13, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter,
                new Vector2(.91f, .08f), new Vector2(.98f, .62f));
            memberField.onClick.AddListener(ToggleMemberDropdown);
            memberDropdownMenu = CreateRuntimePanel("MemberDropdown", memberField.transform, new Color32(5, 9, 20, 255), new Vector2(0, -1.9f), new Vector2(1, 0));
            memberDropdownMenu.SetActive(false);

            teamPhotoHost = CreateRuntimePanel("TeamPhoto", panel.transform, new Color32(5, 11, 23, 255), new Vector2(.035f, .07f), new Vector2(.28f, .34f));
            var photoButton = CreateRuntimeButton("ChooseTeamPhoto", teamPhotoHost.transform, "+  CHOOSE PHOTO", new Vector2(.06f, .06f), new Vector2(.94f, .28f),
                new Color32(25, 45, 65, 255), Color.white); photoButton.onClick.AddListener(PickTeamPhoto);
            teamPhotoStatus = CreateRuntimeText("TeamPhotoStatus", panel.transform, "No photo selected", 12, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter,
                new Vector2(.035f, .015f), new Vector2(.28f, .07f));

            teamValidationText = CreateRuntimeText("Validation", panel.transform, string.Empty, 13, new Color32(255, 105, 105, 255), TextAnchor.MiddleLeft,
                new Vector2(.31f, .12f), new Vector2(.72f, .25f), FontStyle.Bold);
            var save = CreateRuntimeButton("SaveTeamButton", panel.transform, "CREATE TEAM", new Vector2(.76f, .14f), new Vector2(.965f, .32f),
                new Color32(240, 190, 42, 255), new Color32(5, 9, 20, 255));
            teamSaveLabel = save.transform.Find("Label").GetComponent<Text>(); save.onClick.AddListener(SaveTeam);
            return panel;
        }

        private GameObject CreateRosterView(Transform workspace)
        {
            var view = CreateRuntimePanel("RosterView", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            var toolbar = CreateRuntimePanel("RosterToolbar", view.transform, new Color32(12, 21, 37, 255), new Vector2(.02f, .79f), new Vector2(.98f, .96f));
            rosterCountText = CreateRuntimeText("RosterCount", toolbar.transform, "SIGNED TALENT  /  0", 17, new Color32(45, 190, 230, 255),
                TextAnchor.MiddleLeft, new Vector2(.025f, 0), new Vector2(.65f, 1), FontStyle.Bold);
            var signButton = CreateRuntimeButton("SignWrestlerButton", toolbar.transform, "+  SIGN WRESTLER",
                new Vector2(.76f, .16f), new Vector2(.975f, .84f), new Color32(240, 190, 42, 255), new Color32(5, 9, 20, 255));
            signButton.onClick.AddListener(ShowWrestlerCreation);

            var viewport = CreateRuntimePanel("RosterTable", view.transform, new Color32(5, 11, 23, 255), new Vector2(.02f, .05f), new Vector2(.98f, .74f));
            viewport.AddComponent<RectMask2D>();
            var scroll = viewport.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 4f;
            var content = new GameObject("WrestlerCardGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(.02f, 1);
            contentRect.anchorMax = new Vector2(.98f, 1);
            contentRect.pivot = new Vector2(.5f, 1);
            contentRect.anchoredPosition = new Vector2(0, -20);
            contentRect.sizeDelta = new Vector2(0, 180);
            var grid = content.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(230, 230);
            grid.spacing = new Vector2(18, 18);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.childAlignment = TextAnchor.UpperCenter;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRect;
            wrestlerGrid = content.transform;
            emptyRosterText = CreateRuntimeText("EmptyRoster", viewport.transform,
                "NO WRESTLERS SIGNED\n\nUse SIGN WRESTLER to add the first portrait card to this universe.",
                20, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter,
                new Vector2(.08f, .12f), new Vector2(.92f, .88f), FontStyle.Bold);
            return view;
        }

        private GameObject CreateWrestlerCreationPanel(Transform workspace)
        {
            var panel = CreateRuntimePanel("WrestlerCreationPanel", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            wrestlerFormTitle = CreateRuntimeText("Title", panel.transform, "SIGN WRESTLER", 27, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.035f, .84f), new Vector2(.5f, .98f), FontStyle.Bold);
            var back = CreateRuntimeButton("BackToRosterButton", panel.transform, "BACK TO ROSTER",
                new Vector2(.80f, .86f), new Vector2(.965f, .96f), new Color32(25, 45, 65, 255), Color.white);
            back.onClick.AddListener(ShowRoster);

            wrestlerNameInput = CreateRuntimeInput("WrestlerName", panel.transform, "NAME", "Wrestler name", new Vector2(.035f, .66f), new Vector2(.63f, .82f));
            brandDropdown = CreateRuntimeDropdown("Brand", panel.transform, "BRAND", new[] { "Unassigned" }, 0, new Vector2(.035f, .46f), new Vector2(.31f, .62f));
            dispositionDropdown = CreateRuntimeDropdown("Disposition", panel.transform, "DISPOSITION", Dispositions, 0, new Vector2(.33f, .46f), new Vector2(.61f, .62f));
            genderDropdown = CreateRuntimeDropdown("Gender", panel.transform, "GENDER", Genders, 0, new Vector2(.63f, .46f), new Vector2(.965f, .62f));
            tierDropdown = CreateRuntimeDropdown("Tier", panel.transform, "TIER", Tiers, 1, new Vector2(.035f, .25f), new Vector2(.31f, .41f));
            overallInput = CreateRuntimeInput("Overall", panel.transform, "OVR  (1-100)", "75", new Vector2(.33f, .25f), new Vector2(.48f, .41f));
            overallInput.characterLimit = 3;
            overallInput.contentType = InputField.ContentType.IntegerNumber;

            wrestlerPhotoHost = CreateRuntimePanel("WrestlerPhoto", panel.transform, new Color32(5, 11, 23, 255), new Vector2(.51f, .13f), new Vector2(.76f, .41f));
            var photoButton = CreateRuntimeButton("ChoosePhoto", wrestlerPhotoHost.transform, "+  CHOOSE PHOTO", new Vector2(.06f, .06f), new Vector2(.94f, .28f), new Color32(25, 45, 65, 255), Color.white);
            photoButton.onClick.AddListener(PickWrestlerPhoto);
            wrestlerPhotoStatus = CreateRuntimeText("PhotoStatus", panel.transform, "No photo selected", 13, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter,
                new Vector2(.51f, .06f), new Vector2(.76f, .13f));
            wrestlerValidationText = CreateRuntimeText("Validation", panel.transform, string.Empty, 13, new Color32(255, 105, 105, 255), TextAnchor.MiddleLeft,
                new Vector2(.035f, .05f), new Vector2(.50f, .14f), FontStyle.Bold);
            var sign = CreateRuntimeButton("SaveWrestlerButton", panel.transform, "SIGN WRESTLER", new Vector2(.79f, .15f), new Vector2(.965f, .36f), new Color32(240, 190, 42, 255), new Color32(5, 9, 20, 255));
            wrestlerSaveLabel = sign.transform.Find("Label").GetComponent<Text>();
            sign.onClick.AddListener(SaveWrestler);
            return panel;
        }

        private void ShowTeamCreation()
        {
            SelectSection("RosterButton", "CREATE TEAM", string.Empty);
            sectionContentText.gameObject.SetActive(false); teamCreationPanel.SetActive(true);
            editingTeam = null; teamFormTitle.text = "CREATE TEAM"; teamSaveLabel.text = "CREATE TEAM";
            teamNameInput.text = string.Empty; teamBrandDropdown.value = 0; teamDispositionDropdown.value = 0;
            teamBrandDropdown.RefreshShownValue(); teamDispositionDropdown.RefreshShownValue();
            selectedTeamMemberIds.Clear(); teamValidationText.text = string.Empty; BuildMemberDropdown();
            selectedTeamPhotoPath = string.Empty; teamPhotoStatus.text = "No photo selected";
            var oldPreview = teamPhotoHost.transform.Find("PhotoPreview"); if (oldPreview != null) Destroy(oldPreview.gameObject);
        }

        private void EditTeam(TeamRecord team)
        {
            ShowTeamCreation(); editingTeam = team; teamFormTitle.text = "EDIT TEAM"; teamSaveLabel.text = "SAVE CHANGES";
            teamNameInput.text = team.name; SetDropdownValue(teamBrandDropdown, team.brand); SetDropdownValue(teamDispositionDropdown, team.disposition);
            selectedTeamMemberIds.Clear(); selectedTeamMemberIds.AddRange(team.memberIds); BuildMemberDropdown();
            selectedTeamPhotoPath = string.Empty;
            teamPhotoStatus.text = string.IsNullOrEmpty(team.photoPath) ? "No photo selected" : System.IO.Path.GetFileName(team.photoPath);
            var oldPreview = teamPhotoHost.transform.Find("PhotoPreview"); if (oldPreview != null) Destroy(oldPreview.gameObject);
            var texture = UniverseImageStorage.LoadTexture(team.photoPath);
            if (texture != null) { loadedTextures.Add(texture); SetRuntimePhoto(teamPhotoHost.transform, "PhotoPreview", texture, new Vector2(.06f, .31f), new Vector2(.94f, .94f)); }
        }

        private void PickTeamPhoto()
        {
            string path; if (!WindowsImageFilePicker.TryPickImage(out path)) return;
            var texture = UniverseImageStorage.LoadTexture(path);
            if (texture == null) { teamValidationText.text = "Unity could not decode that image."; return; }
            loadedTextures.Add(texture); selectedTeamPhotoPath = path; teamPhotoStatus.text = System.IO.Path.GetFileName(path);
            SetRuntimePhoto(teamPhotoHost.transform, "PhotoPreview", texture, new Vector2(.06f, .31f), new Vector2(.94f, .94f));
        }

        private void ToggleMemberDropdown()
        {
            memberDropdownMenu.SetActive(!memberDropdownMenu.activeSelf);
            if (memberDropdownMenu.activeSelf) memberDropdownMenu.transform.SetAsLastSibling();
        }

        private void BuildMemberDropdown()
        {
            availableTeamMembers = repository.LoadWrestlers(ActiveUniverseSession.UniverseId);
            for (var index = memberDropdownMenu.transform.childCount - 1; index >= 0; index--) Destroy(memberDropdownMenu.transform.GetChild(index).gameObject);
            var count = Mathf.Max(1, availableTeamMembers.Count);
            var rowHeight = 1f / count;
            for (var index = 0; index < availableTeamMembers.Count; index++)
            {
                var wrestler = availableTeamMembers[index];
                var top = 1f - index * rowHeight; var bottom = top - rowHeight;
                var selected = selectedTeamMemberIds.Contains(wrestler.id);
                var row = CreateRuntimeButton("Member_" + wrestler.id, memberDropdownMenu.transform,
                    (selected ? "✓  " : "     ") + wrestler.name, new Vector2(.02f, bottom), new Vector2(.98f, top),
                    selected ? new Color32(25, 65, 82, 255) : new Color32(9, 15, 29, 255), Color.white);
                row.onClick.AddListener(() => ToggleTeamMember(wrestler.id));
            }
            if (availableTeamMembers.Count == 0)
                CreateRuntimeText("NoMembers", memberDropdownMenu.transform, "No wrestlers available", 14, new Color32(142, 160, 181, 255),
                    TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
            UpdateMemberCaption();
        }

        private void ToggleTeamMember(string wrestlerId)
        {
            if (selectedTeamMemberIds.Contains(wrestlerId)) selectedTeamMemberIds.Remove(wrestlerId);
            else if (selectedTeamMemberIds.Count < 5) selectedTeamMemberIds.Add(wrestlerId);
            else { teamValidationText.text = "A team can have no more than five members."; return; }
            BuildMemberDropdown(); memberDropdownMenu.SetActive(true);
        }

        private void UpdateMemberCaption()
        {
            var names = new List<string>();
            foreach (var id in selectedTeamMemberIds)
            {
                var wrestler = availableTeamMembers.Find(item => item.id == id);
                if (wrestler != null) names.Add(wrestler.name);
            }
            memberSelectionCaption.text = names.Count == 0 ? "Select roster members" : string.Join(", ", names.ToArray());
        }

        private void SaveTeam()
        {
            if (string.IsNullOrWhiteSpace(teamNameInput.text)) { teamValidationText.text = "Team name is required."; return; }
            if (selectedTeamMemberIds.Count == 0) { teamValidationText.text = "Select at least one team member."; return; }
            try
            {
                var team = new TeamRecord
                {
                    id = editingTeam == null ? Guid.NewGuid().ToString("N") : editingTeam.id,
                    universeId = ActiveUniverseSession.UniverseId, name = teamNameInput.text.Trim(),
                    brand = teamBrandDropdown.options[teamBrandDropdown.value].text,
                    disposition = teamDispositionDropdown.options[teamDispositionDropdown.value].text,
                    createdUtc = editingTeam == null ? DateTime.UtcNow.ToString("O") : editingTeam.createdUtc,
                    memberIds = new List<string>(selectedTeamMemberIds),
                    photoPath = editingTeam == null ? string.Empty : editingTeam.photoPath
                };
                if (!string.IsNullOrEmpty(selectedTeamPhotoPath))
                    team.photoPath = UniverseImageStorage.Import(team.universeId, selectedTeamPhotoPath, "team_" + team.id);
                repository.SaveTeam(team); memberDropdownMenu.SetActive(false); ShowTeams();
            }
            catch (Exception exception) { Debug.LogException(exception); teamValidationText.text = "The team could not be saved. Check the Console."; }
        }

        private void RefreshTeamCards()
        {
            if (repository == null || teamGrid == null) return;
            for (var index = teamGrid.childCount - 1; index >= 0; index--) Destroy(teamGrid.GetChild(index).gameObject);
            var teams = repository.LoadTeams(ActiveUniverseSession.UniverseId);
            teamCountText.text = "TEAMS  /  " + teams.Count; emptyTeamsText.gameObject.SetActive(teams.Count == 0);
            foreach (var team in teams)
            {
                var card = CreateRuntimePanel("Team_" + team.id, teamGrid, new Color32(14, 23, 40, 255), Vector2.zero, Vector2.one);
                var texture = UniverseImageStorage.LoadTexture(team.photoPath);
                if (texture != null) { loadedTextures.Add(texture); SetRuntimePhoto(card.transform, "TeamPhoto", texture, new Vector2(.04f, .32f), new Vector2(.34f, .94f)); }
                var textMin = texture != null ? .38f : .06f;
                CreateRuntimeText("Name", card.transform, team.name.ToUpperInvariant(), 20, Color.white, TextAnchor.MiddleLeft,
                    new Vector2(textMin, .65f), new Vector2(.94f, .92f), FontStyle.Bold);
                CreateRuntimeText("Members", card.transform, string.Join("  /  ", team.memberNames.ToArray()), 14, new Color32(142, 160, 181, 255),
                    TextAnchor.MiddleLeft, new Vector2(textMin, .31f), new Vector2(.94f, .65f));
                CreateRuntimeText("Disposition", card.transform, team.disposition.ToUpperInvariant(), 12, new Color32(45, 190, 230, 255),
                    TextAnchor.MiddleLeft, new Vector2(textMin, .20f), new Vector2(.94f, .32f), FontStyle.Bold);
                var edit = CreateRuntimeButton("EditButton", card.transform, "EDIT", new Vector2(.06f, .035f), new Vector2(.94f, .18f),
                    new Color32(25, 45, 65, 255), Color.white); edit.onClick.AddListener(() => EditTeam(team));
            }
        }

        private void ResetWrestlerForm()
        {
            editingWrestler = null;
            wrestlerFormTitle.text = "SIGN WRESTLER";
            wrestlerSaveLabel.text = "SIGN WRESTLER";
            wrestlerNameInput.text = string.Empty;
            overallInput.text = "75";
            brandDropdown.value = 0;
            dispositionDropdown.value = 0;
            genderDropdown.value = 0;
            tierDropdown.value = 1;
            brandDropdown.RefreshShownValue();
            dispositionDropdown.RefreshShownValue();
            genderDropdown.RefreshShownValue();
            tierDropdown.RefreshShownValue();
            wrestlerValidationText.text = string.Empty;
            wrestlerPhotoStatus.text = "No photo selected";
            selectedWrestlerPhotoPath = string.Empty;
            var oldPreview = wrestlerPhotoHost.transform.Find("PhotoPreview");
            if (oldPreview != null) Destroy(oldPreview.gameObject);
        }

        private void EditWrestler(WrestlerRecord wrestler)
        {
            SelectSection("RosterButton", "EDIT WRESTLER", string.Empty);
            sectionContentText.gameObject.SetActive(false);
            wrestlerCreationPanel.SetActive(true);
            editingWrestler = wrestler;
            wrestlerFormTitle.text = "EDIT WRESTLER";
            wrestlerSaveLabel.text = "SAVE CHANGES";
            wrestlerNameInput.text = wrestler.name;
            overallInput.text = wrestler.overall.ToString();
            SetDropdownValue(brandDropdown, wrestler.brand);
            SetDropdownValue(dispositionDropdown, wrestler.disposition);
            SetDropdownValue(genderDropdown, wrestler.gender);
            SetDropdownValue(tierDropdown, wrestler.tier);
            wrestlerValidationText.text = string.Empty;
            selectedWrestlerPhotoPath = string.Empty;
            wrestlerPhotoStatus.text = string.IsNullOrEmpty(wrestler.photoPath) ? "No photo selected" : System.IO.Path.GetFileName(wrestler.photoPath);
            var oldPreview = wrestlerPhotoHost.transform.Find("PhotoPreview");
            if (oldPreview != null) Destroy(oldPreview.gameObject);
            var texture = UniverseImageStorage.LoadTexture(wrestler.photoPath);
            if (texture != null)
            {
                loadedTextures.Add(texture);
                SetRuntimePhoto(wrestlerPhotoHost.transform, "PhotoPreview", texture, new Vector2(.06f, .31f), new Vector2(.94f, .94f));
            }
        }

        private static void SetDropdownValue(Dropdown dropdown, string value)
        {
            var index = dropdown.options.FindIndex(option => option.text == value);
            dropdown.value = index < 0 ? 0 : index;
            dropdown.RefreshShownValue();
        }

        private void PickWrestlerPhoto()
        {
            string path;
            if (!WindowsImageFilePicker.TryPickImage(out path)) return;
            var texture = UniverseImageStorage.LoadTexture(path);
            if (texture == null)
            {
                wrestlerValidationText.text = "Unity could not decode that image.";
                return;
            }
            loadedTextures.Add(texture);
            selectedWrestlerPhotoPath = path;
            wrestlerPhotoStatus.text = System.IO.Path.GetFileName(path);
            SetRuntimePhoto(wrestlerPhotoHost.transform, "PhotoPreview", texture, new Vector2(.06f, .31f), new Vector2(.94f, .94f));
        }

        private void SaveWrestler()
        {
            int overall;
            if (string.IsNullOrWhiteSpace(wrestlerNameInput.text))
            {
                wrestlerValidationText.text = "Name is required.";
                return;
            }
            if (!int.TryParse(overallInput.text, out overall) || overall < 1 || overall > 100)
            {
                wrestlerValidationText.text = "OVR must be between 1 and 100.";
                return;
            }

            try
            {
                var isEditing = editingWrestler != null;
                var wrestler = new WrestlerRecord
                {
                    id = isEditing ? editingWrestler.id : Guid.NewGuid().ToString("N"), universeId = ActiveUniverseSession.UniverseId,
                    name = wrestlerNameInput.text.Trim(), brand = brandDropdown.options[brandDropdown.value].text,
                    disposition = dispositionDropdown.options[dispositionDropdown.value].text,
                    gender = genderDropdown.options[genderDropdown.value].text, tier = tierDropdown.options[tierDropdown.value].text,
                    overall = overall, createdUtc = isEditing ? editingWrestler.createdUtc : DateTime.UtcNow.ToString("O"),
                    photoPath = isEditing ? editingWrestler.photoPath : string.Empty
                };
                if (!string.IsNullOrEmpty(selectedWrestlerPhotoPath))
                    wrestler.photoPath = UniverseImageStorage.Import(wrestler.universeId, selectedWrestlerPhotoPath, "wrestler_" + wrestler.id);
                repository.SaveWrestler(wrestler);
                ShowRoster();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                wrestlerValidationText.text = "The wrestler could not be saved. Check the Console.";
            }
        }

        private void RefreshRosterCards()
        {
            if (repository == null || wrestlerGrid == null) return;
            for (var index = wrestlerGrid.childCount - 1; index >= 0; index--)
                Destroy(wrestlerGrid.GetChild(index).gameObject);

            var wrestlers = repository.LoadWrestlers(ActiveUniverseSession.UniverseId);
            rosterCountText.text = "SIGNED TALENT  /  " + wrestlers.Count;
            emptyRosterText.gameObject.SetActive(wrestlers.Count == 0);
            foreach (var wrestler in wrestlers) CreateWrestlerCard(wrestlerGrid, wrestler);
        }

        private void CreateWrestlerCard(Transform parent, WrestlerRecord wrestler)
        {
            var card = CreateRuntimePanel("Wrestler_" + wrestler.id, parent, new Color32(14, 23, 40, 255), Vector2.zero, Vector2.one);
            var texture = UniverseImageStorage.LoadTexture(wrestler.photoPath);
            if (texture != null)
            {
                loadedTextures.Add(texture);
                SetRuntimePhoto(card.transform, "Portrait", texture, new Vector2(.05f, .38f), new Vector2(.95f, .96f));
            }
            CreateRuntimeText("Name", card.transform, wrestler.name.ToUpperInvariant(), 16, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.06f, .24f), new Vector2(.72f, .38f), FontStyle.Bold);
            CreateRuntimeText("Overall", card.transform, wrestler.overall.ToString(), 22, new Color32(240, 190, 42, 255), TextAnchor.MiddleRight,
                new Vector2(.72f, .24f), new Vector2(.94f, .38f), FontStyle.Bold);
            CreateRuntimeText("Details", card.transform, wrestler.disposition.ToUpperInvariant() + "  /  " + wrestler.tier.ToUpperInvariant(), 11,
                new Color32(142, 160, 181, 255), TextAnchor.MiddleLeft, new Vector2(.06f, .14f), new Vector2(.94f, .24f));
            var edit = CreateRuntimeButton("EditButton", card.transform, "EDIT", new Vector2(.06f, .025f), new Vector2(.94f, .13f),
                new Color32(25, 45, 65, 255), Color.white);
            edit.onClick.AddListener(() => EditWrestler(wrestler));
        }

        private static void SetRuntimePhoto(Transform parent, string name, Texture2D texture, Vector2 min, Vector2 max)
        {
            var old = parent.Find(name);
            if (old != null) Destroy(old.gameObject);
            var container = new GameObject(name, typeof(RectTransform));
            container.transform.SetParent(parent, false);
            SetRuntimeRect(container.GetComponent<RectTransform>(), min, max);
            var imageObject = new GameObject("Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(AspectRatioFitter));
            imageObject.transform.SetParent(container.transform, false);
            var rect = imageObject.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.sizeDelta = Vector2.zero;
            var image = imageObject.GetComponent<RawImage>(); image.texture = texture; image.raycastTarget = false;
            var fitter = imageObject.GetComponent<AspectRatioFitter>(); fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent; fitter.aspectRatio = (float)texture.width / texture.height;
        }

        private static InputField CreateRuntimeInput(string name, Transform parent, string label, string placeholder, Vector2 min, Vector2 max)
        {
            var root = CreateRuntimePanel(name, parent, new Color32(5, 11, 23, 255), min, max);
            CreateRuntimeText("FieldLabel", root.transform, label, 12, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                new Vector2(.04f, .58f), new Vector2(.96f, .94f), FontStyle.Bold);
            var value = CreateRuntimeText("Text", root.transform, string.Empty, 18, Color.white, TextAnchor.MiddleLeft, new Vector2(.04f, .05f), new Vector2(.96f, .62f));
            var hint = CreateRuntimeText("Placeholder", root.transform, placeholder, 18, new Color32(90, 105, 125, 255), TextAnchor.MiddleLeft, new Vector2(.04f, .05f), new Vector2(.96f, .62f));
            var input = root.AddComponent<InputField>(); input.textComponent = value; input.placeholder = hint; return input;
        }

        private static Dropdown CreateRuntimeDropdown(string name, Transform parent, string fieldLabel,
            string[] choices, int initialIndex, Vector2 min, Vector2 max)
        {
            var root = CreateRuntimePanel(name, parent, new Color32(5, 11, 23, 255), min, max);
            var dropdown = root.AddComponent<Dropdown>();
            dropdown.targetGraphic = root.GetComponent<Image>();
            CreateRuntimeText("FieldLabel", root.transform, fieldLabel, 12, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                new Vector2(.05f, .58f), new Vector2(.95f, .94f), FontStyle.Bold);
            var caption = CreateRuntimeText("Value", root.transform, string.Empty, 18, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.05f, .06f), new Vector2(.85f, .62f));
            CreateRuntimeText("Arrow", root.transform, "▼", 13, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter,
                new Vector2(.86f, .08f), new Vector2(.96f, .62f));

            var template = CreateRuntimePanel("Template", root.transform, new Color32(9, 15, 29, 255), new Vector2(0, 0), new Vector2(1, 0));
            var templateRect = template.GetComponent<RectTransform>();
            templateRect.pivot = new Vector2(.5f, 1);
            templateRect.anchoredPosition = new Vector2(0, -2);
            templateRect.sizeDelta = new Vector2(0, 150);
            var scroll = template.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var viewport = CreateRuntimePanel("Viewport", template.transform, new Color32(9, 15, 29, 255), new Vector2(.02f, .02f), new Vector2(.98f, .98f));
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1); contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(.5f, 1); contentRect.anchoredPosition = Vector2.zero; contentRect.sizeDelta = new Vector2(0, 32);

            var item = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
            item.transform.SetParent(content.transform, false);
            var itemRect = item.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0, .5f); itemRect.anchorMax = new Vector2(1, .5f); itemRect.sizeDelta = new Vector2(0, 32);
            var itemBackground = CreateRuntimePanel("Item Background", item.transform, new Color32(14, 23, 40, 255), Vector2.zero, Vector2.one);
            var checkmark = CreateRuntimePanel("Item Checkmark", item.transform, new Color32(45, 190, 230, 255), new Vector2(.03f, .25f), new Vector2(.07f, .75f));
            var itemLabel = CreateRuntimeText("Item Label", item.transform, "Option", 15, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.10f, 0), new Vector2(.96f, 1));
            var toggle = item.GetComponent<Toggle>(); toggle.targetGraphic = itemBackground.GetComponent<Image>(); toggle.graphic = checkmark.GetComponent<Image>();

            scroll.viewport = viewport.GetComponent<RectTransform>(); scroll.content = contentRect;
            dropdown.template = templateRect; dropdown.captionText = caption; dropdown.itemText = itemLabel;
            dropdown.ClearOptions(); dropdown.AddOptions(new List<string>(choices));
            dropdown.value = Mathf.Clamp(initialIndex, 0, choices.Length - 1); dropdown.RefreshShownValue();
            template.SetActive(false);
            return dropdown;
        }

        private static GameObject CreateRuntimePanel(string name, Transform parent, Color color, Vector2 min, Vector2 max)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            SetRuntimeRect(panel.GetComponent<RectTransform>(), min, max);
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static Button CreateRuntimeButton(string name, Transform parent, string label, Vector2 min, Vector2 max, Color background, Color textColor)
        {
            var buttonObject = CreateRuntimePanel(name, parent, background, min, max);
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonObject.GetComponent<Image>();
            CreateRuntimeText("Label", buttonObject.transform, label, 15, textColor, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, FontStyle.Bold);
            return button;
        }

        private static Text CreateRuntimeText(string name, Transform parent, string value, int size, Color color,
            TextAnchor alignment, Vector2 min, Vector2 max, FontStyle style = FontStyle.Normal)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            SetRuntimeRect(textObject.GetComponent<RectTransform>(), min, max);
            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.text = value;
            return text;
        }

        private void EnsureNavigationBar()
        {
            var root = promotionNameText != null ? promotionNameText.transform.root : transform.root;
            var background = root.Find("Background");
            if (background == null) return;
            if (sectionTitleText == null)
            {
                var section = background.Find("Section");
                if (section != null) sectionTitleText = section.GetComponent<Text>();
            }
            if (sectionContentText == null)
            {
                var placeholder = background.Find("FeatureWorkspace/Placeholder");
                if (placeholder != null) sectionContentText = placeholder.GetComponent<Text>();
            }
            var existingNavigation = background.Find("WorkspaceNavigation");
            if (existingNavigation != null)
            {
                EnsureRosterDropdown(existingNavigation);
                return;
            }

            var title = background.Find("Title");
            var owner = background.Find("Owner");
            var startDate = background.Find("StartDate");
            var workspace = background.Find("FeatureWorkspace");
            if (sectionTitleText != null) SetRuntimeRect(sectionTitleText.rectTransform, new Vector2(.04f, .68f), new Vector2(.6f, .76f));
            if (title != null) SetRuntimeRect(title.GetComponent<RectTransform>(), new Vector2(.04f, .55f), new Vector2(.85f, .68f));
            if (owner != null) SetRuntimeRect(owner.GetComponent<RectTransform>(), new Vector2(.04f, .47f), new Vector2(.46f, .55f));
            if (startDate != null) SetRuntimeRect(startDate.GetComponent<RectTransform>(), new Vector2(.48f, .47f), new Vector2(.9f, .55f));
            if (workspace != null) SetRuntimeRect(workspace.GetComponent<RectTransform>(), new Vector2(.04f, .07f), new Vector2(.96f, .43f));

            var bar = new GameObject("WorkspaceNavigation", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bar.transform.SetParent(background, false);
            SetRuntimeRect(bar.GetComponent<RectTransform>(), new Vector2(0, .79f), new Vector2(1, .88f));
            bar.GetComponent<Image>().color = inactiveNavigation;
            CreateRuntimeNavigationButton(bar.transform, "MyUniverseButton", "MY UNIVERSE", .18f, .31f, ShowMyUniverse);
            CreateRuntimeNavigationButton(bar.transform, "RosterButton", "ROSTER", .31f, .43f, ShowRoster);
            CreateRuntimeNavigationButton(bar.transform, "BookingButton", "BOOKING", .43f, .55f, ShowBooking);
            CreateRuntimeNavigationButton(bar.transform, "ResultsButton", "RESULTS", .55f, .67f, ShowResults);
            CreateRuntimeNavigationButton(bar.transform, "AnalyticsButton", "ANALYTICS", .67f, .80f, ShowAnalytics);
            EnsureRosterDropdown(bar.transform);
        }

        private void EnsureRosterDropdown(Transform navigation)
        {
            navigation.SetAsLastSibling();
            var rosterButton = navigation.Find("RosterButton");
            if (rosterButton == null || rosterButton.Find("RosterDropdown") != null) return;

            var menu = CreateRuntimePanel("RosterDropdown", rosterButton, new Color32(5, 9, 20, 255),
                new Vector2(0, -1.55f), new Vector2(1, 0));
            menu.transform.SetAsLastSibling();
            var rosterItem = CreateRuntimeButton("RosterMenuItem", menu.transform, "ROSTER",
                new Vector2(0, .52f), new Vector2(1, .96f), new Color32(9, 15, 29, 255), Color.white);
            rosterItem.onClick.AddListener(ShowRoster);
            var teamsItem = CreateRuntimeButton("TeamsMenuItem", menu.transform, "TEAMS",
                new Vector2(0, .04f), new Vector2(1, .48f), new Color32(9, 15, 29, 255), Color.white);
            teamsItem.onClick.AddListener(ShowTeams);

            var hover = rosterButton.gameObject.AddComponent<NavigationHoverDropdown>();
            hover.Configure(menu);
            rosterButton.gameObject.AddComponent<NavigationHoverRelay>().Configure(hover);
            menu.AddComponent<NavigationHoverRelay>().Configure(hover);
        }

        private static void CreateRuntimeNavigationButton(Transform parent, string name, string labelText,
            float minX, float maxX, Action action)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            SetRuntimeRect(buttonObject.GetComponent<RectTransform>(), new Vector2(minX, 0), new Vector2(maxX, 1));
            buttonObject.GetComponent<Image>().color = new Color32(2, 4, 9, 255);
            buttonObject.GetComponent<Button>().onClick.AddListener(() => action());

            var label = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            label.transform.SetParent(buttonObject.transform, false);
            SetRuntimeRect(label.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
            var text = label.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = labelText;

            var underline = new GameObject("ActiveUnderline", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            underline.transform.SetParent(buttonObject.transform, false);
            SetRuntimeRect(underline.GetComponent<RectTransform>(), new Vector2(.15f, 0), new Vector2(.85f, .045f));
            underline.GetComponent<Image>().color = new Color32(45, 190, 230, 255);
            underline.SetActive(name == "MyUniverseButton");
        }

        private static void SetRuntimeRect(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void OnDestroy()
        {
            foreach (var texture in loadedTextures)
                if (texture != null) Destroy(texture);
            loadedTextures.Clear();
        }

        public void ReturnToLandingPage()
        {
            ActiveUniverseSession.Clear();
            SceneManager.LoadScene("LandingPage");
        }
    }
}
