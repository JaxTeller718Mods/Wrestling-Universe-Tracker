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
        private GameObject titlesView;
        private GameObject titleCreationPanel;
        private Transform titleGrid;
        private Text emptyTitlesText;
        private Text titleCountText;
        private InputField titleNameInput;
        private Dropdown titleBrandDropdown;
        private Dropdown titleHolderDropdown;
        private GameObject titleImageHost;
        private Text titleImageStatus;
        private Text titleValidationText;
        private string selectedTitleImagePath;
        private List<WrestlerRecord> titleHolderOptions = new List<WrestlerRecord>();
        private Dropdown titleDivisionDropdown;
        private Text titleFormTitle;
        private Text titleSaveLabel;
        private TitleRecord editingTitle;
        private static readonly string[] TitleDivisions = { "Men's", "Women's", "Tag Team" };
        private GameObject titleHistoryPanel;
        private Text titleHistoryNameText;
        private GameObject brandsView;
        private GameObject showsView;
        private GameObject specialsView;
        private GameObject locationsView;
        private GameObject locationCreationPanel;
        private Transform locationGrid;
        private Text emptyLocationsText;
        private Text locationCountText;
        private InputField venueNameInput;
        private InputField venueLocationInput;
        private InputField venueCapacityInput;
        private Text locationValidationText;
        private Text locationFormTitle;
        private Text locationSaveLabel;
        private LocationRecord editingLocation;
        private GameObject brandCreationPanel;
        private GameObject brandInfoPanel;
        private Transform brandGrid;
        private Text emptyBrandsText;
        private Text brandCountText;
        private InputField brandNameInput;
        private InputField brandColorInput;
        private GameObject brandImageHost;
        private Text brandImageStatus;
        private Text brandValidationText;
        private Text brandFormTitle;
        private Text brandSaveLabel;
        private Text brandInfoTitle;
        private Text brandInfoRoster;
        private BrandRecord editingBrand;
        private string selectedBrandImagePath;
        private GameObject tvShowCreationPanel;
        private Transform tvShowGrid;
        private Text emptyTvShowsText;
        private Text tvShowCountText;
        private InputField tvShowNameInput;
        private Dropdown tvShowFrequencyDropdown;
        private Dropdown tvShowDayDropdown;
        private GameObject tvShowBrandMenu;
        private Text tvShowBrandCaption;
        private GameObject tvShowImageHost;
        private Text tvShowImageStatus;
        private Text tvShowValidationText;
        private Text tvShowFormTitle;
        private Text tvShowSaveLabel;
        private TvShowRecord editingTvShow;
        private string selectedTvShowImagePath;
        private readonly List<string> selectedTvShowBrandIds = new List<string>();
        private List<BrandRecord> availableTvShowBrands = new List<BrandRecord>();
        private GameObject specialCreationPanel;
        private Transform specialGrid;
        private Text emptySpecialsText;
        private Text specialCountText;
        private InputField specialNameInput;
        private Dropdown specialMonthDropdown;
        private Dropdown specialWeekDropdown;
        private Dropdown specialDayDropdown;
        private GameObject specialBrandMenu;
        private Text specialBrandCaption;
        private GameObject specialImageHost;
        private Text specialImageStatus;
        private Text specialValidationText;
        private Text specialFormTitle;
        private Text specialSaveLabel;
        private SpecialRecord editingSpecial;
        private string selectedSpecialImagePath;
        private readonly List<string> selectedSpecialBrandIds = new List<string>();
        private List<BrandRecord> availableSpecialBrands = new List<BrandRecord>();
        private GameObject calendarView;
        private Dropdown calendarMonthDropdown;
        private InputField calendarYearInput;
        private Text calendarHeadingText;
        private int calendarYear = DateTime.Now.Year;
        private readonly List<Transform> calendarCells = new List<Transform>();
        private GameObject showBookingPanel;
        private Text bookingShowNameText;
        private Text bookingScheduleText;
        private GameObject matchBookingHeader;
        private GameObject matchBookingBody;
        private LayoutElement matchBookingBodyLayout;
        private Text matchBookingArrow;
        private GameObject segmentBookingHeader;
        private GameObject segmentBookingBody;
        private Text segmentBookingArrow;
        private RectTransform bookingAccordionContent;
        private ScrollRect bookingAccordionScroll;
        private bool matchBookingExpanded;
        private bool segmentBookingExpanded;
        private Dropdown matchStipulationDropdown;
        private Dropdown matchFormatDropdown;
        private Dropdown matchTitleDropdown;
        private List<TitleRecord> matchTitleOptions = new List<TitleRecord>();
        private Dropdown matchGenderDropdown;
        private GameObject matchStagesGroup;
        private Dropdown matchStageOneDropdown;
        private Dropdown matchStageTwoDropdown;
        private Dropdown matchStageThreeDropdown;
        private GameObject matchParticipantMenu;
        private Button matchParticipantSelector;
        private Text matchParticipantCaption;
        private readonly List<string> selectedMatchParticipantIds = new List<string>();
        private List<WrestlerRecord> availableMatchParticipants = new List<WrestlerRecord>();
        private List<string> activeBookingBrandNames = new List<string>();
        private Button addMatchToCardButton;
        private Text addMatchToCardLabel;
        private Text matchBookingValidationText;
        private Transform bookedMatchCardList;
        private LayoutElement bookedMatchCardListLayout;
        private BookedMatchRecord editingBookedMatch;
        private readonly HashSet<string> expandedBookedMatchIds = new HashSet<string>();
        private static readonly string[] ShowFrequencies = { "Weekly", "Bi-Weekly", "Monthly", "Special" };
        private static readonly string[] WeekDays = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        private static readonly string[] Months = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        private static readonly string[] MonthWeeks = { "Week 1", "Week 2", "Week 3", "Week 4" };
        private static readonly string[] MatchStipulations = { "Normal", "Tag Team", "Extreme Rules", "Falls Count Anywhere", "Hell in a Cell", "Steel Cage", "Table Match", "Ladder Match", "Tables, Ladders, and Chairs", "Submission Match", "Last Man Standing", "No Holds Barred", "Iron Man Match", "Casket Match", "Ambulance Match", "Dumpster Match", "I Quit Match", "Inferno Match", "Elimination Chamber", "War Games", "Underground Match", "3 Stages of Hell", "Backstage Brawl", "Gauntlet Match", "Money in the Bank" };
        private static readonly string[] MatchFormats = { "One on One", "Triple Threat", "Fatal 4-Way", "5-Way", "6-Way", "8-Way" };
        private static readonly string[] TagTeamMatchFormats = {
            "Two on Two", "Two on Two - Mixed Tag", "Two on Two - Tornado Tag", "Three on Three", "Three on Three - Tornado Tag",
            "Four on Four", "Triple Threat Tornado Tag", "4-Way Tornado Tag", "Handicap - One on Two",
            "Handicap - One on Two Tornado Tag", "Handicap - One on Three", "Handicap - Two on Three"
        };
        private static readonly string[] ExtremeRulesMatchFormats = { "One on One", "Triple Threat", "Fatal 4-Way", "5-Way", "Two on Two" };
        private static readonly string[] FallsCountAnywhereMatchFormats = { "One on One", "Triple Threat", "Fatal 4-Way" };
        private static readonly string[] SteelCageMatchFormats = { "One on One", "Triple Threat", "Fatal 4-Way", "Two on Two" };
        private static readonly string[] HellInACellMatchFormats = {
            "One on One", "Triple Threat", "Fatal 4-Way", "5-Way", "6-Way", "Two on Two", "Three on Three", "Triple Threat Tornado Tag"
        };
        private static readonly string[] TableMatchFormats = {
            "One on One", "Triple Threat", "Fatal 4-Way", "5-Way", "Two on Two", "Three on Three", "Triple Threat Tornado Tag"
        };
        private static readonly string[] LadderMatchFormats = {
            "One on One", "Triple Threat", "Fatal 4-Way", "5-Way", "6-Way", "8-Way", "Two on Two", "Three on Three",
            "Four on Four", "Triple Threat Tornado Tag"
        };
        private static readonly string[] TablesLaddersChairsMatchFormats = {
            "One on One", "Triple Threat", "Fatal 4-Way", "5-Way", "Two on Two", "Three on Three", "Triple Threat Tornado Tag"
        };
        private static readonly string[] OneOnOneOnlyMatchFormats = { "One on One" };
        private static readonly string[] SixWayOnlyMatchFormats = { "6-Way" };
        private static readonly string[] WarGamesMatchFormats = { "Three on Three", "Four on Four" };
        private static readonly string[] BackstageBrawlMatchFormats = {
            "One on One", "Triple Threat", "Fatal 4-Way", "6-Way", "Two on Two", "Handicap - One on Two"
        };
        private static readonly string[] GauntletMatchFormats = {
            "4 Entrants", "5 Entrants", "6 Entrants", "8 Entrants", "10 Entrants", "20 Entrants", "30 Entrants"
        };
        private static readonly string[] MoneyInTheBankMatchFormats = {
            "4-Way Ladder", "5-Way Ladder", "6 Entrants", "8-Way Ladder"
        };
        private static readonly string[] ThreeStagesOfHellOptions = {
            "Normal", "Extreme Rules", "Falls Count Anywhere", "Hell in a Cell", "Steel Cage", "Submission Match", "I Quit",
            "Iron Man", "Last Man Standing", "No Holds Barred", "Casket Match", "Ambulance Match", "Inferno Match",
            "Dumpster Match", "Underground Match", "Bloodline Rules"
        };
        private static readonly string[] MatchGenderFilters = { "Both Genders", "Male", "Female", "Neutral" };

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
                InitializeCalendarDate(universe.startDate);
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

        public void ShowTitles()
        {
            SelectSection("RosterButton", "TITLES", string.Empty);
            sectionContentText.gameObject.SetActive(false); titlesView.SetActive(true); RefreshTitleCards();
        }

        public void ShowBrands()
        {
            ShowUniverseSetupView("BRANDS", brandsView);
            RefreshBrandCards();
        }
        public void ShowTvShows()
        {
            ShowUniverseSetupView("TV SHOWS", showsView);
            RefreshTvShowCards();
        }
        public void ShowSpecials()
        {
            ShowUniverseSetupView("SPECIALS", specialsView);
            RefreshSpecialCards();
        }
        public void ShowLocations()
        {
            ShowUniverseSetupView("LOCATIONS", locationsView);
            RefreshLocationCards();
        }

        private void ShowUniverseSetupView(string title, GameObject view)
        {
            SelectSection("MyUniverseButton", title, string.Empty);
            sectionContentText.gameObject.SetActive(false);
            view.SetActive(true);
        }

        public void ShowBooking() => SelectSection("BookingButton", "BOOKING",
            "BOOKING CENTER\n\nShows, events, matches, and segments will be created here.");

        public void ShowCalendar()
        {
            SelectSection("BookingButton", "CALENDAR", string.Empty);
            sectionContentText.gameObject.SetActive(false);
            calendarView.SetActive(true);
            RefreshCalendar();
        }

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
            if (titlesView != null) titlesView.SetActive(false);
            if (titleCreationPanel != null) titleCreationPanel.SetActive(false);
            if (titleHistoryPanel != null) titleHistoryPanel.SetActive(false);
            if (brandsView != null) brandsView.SetActive(false);
            if (showsView != null) showsView.SetActive(false);
            if (specialsView != null) specialsView.SetActive(false);
            if (locationsView != null) locationsView.SetActive(false);
            if (locationCreationPanel != null) locationCreationPanel.SetActive(false);
            if (brandCreationPanel != null) brandCreationPanel.SetActive(false);
            if (brandInfoPanel != null) brandInfoPanel.SetActive(false);
            if (tvShowCreationPanel != null) tvShowCreationPanel.SetActive(false);
            if (specialCreationPanel != null) specialCreationPanel.SetActive(false);
            if (calendarView != null) calendarView.SetActive(false);
            if (showBookingPanel != null) showBookingPanel.SetActive(false);
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
            titlesView = CreateTitlesView(workspace);
            titleCreationPanel = CreateTitleCreationPanel(workspace);
            titleHistoryPanel = CreateTitleHistoryPanel(workspace);
            brandsView = CreateBrandsView(workspace);
            brandCreationPanel = CreateBrandCreationPanel(workspace);
            brandInfoPanel = CreateBrandInfoPanel(workspace);
            showsView = CreateTvShowsView(workspace);
            tvShowCreationPanel = CreateTvShowCreationPanel(workspace);
            specialsView = CreateSpecialsView(workspace);
            specialCreationPanel = CreateSpecialCreationPanel(workspace);
            calendarView = CreateCalendarView(workspace);
            showBookingPanel = CreateShowBookingPanel(workspace);
            locationsView = CreateLocationsView(workspace);
            locationCreationPanel = CreateLocationCreationPanel(workspace);
            rosterView.SetActive(false);
            wrestlerCreationPanel.SetActive(false);
            teamsView.SetActive(false);
            teamCreationPanel.SetActive(false);
            titlesView.SetActive(false); titleCreationPanel.SetActive(false);
            titleHistoryPanel.SetActive(false);
            brandsView.SetActive(false); showsView.SetActive(false); specialsView.SetActive(false); locationsView.SetActive(false);
            locationCreationPanel.SetActive(false);
            brandCreationPanel.SetActive(false); brandInfoPanel.SetActive(false);
            tvShowCreationPanel.SetActive(false);
            specialCreationPanel.SetActive(false);
            calendarView.SetActive(false);
            showBookingPanel.SetActive(false);
        }

        private GameObject CreateUniverseSetupView(Transform workspace, string objectName, string heading, string action, string emptyMessage)
        {
            var view = CreateRuntimePanel(objectName, workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            var toolbar = CreateRuntimePanel("Toolbar", view.transform, new Color32(12, 21, 37, 255), new Vector2(.02f, .79f), new Vector2(.98f, .96f));
            CreateRuntimeText("Heading", toolbar.transform, heading, 18, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                new Vector2(.025f, 0), new Vector2(.65f, 1), FontStyle.Bold);
            CreateRuntimeButton("CreateButton", toolbar.transform, action, new Vector2(.76f, .16f), new Vector2(.975f, .84f),
                new Color32(240, 190, 42, 255), new Color32(5, 9, 20, 255));
            var table = CreateRuntimePanel("Table", view.transform, new Color32(5, 11, 23, 255), new Vector2(.02f, .05f), new Vector2(.98f, .74f));
            CreateRuntimeText("EmptyState", table.transform, emptyMessage, 20, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter,
                new Vector2(.08f, .12f), new Vector2(.92f, .88f), FontStyle.Bold);
            return view;
        }

        private GameObject CreateLocationsView(Transform workspace)
        {
            var view = CreateRuntimePanel("LocationsView", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            var toolbar = CreateRuntimePanel("LocationsToolbar", view.transform, new Color32(12, 21, 37, 255), new Vector2(.02f, .79f), new Vector2(.98f, .96f));
            locationCountText = CreateRuntimeText("LocationCount", toolbar.transform, "LOCATIONS  /  0", 17, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                new Vector2(.025f, 0), new Vector2(.65f, 1), FontStyle.Bold);
            var create = CreateRuntimeButton("CreateLocationButton", toolbar.transform, "+  CREATE LOCATION", new Vector2(.76f, .16f), new Vector2(.975f, .84f),
                new Color32(240, 190, 42, 255), new Color32(5, 9, 20, 255)); create.onClick.AddListener(ShowLocationCreation);
            var table = CreateRuntimePanel("LocationTable", view.transform, new Color32(5, 11, 23, 255), new Vector2(.02f, .05f), new Vector2(.98f, .74f));
            table.AddComponent<RectMask2D>(); var scroll = table.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true; scroll.scrollSensitivity = 4f; scroll.movementType = ScrollRect.MovementType.Clamped;
            locationGrid = new GameObject("LocationCardGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter)).transform;
            locationGrid.SetParent(table.transform, false); var locationRect = locationGrid.GetComponent<RectTransform>();
            locationRect.anchorMin = new Vector2(.02f, 1); locationRect.anchorMax = new Vector2(.98f, 1); locationRect.pivot = new Vector2(.5f, 1);
            locationRect.anchoredPosition = new Vector2(0, -18); locationRect.sizeDelta = new Vector2(0, 180);
            var grid = locationGrid.GetComponent<GridLayoutGroup>(); grid.cellSize = new Vector2(420, 160); grid.spacing = new Vector2(20, 18);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 3; grid.childAlignment = TextAnchor.UpperCenter;
            locationGrid.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = table.GetComponent<RectTransform>(); scroll.content = locationRect;
            emptyLocationsText = CreateRuntimeText("EmptyLocations", table.transform, "NO LOCATIONS CREATED\n\nCreate the first venue for this universe.", 20,
                new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter, new Vector2(.08f, .12f), new Vector2(.92f, .88f), FontStyle.Bold);
            return view;
        }

        private GameObject CreateBrandsView(Transform workspace)
        {
            var view = CreateRuntimePanel("BrandsView", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            var toolbar = CreateRuntimePanel("BrandsToolbar", view.transform, new Color32(12, 21, 37, 255), new Vector2(.02f, .79f), new Vector2(.98f, .96f));
            brandCountText = CreateRuntimeText("BrandCount", toolbar.transform, "BRANDS  /  0", 17, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                new Vector2(.025f, 0), new Vector2(.65f, 1), FontStyle.Bold);
            var create = CreateRuntimeButton("CreateBrandButton", toolbar.transform, "+  CREATE BRAND", new Vector2(.76f, .16f), new Vector2(.975f, .84f),
                new Color32(240, 190, 42, 255), new Color32(5, 9, 20, 255)); create.onClick.AddListener(ShowBrandCreation);
            var table = CreateRuntimePanel("BrandTable", view.transform, new Color32(5, 11, 23, 255), new Vector2(.02f, .05f), new Vector2(.98f, .74f));
            brandGrid = new GameObject("BrandCardGrid", typeof(RectTransform), typeof(GridLayoutGroup)).transform;
            brandGrid.SetParent(table.transform, false); SetRuntimeRect(brandGrid.GetComponent<RectTransform>(), new Vector2(.02f, .05f), new Vector2(.98f, .95f));
            var grid = brandGrid.GetComponent<GridLayoutGroup>(); grid.cellSize = new Vector2(330, 180); grid.spacing = new Vector2(18, 18);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 4; grid.childAlignment = TextAnchor.UpperCenter;
            emptyBrandsText = CreateRuntimeText("EmptyBrands", table.transform, "NO BRANDS CREATED\n\nCreate Raw, SmackDown, ECW, NXT, or your own brand.", 20,
                new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter, new Vector2(.08f, .12f), new Vector2(.92f, .88f), FontStyle.Bold);
            return view;
        }

        private GameObject CreateBrandCreationPanel(Transform workspace)
        {
            var panel = CreateRuntimePanel("BrandCreationPanel", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            brandFormTitle = CreateRuntimeText("Title", panel.transform, "CREATE BRAND", 27, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.035f, .84f), new Vector2(.55f, .98f), FontStyle.Bold);
            var back = CreateRuntimeButton("BackToBrandsButton", panel.transform, "BACK TO BRANDS", new Vector2(.80f, .86f), new Vector2(.965f, .96f),
                new Color32(25, 45, 65, 255), Color.white); back.onClick.AddListener(ShowBrands);
            brandNameInput = CreateRuntimeInput("BrandName", panel.transform, "BRAND NAME", "Brand name", new Vector2(.035f, .59f), new Vector2(.60f, .80f));
            brandColorInput = CreateRuntimeInput("BrandColor", panel.transform, "BRAND COLOR  (HEX)", "#E2231A", new Vector2(.035f, .34f), new Vector2(.38f, .54f));
            brandColorInput.characterLimit = 7;
            brandImageHost = CreateRuntimePanel("BrandImage", panel.transform, new Color32(5, 11, 23, 255), new Vector2(.63f, .32f), new Vector2(.965f, .80f));
            var choose = CreateRuntimeButton("ChooseBrandImage", brandImageHost.transform, "+  CHOOSE IMAGE", new Vector2(.06f, .05f), new Vector2(.94f, .24f),
                new Color32(25, 45, 65, 255), Color.white); choose.onClick.AddListener(PickBrandImage);
            brandImageStatus = CreateRuntimeText("ImageStatus", panel.transform, "No image selected", 12, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter,
                new Vector2(.63f, .26f), new Vector2(.965f, .32f));
            brandValidationText = CreateRuntimeText("Validation", panel.transform, string.Empty, 13, new Color32(255, 105, 105, 255), TextAnchor.MiddleLeft,
                new Vector2(.035f, .10f), new Vector2(.68f, .28f), FontStyle.Bold);
            var save = CreateRuntimeButton("SaveBrandButton", panel.transform, "CREATE BRAND", new Vector2(.76f, .08f), new Vector2(.965f, .23f),
                new Color32(240, 190, 42, 255), new Color32(5, 9, 20, 255)); brandSaveLabel = save.transform.Find("Label").GetComponent<Text>();
            save.onClick.AddListener(SaveBrand);
            return panel;
        }

        private GameObject CreateBrandInfoPanel(Transform workspace)
        {
            var panel = CreateRuntimePanel("BrandInfoPanel", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            brandInfoTitle = CreateRuntimeText("Title", panel.transform, "BRAND INFO", 27, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.04f, .78f), new Vector2(.68f, .96f), FontStyle.Bold);
            var back = CreateRuntimeButton("BackToBrandsButton", panel.transform, "BACK TO BRANDS", new Vector2(.80f, .82f), new Vector2(.965f, .94f),
                new Color32(25, 45, 65, 255), Color.white); back.onClick.AddListener(ShowBrands);
            var rosterPanel = CreateRuntimePanel("AssignedRoster", panel.transform, new Color32(5, 11, 23, 255), new Vector2(.04f, .08f), new Vector2(.96f, .72f));
            brandInfoRoster = CreateRuntimeText("Roster", rosterPanel.transform, "NO ROSTER MEMBERS ASSIGNED", 18, new Color32(142, 160, 181, 255),
                TextAnchor.UpperLeft, new Vector2(.05f, .08f), new Vector2(.95f, .92f), FontStyle.Bold);
            return panel;
        }

        private GameObject CreateTvShowsView(Transform workspace)
        {
            var view = CreateRuntimePanel("TvShowsView", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            var toolbar = CreateRuntimePanel("TvShowsToolbar", view.transform, new Color32(12, 21, 37, 255), new Vector2(.02f, .79f), new Vector2(.98f, .96f));
            tvShowCountText = CreateRuntimeText("ShowCount", toolbar.transform, "TV SHOWS  /  0", 17, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                new Vector2(.025f, 0), new Vector2(.65f, 1), FontStyle.Bold);
            var create = CreateRuntimeButton("CreateTvShowButton", toolbar.transform, "+  CREATE TV SHOW", new Vector2(.76f, .16f), new Vector2(.975f, .84f),
                new Color32(240, 190, 42, 255), new Color32(5, 9, 20, 255)); create.onClick.AddListener(ShowTvShowCreation);
            var table = CreateRuntimePanel("TvShowTable", view.transform, new Color32(5, 11, 23, 255), new Vector2(.02f, .05f), new Vector2(.98f, .74f));
            tvShowGrid = new GameObject("TvShowCardGrid", typeof(RectTransform), typeof(GridLayoutGroup)).transform;
            tvShowGrid.SetParent(table.transform, false); SetRuntimeRect(tvShowGrid.GetComponent<RectTransform>(), new Vector2(.02f, .05f), new Vector2(.98f, .95f));
            var grid = tvShowGrid.GetComponent<GridLayoutGroup>(); grid.cellSize = new Vector2(350, 190); grid.spacing = new Vector2(20, 18);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 4; grid.childAlignment = TextAnchor.UpperCenter;
            emptyTvShowsText = CreateRuntimeText("EmptyShows", table.transform, "NO TV SHOWS CREATED\n\nCreate the first recurring show for this universe.", 20,
                new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter, new Vector2(.08f, .12f), new Vector2(.92f, .88f), FontStyle.Bold);
            return view;
        }

        private GameObject CreateTvShowCreationPanel(Transform workspace)
        {
            var panel = CreateRuntimePanel("TvShowCreationPanel", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            tvShowFormTitle = CreateRuntimeText("Title", panel.transform, "CREATE TV SHOW", 27, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.035f, .84f), new Vector2(.55f, .98f), FontStyle.Bold);
            var back = CreateRuntimeButton("BackToTvShowsButton", panel.transform, "BACK TO TV SHOWS", new Vector2(.80f, .86f), new Vector2(.965f, .96f),
                new Color32(25, 45, 65, 255), Color.white); back.onClick.AddListener(ShowTvShows);
            tvShowNameInput = CreateRuntimeInput("ShowName", panel.transform, "SHOW NAME", "Monday Night Raw", new Vector2(.035f, .63f), new Vector2(.60f, .81f));
            tvShowFrequencyDropdown = CreateRuntimeDropdown("Frequency", panel.transform, "FREQUENCY", ShowFrequencies, 0, new Vector2(.035f, .41f), new Vector2(.31f, .59f));
            tvShowDayDropdown = CreateRuntimeDropdown("DayOfWeek", panel.transform, "DAY OF THE WEEK", WeekDays, 1, new Vector2(.33f, .41f), new Vector2(.60f, .59f));
            var brandSelector = CreateRuntimeButton("ParentBrandSelector", panel.transform, string.Empty, new Vector2(.035f, .19f), new Vector2(.60f, .37f),
                new Color32(5, 11, 23, 255), Color.white);
            var oldLabel = brandSelector.transform.Find("Label"); if (oldLabel != null) Destroy(oldLabel.gameObject);
            CreateRuntimeText("FieldLabel", brandSelector.transform, "PARENT BRANDS", 12, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                new Vector2(.04f, .58f), new Vector2(.96f, .94f), FontStyle.Bold);
            tvShowBrandCaption = CreateRuntimeText("Value", brandSelector.transform, "Select one or more brands", 17, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.04f, .06f), new Vector2(.90f, .62f));
            CreateRuntimeText("Arrow", brandSelector.transform, "▼", 13, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter,
                new Vector2(.91f, .08f), new Vector2(.98f, .62f)); brandSelector.onClick.AddListener(ToggleTvShowBrandMenu);
            tvShowBrandMenu = CreateRuntimePanel("ParentBrandDropdown", brandSelector.transform, new Color32(5, 9, 20, 255), new Vector2(0, -1.9f), new Vector2(1, 0));
            tvShowBrandMenu.SetActive(false);
            tvShowImageHost = CreateRuntimePanel("ShowImage", panel.transform, new Color32(5, 11, 23, 255), new Vector2(.64f, .28f), new Vector2(.965f, .81f));
            var choose = CreateRuntimeButton("ChooseShowImage", tvShowImageHost.transform, "+  CHOOSE SHOW IMAGE", new Vector2(.06f, .05f), new Vector2(.94f, .22f),
                new Color32(25, 45, 65, 255), Color.white); choose.onClick.AddListener(PickTvShowImage);
            tvShowImageStatus = CreateRuntimeText("ImageStatus", panel.transform, "No image selected", 12, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter,
                new Vector2(.64f, .22f), new Vector2(.965f, .28f));
            tvShowValidationText = CreateRuntimeText("Validation", panel.transform, string.Empty, 13, new Color32(255, 105, 105, 255), TextAnchor.MiddleLeft,
                new Vector2(.035f, .04f), new Vector2(.70f, .17f), FontStyle.Bold);
            var save = CreateRuntimeButton("SaveTvShowButton", panel.transform, "CREATE TV SHOW", new Vector2(.76f, .06f), new Vector2(.965f, .19f),
                new Color32(240, 190, 42, 255), new Color32(5, 9, 20, 255)); tvShowSaveLabel = save.transform.Find("Label").GetComponent<Text>();
            save.onClick.AddListener(SaveTvShow);
            return panel;
        }

        private GameObject CreateSpecialsView(Transform workspace)
        {
            var view = CreateRuntimePanel("SpecialsView", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            var toolbar = CreateRuntimePanel("SpecialsToolbar", view.transform, new Color32(12, 21, 37, 255), new Vector2(.02f, .79f), new Vector2(.98f, .96f));
            specialCountText = CreateRuntimeText("SpecialCount", toolbar.transform, "SPECIALS  /  0", 17, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                new Vector2(.025f, 0), new Vector2(.65f, 1), FontStyle.Bold);
            var create = CreateRuntimeButton("CreateSpecialButton", toolbar.transform, "+  CREATE SPECIAL", new Vector2(.76f, .16f), new Vector2(.975f, .84f),
                new Color32(240, 190, 42, 255), new Color32(5, 9, 20, 255)); create.onClick.AddListener(ShowSpecialCreation);
            var table = CreateRuntimePanel("SpecialTable", view.transform, new Color32(5, 11, 23, 255), new Vector2(.02f, .05f), new Vector2(.98f, .74f));
            specialGrid = new GameObject("SpecialCardGrid", typeof(RectTransform), typeof(GridLayoutGroup)).transform;
            specialGrid.SetParent(table.transform, false); SetRuntimeRect(specialGrid.GetComponent<RectTransform>(), new Vector2(.02f, .05f), new Vector2(.98f, .95f));
            var grid = specialGrid.GetComponent<GridLayoutGroup>(); grid.cellSize = new Vector2(350, 190); grid.spacing = new Vector2(20, 18);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 4; grid.childAlignment = TextAnchor.UpperCenter;
            emptySpecialsText = CreateRuntimeText("EmptySpecials", table.transform, "NO SPECIALS CREATED\n\nCreate the first PLE, PPV, or special event for this universe.", 20,
                new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter, new Vector2(.08f, .12f), new Vector2(.92f, .88f), FontStyle.Bold);
            return view;
        }

        private GameObject CreateSpecialCreationPanel(Transform workspace)
        {
            var panel = CreateRuntimePanel("SpecialCreationPanel", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            specialFormTitle = CreateRuntimeText("Title", panel.transform, "CREATE SPECIAL", 27, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.035f, .84f), new Vector2(.55f, .98f), FontStyle.Bold);
            var back = CreateRuntimeButton("BackToSpecialsButton", panel.transform, "BACK TO SPECIALS", new Vector2(.80f, .86f), new Vector2(.965f, .96f),
                new Color32(25, 45, 65, 255), Color.white); back.onClick.AddListener(ShowSpecials);
            specialNameInput = CreateRuntimeInput("SpecialName", panel.transform, "SPECIAL NAME", "WrestleMania", new Vector2(.035f, .63f), new Vector2(.60f, .81f));
            specialMonthDropdown = CreateRuntimeDropdown("Month", panel.transform, "MONTH", Months, 0, new Vector2(.035f, .41f), new Vector2(.22f, .59f));
            specialWeekDropdown = CreateRuntimeDropdown("Week", panel.transform, "WEEK", MonthWeeks, 0, new Vector2(.235f, .41f), new Vector2(.41f, .59f));
            specialDayDropdown = CreateRuntimeDropdown("Day", panel.transform, "DAY", WeekDays, 0, new Vector2(.425f, .41f), new Vector2(.60f, .59f));
            var brandSelector = CreateRuntimeButton("ParticipatingBrandSelector", panel.transform, string.Empty, new Vector2(.035f, .19f), new Vector2(.60f, .37f),
                new Color32(5, 11, 23, 255), Color.white);
            var oldLabel = brandSelector.transform.Find("Label"); if (oldLabel != null) Destroy(oldLabel.gameObject);
            CreateRuntimeText("FieldLabel", brandSelector.transform, "PARTICIPATING BRANDS", 12, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                new Vector2(.04f, .58f), new Vector2(.96f, .94f), FontStyle.Bold);
            specialBrandCaption = CreateRuntimeText("Value", brandSelector.transform, "Select one or more brands", 17, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.04f, .06f), new Vector2(.90f, .62f));
            CreateRuntimeText("Arrow", brandSelector.transform, "▼", 13, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter,
                new Vector2(.91f, .08f), new Vector2(.98f, .62f)); brandSelector.onClick.AddListener(ToggleSpecialBrandMenu);
            specialBrandMenu = CreateRuntimePanel("ParticipatingBrandDropdown", brandSelector.transform, new Color32(5, 9, 20, 255), new Vector2(0, -1.9f), new Vector2(1, 0));
            specialBrandMenu.SetActive(false);
            specialImageHost = CreateRuntimePanel("SpecialImage", panel.transform, new Color32(5, 11, 23, 255), new Vector2(.64f, .28f), new Vector2(.965f, .81f));
            var choose = CreateRuntimeButton("ChooseSpecialImage", specialImageHost.transform, "+  CHOOSE SPECIAL IMAGE", new Vector2(.06f, .05f), new Vector2(.94f, .22f),
                new Color32(25, 45, 65, 255), Color.white); choose.onClick.AddListener(PickSpecialImage);
            specialImageStatus = CreateRuntimeText("ImageStatus", panel.transform, "No image selected", 12, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter,
                new Vector2(.64f, .22f), new Vector2(.965f, .28f));
            specialValidationText = CreateRuntimeText("Validation", panel.transform, string.Empty, 13, new Color32(255, 105, 105, 255), TextAnchor.MiddleLeft,
                new Vector2(.035f, .04f), new Vector2(.70f, .17f), FontStyle.Bold);
            var save = CreateRuntimeButton("SaveSpecialButton", panel.transform, "CREATE SPECIAL", new Vector2(.76f, .06f), new Vector2(.965f, .19f),
                new Color32(240, 190, 42, 255), new Color32(5, 9, 20, 255)); specialSaveLabel = save.transform.Find("Label").GetComponent<Text>();
            save.onClick.AddListener(SaveSpecial);
            return panel;
        }

        private GameObject CreateCalendarView(Transform workspace)
        {
            var view = CreateRuntimePanel("CalendarView", workspace, new Color32(3, 9, 14, 255), Vector2.zero, Vector2.one);
            calendarHeadingText = CreateRuntimeText("CalendarHeading", view.transform, "UNIVERSE CALENDAR", 24, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.02f, .87f), new Vector2(.42f, .99f), FontStyle.Bold);
            var backTen = CreateRuntimeButton("BackTenYears", view.transform, "-10", new Vector2(.50f, .88f), new Vector2(.545f, .975f),
                new Color32(25, 45, 65, 255), Color.white); backTen.onClick.AddListener(() => ChangeCalendarYear(-10));
            var backOne = CreateRuntimeButton("BackOneYear", view.transform, "<", new Vector2(.55f, .88f), new Vector2(.59f, .975f),
                new Color32(25, 45, 65, 255), Color.white); backOne.onClick.AddListener(() => ChangeCalendarYear(-1));
            calendarYearInput = CreateRuntimeInput("CalendarYear", view.transform, "YEAR", calendarYear.ToString(), new Vector2(.595f, .865f), new Vector2(.70f, .985f));
            calendarYearInput.contentType = InputField.ContentType.IntegerNumber; calendarYearInput.characterLimit = 4;
            calendarYearInput.onEndEdit.AddListener(_ => ApplyCalendarYearInput());
            var forwardOne = CreateRuntimeButton("ForwardOneYear", view.transform, ">", new Vector2(.705f, .88f), new Vector2(.745f, .975f),
                new Color32(25, 45, 65, 255), Color.white); forwardOne.onClick.AddListener(() => ChangeCalendarYear(1));
            var forwardTen = CreateRuntimeButton("ForwardTenYears", view.transform, "+10", new Vector2(.75f, .88f), new Vector2(.795f, .975f),
                new Color32(25, 45, 65, 255), Color.white); forwardTen.onClick.AddListener(() => ChangeCalendarYear(10));
            calendarMonthDropdown = CreateRuntimeDropdown("CalendarMonth", view.transform, "MONTH", Months, Mathf.Clamp(DateTime.Now.Month - 1, 0, 11),
                new Vector2(.81f, .865f), new Vector2(.98f, .985f));
            calendarMonthDropdown.onValueChanged.AddListener(_ => RefreshCalendar());

            var dayNames = new[] { "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SATURDAY", "SUNDAY" };
            const float gridLeft = .065f;
            const float gridRight = .99f;
            var columnWidth = (gridRight - gridLeft) / 7f;
            for (var day = 0; day < 7; day++)
                CreateRuntimeText("Day_" + day, view.transform, dayNames[day], 12, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter,
                    new Vector2(gridLeft + day * columnWidth, .79f), new Vector2(gridLeft + (day + 1) * columnWidth, .86f), FontStyle.Bold);

            calendarCells.Clear();
            const float top = .78f;
            const float bottom = .025f;
            var rowHeight = (top - bottom) / 4f;
            for (var week = 0; week < 4; week++)
            {
                var rowTop = top - week * rowHeight;
                var rowBottom = rowTop - rowHeight + .008f;
                CreateRuntimeText("WeekLabel_" + week, view.transform, "WEEK " + (week + 1), 12, new Color32(45, 190, 230, 255), TextAnchor.UpperLeft,
                    new Vector2(.008f, rowBottom), new Vector2(.062f, rowTop), FontStyle.Bold);
                for (var day = 0; day < 7; day++)
                {
                    var cell = CreateRuntimePanel("CalendarCell_W" + (week + 1) + "_D" + day, view.transform, new Color32(8, 17, 24, 255),
                        new Vector2(gridLeft + day * columnWidth + .002f, rowBottom), new Vector2(gridLeft + (day + 1) * columnWidth - .002f, rowTop - .008f));
                    calendarCells.Add(cell.transform);
                }
            }
            return view;
        }

        private GameObject CreateShowBookingPanel(Transform workspace)
        {
            var panel = CreateRuntimePanel("ShowBookingPanel", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            CreateRuntimeText("Eyebrow", panel.transform, "SHOW BOOKING", 14, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                new Vector2(.035f, .84f), new Vector2(.42f, .97f), FontStyle.Bold);
            bookingShowNameText = CreateRuntimeText("ShowName", panel.transform, "SHOW NAME", 30, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.035f, .72f), new Vector2(.68f, .87f), FontStyle.Bold);
            bookingScheduleText = CreateRuntimeText("Schedule", panel.transform, string.Empty, 15, new Color32(142, 160, 181, 255), TextAnchor.MiddleLeft,
                new Vector2(.035f, .64f), new Vector2(.72f, .74f), FontStyle.Bold);
            var back = CreateRuntimeButton("BackToCalendarButton", panel.transform, "BACK TO CALENDAR", new Vector2(.79f, .84f), new Vector2(.965f, .95f),
                new Color32(25, 45, 65, 255), Color.white); back.onClick.AddListener(ShowCalendar);
            var workspacePanel = CreateRuntimePanel("BookingWorkspace", panel.transform, new Color32(5, 11, 23, 255), new Vector2(.035f, .04f), new Vector2(.965f, .62f));
            workspacePanel.AddComponent<RectMask2D>();
            bookingAccordionScroll = workspacePanel.AddComponent<ScrollRect>(); bookingAccordionScroll.horizontal = false; bookingAccordionScroll.vertical = true;
            bookingAccordionScroll.movementType = ScrollRect.MovementType.Clamped; bookingAccordionScroll.scrollSensitivity = 4f;
            var contentObject = new GameObject("AccordionContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObject.transform.SetParent(workspacePanel.transform, false); bookingAccordionContent = contentObject.GetComponent<RectTransform>();
            bookingAccordionContent.anchorMin = new Vector2(.02f, 1); bookingAccordionContent.anchorMax = new Vector2(.98f, 1);
            bookingAccordionContent.pivot = new Vector2(.5f, 1); bookingAccordionContent.anchoredPosition = new Vector2(0, -12); bookingAccordionContent.sizeDelta = Vector2.zero;
            var accordionLayout = contentObject.GetComponent<VerticalLayoutGroup>(); accordionLayout.spacing = 14; accordionLayout.padding = new RectOffset(0, 0, 0, 18);
            accordionLayout.childControlWidth = true; accordionLayout.childControlHeight = true; accordionLayout.childForceExpandWidth = true; accordionLayout.childForceExpandHeight = false;
            contentObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            bookingAccordionScroll.viewport = workspacePanel.GetComponent<RectTransform>(); bookingAccordionScroll.content = bookingAccordionContent;
            matchBookingHeader = CreateBookingAccordionHeader("AddMatchHeader", bookingAccordionContent, "+", "ADD MATCH",
                new Color32(45, 190, 230, 255), out matchBookingArrow);
            matchBookingHeader.AddComponent<LayoutElement>().preferredHeight = 72;
            matchBookingHeader.GetComponent<Button>().onClick.AddListener(ToggleMatchBooking);
            matchBookingBody = CreateRuntimePanel("MatchBookingBody", bookingAccordionContent, new Color32(8, 15, 27, 255), Vector2.zero, Vector2.one);
            matchBookingBodyLayout = matchBookingBody.AddComponent<LayoutElement>(); matchBookingBodyLayout.preferredHeight = 500;
            matchStipulationDropdown = CreateRuntimeDropdown("MatchStipulation", matchBookingBody.transform, "STIPULATION", MatchStipulations, 0,
                new Vector2(.035f, .76f), new Vector2(.485f, .96f));
            matchFormatDropdown = CreateRuntimeDropdown("MatchFormat", matchBookingBody.transform, "FORMAT", MatchFormats, 0,
                new Vector2(.515f, .76f), new Vector2(.965f, .96f));
            matchTitleDropdown = CreateRuntimeDropdown("MatchTitle", matchBookingBody.transform, "TITLE ON THE LINE", new[] { "None" }, 0,
                new Vector2(.035f, .52f), new Vector2(.61f, .71f));
            matchGenderDropdown = CreateRuntimeDropdown("MatchGender", matchBookingBody.transform, "GENDER FILTER", MatchGenderFilters, 0,
                new Vector2(.64f, .52f), new Vector2(.965f, .71f));
            matchParticipantSelector = CreateRuntimeButton("MatchParticipantSelector", matchBookingBody.transform, string.Empty,
                new Vector2(.035f, .23f), new Vector2(.965f, .45f), new Color32(5, 11, 23, 255), Color.white);
            var participantSelector = matchParticipantSelector;
            var participantLabel = participantSelector.transform.Find("Label"); if (participantLabel != null) Destroy(participantLabel.gameObject);
            CreateRuntimeText("FieldLabel", participantSelector.transform, "PARTICIPANTS", 12, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                new Vector2(.035f, .58f), new Vector2(.96f, .94f), FontStyle.Bold);
            matchParticipantCaption = CreateRuntimeText("Value", participantSelector.transform, "Select 2 wrestlers", 16, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.035f, .06f), new Vector2(.90f, .62f));
            CreateRuntimeText("Arrow", participantSelector.transform, "▼", 13, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter,
                new Vector2(.92f, .08f), new Vector2(.98f, .62f)); participantSelector.onClick.AddListener(ToggleMatchParticipantMenu);
            matchParticipantMenu = CreateRuntimePanel("MatchParticipantDropdown", participantSelector.transform, new Color32(5, 9, 20, 255),
                new Vector2(0, 1f), new Vector2(1, 3.2f));
            var participantCanvas = matchParticipantMenu.AddComponent<Canvas>(); participantCanvas.overrideSorting = true; participantCanvas.sortingOrder = 500;
            matchParticipantMenu.AddComponent<GraphicRaycaster>(); matchParticipantMenu.SetActive(false);
            matchStagesGroup = CreateRuntimePanel("ThreeStagesSelectors", matchBookingBody.transform, new Color32(8, 15, 27, 0),
                new Vector2(.035f, .155f), new Vector2(.965f, .34f));
            matchStageOneDropdown = CreateRuntimeDropdown("StageOne", matchStagesGroup.transform, "STAGE 1", ThreeStagesOfHellOptions, 0,
                new Vector2(0, 0), new Vector2(.315f, 1));
            matchStageTwoDropdown = CreateRuntimeDropdown("StageTwo", matchStagesGroup.transform, "STAGE 2", ThreeStagesOfHellOptions, 1,
                new Vector2(.3425f, 0), new Vector2(.6575f, 1));
            matchStageThreeDropdown = CreateRuntimeDropdown("StageThree", matchStagesGroup.transform, "STAGE 3", ThreeStagesOfHellOptions, 2,
                new Vector2(.685f, 0), new Vector2(1, 1));
            matchStagesGroup.SetActive(false);
            matchBookingValidationText = CreateRuntimeText("MatchValidation", matchBookingBody.transform, string.Empty, 12, new Color32(255, 105, 105, 255),
                TextAnchor.MiddleLeft, new Vector2(.035f, .15f), new Vector2(.72f, .22f), FontStyle.Bold);
            addMatchToCardButton = CreateRuntimeButton("AddMatchToCard", matchBookingBody.transform, "+  ADD TO CARD",
                new Vector2(.035f, .025f), new Vector2(.965f, .14f), new Color32(25, 45, 65, 255), Color.white);
            addMatchToCardLabel = addMatchToCardButton.transform.Find("Label").GetComponent<Text>();
            addMatchToCardButton.onClick.AddListener(AddMatchToCard);
            matchStipulationDropdown.onValueChanged.AddListener(_ => RefreshMatchFormats());
            matchFormatDropdown.onValueChanged.AddListener(_ => HandleMatchFormatChanged());
            matchGenderDropdown.onValueChanged.AddListener(_ => RefreshMatchParticipants());
            segmentBookingHeader = CreateBookingAccordionHeader("AddSegmentHeader", bookingAccordionContent, "●", "ADD SEGMENT",
                new Color32(185, 103, 255, 255), out segmentBookingArrow);
            segmentBookingHeader.AddComponent<LayoutElement>().preferredHeight = 72;
            segmentBookingHeader.GetComponent<Button>().onClick.AddListener(ToggleSegmentBooking);
            segmentBookingBody = CreateRuntimePanel("SegmentBookingBody", bookingAccordionContent, new Color32(12, 10, 25, 255), Vector2.zero, Vector2.one);
            segmentBookingBody.AddComponent<LayoutElement>().preferredHeight = 300;
            var matchListObject = new GameObject("BookedMatchCardList", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            matchListObject.transform.SetParent(bookingAccordionContent, false); bookedMatchCardList = matchListObject.transform;
            var matchListLayout = matchListObject.GetComponent<VerticalLayoutGroup>(); matchListLayout.spacing = 12;
            matchListLayout.childControlWidth = true; matchListLayout.childControlHeight = true; matchListLayout.childForceExpandWidth = true; matchListLayout.childForceExpandHeight = false;
            bookedMatchCardListLayout = matchListObject.GetComponent<LayoutElement>(); bookedMatchCardListLayout.preferredHeight = 72;
            matchBookingExpanded = false; segmentBookingExpanded = false; RefreshBookingAccordionLayout();
            if (bookingAccordionScroll != null) bookingAccordionScroll.verticalNormalizedPosition = 1f;
            return panel;
        }

        private GameObject CreateBookingAccordionHeader(string name, Transform parent, string icon, string label, Color accent, out Text arrow)
        {
            var button = CreateRuntimeButton(name, parent, string.Empty, Vector2.zero, Vector2.one, new Color32(7, 12, 22, 255), Color.white);
            var oldLabel = button.transform.Find("Label"); if (oldLabel != null) Destroy(oldLabel.gameObject);
            CreateRuntimeText("Icon", button.transform, icon, 26, accent, TextAnchor.MiddleCenter,
                new Vector2(.025f, .12f), new Vector2(.085f, .88f), FontStyle.Bold);
            CreateRuntimeText("Title", button.transform, label, 19, accent, TextAnchor.MiddleLeft,
                new Vector2(.095f, .08f), new Vector2(.75f, .92f), FontStyle.Bold);
            arrow = CreateRuntimeText("Arrow", button.transform, "▼", 17, new Color32(190, 198, 210, 255), TextAnchor.MiddleCenter,
                new Vector2(.92f, .10f), new Vector2(.98f, .90f), FontStyle.Bold);
            return button.gameObject;
        }

        private GameObject CreateLocationCreationPanel(Transform workspace)
        {
            var panel = CreateRuntimePanel("LocationCreationPanel", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            locationFormTitle = CreateRuntimeText("Title", panel.transform, "CREATE LOCATION", 27, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.035f, .84f), new Vector2(.55f, .98f), FontStyle.Bold);
            var back = CreateRuntimeButton("BackToLocationsButton", panel.transform, "BACK TO LOCATIONS", new Vector2(.78f, .86f), new Vector2(.965f, .96f),
                new Color32(25, 45, 65, 255), Color.white); back.onClick.AddListener(ShowLocations);
            venueNameInput = CreateRuntimeInput("VenueName", panel.transform, "VENUE NAME", "Madison Square Garden", new Vector2(.035f, .60f), new Vector2(.965f, .80f));
            venueLocationInput = CreateRuntimeInput("VenueLocation", panel.transform, "VENUE LOCATION", "New York, NY", new Vector2(.035f, .36f), new Vector2(.62f, .55f));
            venueCapacityInput = CreateRuntimeInput("VenueCapacity", panel.transform, "VENUE CAPACITY", "19812", new Vector2(.65f, .36f), new Vector2(.965f, .55f));
            venueCapacityInput.contentType = InputField.ContentType.IntegerNumber; venueCapacityInput.characterLimit = 9;
            locationValidationText = CreateRuntimeText("Validation", panel.transform, string.Empty, 13, new Color32(255, 105, 105, 255), TextAnchor.MiddleLeft,
                new Vector2(.035f, .13f), new Vector2(.70f, .30f), FontStyle.Bold);
            var save = CreateRuntimeButton("SaveLocationButton", panel.transform, "CREATE LOCATION", new Vector2(.76f, .14f), new Vector2(.965f, .31f),
                new Color32(240, 190, 42, 255), new Color32(5, 9, 20, 255)); locationSaveLabel = save.transform.Find("Label").GetComponent<Text>();
            save.onClick.AddListener(SaveLocation);
            return panel;
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
            teamGridRect.pivot = new Vector2(.5f, 1); teamGridRect.anchoredPosition = new Vector2(0, -20); teamGridRect.sizeDelta = new Vector2(0, 360);
            var grid = teamGrid.GetComponent<GridLayoutGroup>(); grid.cellSize = new Vector2(470, 360); grid.spacing = new Vector2(22, 22);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 3; grid.childAlignment = TextAnchor.UpperCenter;
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
            memberDropdownMenu = CreateRuntimePanel("MemberDropdown", memberField.transform, new Color32(5, 9, 20, 255), new Vector2(0, 1f), new Vector2(1, 2.9f));
            var memberCanvas = memberDropdownMenu.AddComponent<Canvas>(); memberCanvas.overrideSorting = true; memberCanvas.sortingOrder = 500;
            memberDropdownMenu.AddComponent<GraphicRaycaster>(); memberDropdownMenu.SetActive(false);

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

        private GameObject CreateTitlesView(Transform workspace)
        {
            var view = CreateRuntimePanel("TitlesView", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            var toolbar = CreateRuntimePanel("TitlesToolbar", view.transform, new Color32(12, 21, 37, 255), new Vector2(.02f, .79f), new Vector2(.98f, .96f));
            titleCountText = CreateRuntimeText("TitleCount", toolbar.transform, "TITLES  /  0", 17, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                new Vector2(.025f, 0), new Vector2(.65f, 1), FontStyle.Bold);
            var create = CreateRuntimeButton("CreateTitleButton", toolbar.transform, "+  CREATE TITLE", new Vector2(.76f, .16f), new Vector2(.975f, .84f),
                new Color32(240, 190, 42, 255), new Color32(5, 9, 20, 255)); create.onClick.AddListener(ShowTitleCreation);
            var table = CreateRuntimePanel("TitleTable", view.transform, new Color32(5, 11, 23, 255), new Vector2(.02f, .05f), new Vector2(.98f, .74f));
            table.AddComponent<RectMask2D>(); var scroll = table.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true; scroll.scrollSensitivity = 4f; scroll.movementType = ScrollRect.MovementType.Clamped;
            titleGrid = new GameObject("TitleCardGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter)).transform;
            titleGrid.SetParent(table.transform, false); var rect = titleGrid.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(.02f, 1); rect.anchorMax = new Vector2(.98f, 1); rect.pivot = new Vector2(.5f, 1); rect.anchoredPosition = new Vector2(0, -18); rect.sizeDelta = new Vector2(0, 230);
            var grid = titleGrid.GetComponent<GridLayoutGroup>(); grid.cellSize = new Vector2(430, 230); grid.spacing = new Vector2(22, 20);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 3; grid.childAlignment = TextAnchor.UpperCenter;
            titleGrid.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = table.GetComponent<RectTransform>(); scroll.content = rect;
            emptyTitlesText = CreateRuntimeText("EmptyTitles", table.transform, "NO TITLES CREATED\n\nUse CREATE TITLE to add the first championship.", 20,
                new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter, new Vector2(.08f, .12f), new Vector2(.92f, .88f), FontStyle.Bold);
            return view;
        }

        private GameObject CreateTitleCreationPanel(Transform workspace)
        {
            var panel = CreateRuntimePanel("TitleCreationPanel", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            titleFormTitle = CreateRuntimeText("Title", panel.transform, "CREATE TITLE", 27, Color.white, TextAnchor.MiddleLeft, new Vector2(.035f, .84f), new Vector2(.5f, .98f), FontStyle.Bold);
            var back = CreateRuntimeButton("BackToTitlesButton", panel.transform, "BACK TO TITLES", new Vector2(.80f, .86f), new Vector2(.965f, .96f),
                new Color32(25, 45, 65, 255), Color.white); back.onClick.AddListener(ShowTitles);
            titleNameInput = CreateRuntimeInput("TitleName", panel.transform, "TITLE NAME", "Championship name", new Vector2(.035f, .62f), new Vector2(.55f, .80f));
            titleBrandDropdown = CreateRuntimeDropdown("TitleBrand", panel.transform, "BRAND", new[] { "Unassigned" }, 0, new Vector2(.58f, .62f), new Vector2(.965f, .80f));
            titleHolderDropdown = CreateRuntimeDropdown("TitleHolder", panel.transform, "TITLE HOLDER", new[] { "Vacant" }, 0, new Vector2(.035f, .38f), new Vector2(.55f, .57f));
            titleDivisionDropdown = CreateRuntimeDropdown("TitleDivision", panel.transform, "DIVISION", TitleDivisions, 0, new Vector2(.035f, .19f), new Vector2(.55f, .35f));
            titleImageHost = CreateRuntimePanel("TitleImage", panel.transform, new Color32(5, 11, 23, 255), new Vector2(.58f, .25f), new Vector2(.965f, .57f));
            var choose = CreateRuntimeButton("ChooseTitleImage", titleImageHost.transform, "+  CHOOSE TITLE IMAGE", new Vector2(.06f, .05f), new Vector2(.94f, .25f),
                new Color32(25, 45, 65, 255), Color.white); choose.onClick.AddListener(PickTitleImage);
            titleImageStatus = CreateRuntimeText("ImageStatus", panel.transform, "No image selected", 12, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter,
                new Vector2(.58f, .19f), new Vector2(.965f, .25f));
            titleValidationText = CreateRuntimeText("Validation", panel.transform, string.Empty, 13, new Color32(255, 105, 105, 255), TextAnchor.MiddleLeft,
                new Vector2(.035f, .04f), new Vector2(.62f, .17f), FontStyle.Bold);
            var save = CreateRuntimeButton("SaveTitleButton", panel.transform, "CREATE TITLE", new Vector2(.76f, .06f), new Vector2(.965f, .18f),
                new Color32(240, 190, 42, 255), new Color32(5, 9, 20, 255)); save.onClick.AddListener(SaveTitle);
            titleSaveLabel = save.transform.Find("Label").GetComponent<Text>();
            return panel;
        }

        private GameObject CreateTitleHistoryPanel(Transform workspace)
        {
            var panel = CreateRuntimePanel("TitleHistoryPanel", workspace, new Color32(9, 15, 29, 255), Vector2.zero, Vector2.one);
            CreateRuntimeText("Eyebrow", panel.transform, "CHAMPIONSHIP HISTORY", 14, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                new Vector2(.04f, .78f), new Vector2(.62f, .94f), FontStyle.Bold);
            titleHistoryNameText = CreateRuntimeText("TitleName", panel.transform, "TITLE HISTORY", 29, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.04f, .64f), new Vector2(.72f, .82f), FontStyle.Bold);
            var back = CreateRuntimeButton("BackToTitlesButton", panel.transform, "BACK TO TITLES", new Vector2(.80f, .80f), new Vector2(.965f, .94f),
                new Color32(25, 45, 65, 255), Color.white); back.onClick.AddListener(ShowTitles);
            var historyTable = CreateRuntimePanel("HistoryTable", panel.transform, new Color32(5, 11, 23, 255), new Vector2(.04f, .08f), new Vector2(.96f, .59f));
            CreateRuntimeText("EmptyHistory", historyTable.transform,
                "NO TITLE HISTORY RECORDED\n\nChampionship reigns, holders, victories, vacancies, and dates will appear here.",
                20, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter,
                new Vector2(.08f, .12f), new Vector2(.92f, .88f), FontStyle.Bold);
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
            contentRect.sizeDelta = new Vector2(0, 390);
            var grid = content.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(340, 390);
            grid.spacing = new Vector2(22, 22);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
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
            teamNameInput.text = string.Empty; PopulateBrandDropdown(teamBrandDropdown); teamDispositionDropdown.value = 0;
            teamDispositionDropdown.RefreshShownValue();
            selectedTeamMemberIds.Clear(); teamValidationText.text = string.Empty; BuildMemberDropdown();
            selectedTeamPhotoPath = string.Empty; teamPhotoStatus.text = "No photo selected";
            var oldPreview = teamPhotoHost.transform.Find("PhotoPreview"); if (oldPreview != null) Destroy(oldPreview.gameObject);
        }

        private void EditTeam(TeamRecord team)
        {
            ShowTeamCreation(); editingTeam = team; teamFormTitle.text = "EDIT TEAM"; teamSaveLabel.text = "SAVE CHANGES";
            teamNameInput.text = team.name; PopulateBrandDropdown(teamBrandDropdown, team.brand); SetDropdownValue(teamDispositionDropdown, team.disposition);
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
                if (texture != null) { loadedTextures.Add(texture); SetRuntimePhoto(card.transform, "TeamPhoto", texture, new Vector2(.04f, .43f), new Vector2(.96f, .97f)); }
                var textMin = .06f;
                CreateRuntimeText("Name", card.transform, team.name.ToUpperInvariant(), 20, Color.white, TextAnchor.MiddleLeft,
                    new Vector2(textMin, .31f), new Vector2(.94f, .43f), FontStyle.Bold);
                CreateRuntimeText("Members", card.transform, string.Join("  /  ", team.memberNames.ToArray()), 14, new Color32(142, 160, 181, 255),
                    TextAnchor.MiddleLeft, new Vector2(textMin, .20f), new Vector2(.94f, .31f));
                CreateRuntimeText("Disposition", card.transform, team.disposition.ToUpperInvariant(), 12, new Color32(45, 190, 230, 255),
                    TextAnchor.MiddleLeft, new Vector2(textMin, .13f), new Vector2(.94f, .20f), FontStyle.Bold);
                var edit = CreateRuntimeButton("EditButton", card.transform, "EDIT", new Vector2(.06f, .025f), new Vector2(.94f, .12f),
                    new Color32(25, 45, 65, 255), Color.white); edit.onClick.AddListener(() => EditTeam(team));
            }
        }

        private void ShowTitleCreation()
        {
            SelectSection("RosterButton", "CREATE TITLE", string.Empty); sectionContentText.gameObject.SetActive(false); titleCreationPanel.SetActive(true);
            editingTitle = null; titleFormTitle.text = "CREATE TITLE"; titleSaveLabel.text = "CREATE TITLE";
            titleNameInput.text = string.Empty; PopulateBrandDropdown(titleBrandDropdown); titleDivisionDropdown.value = 0; selectedTitleImagePath = string.Empty;
            titleDivisionDropdown.RefreshShownValue();
            titleImageStatus.text = "No image selected"; titleValidationText.text = string.Empty;
            var old = titleImageHost.transform.Find("ImagePreview"); if (old != null) Destroy(old.gameObject);
            titleHolderOptions = repository.LoadWrestlers(ActiveUniverseSession.UniverseId);
            var options = new List<string> { "Vacant" }; foreach (var wrestler in titleHolderOptions) options.Add(wrestler.name);
            titleHolderDropdown.ClearOptions(); titleHolderDropdown.AddOptions(options); titleHolderDropdown.value = 0; titleHolderDropdown.RefreshShownValue();
        }

        private void ShowLocationCreation()
        {
            SelectSection("MyUniverseButton", "CREATE LOCATION", string.Empty); sectionContentText.gameObject.SetActive(false);
            locationCreationPanel.SetActive(true); editingLocation = null; locationFormTitle.text = "CREATE LOCATION"; locationSaveLabel.text = "CREATE LOCATION";
            venueNameInput.text = string.Empty; venueLocationInput.text = string.Empty; venueCapacityInput.text = string.Empty; locationValidationText.text = string.Empty;
        }

        private void ShowBrandCreation()
        {
            SelectSection("MyUniverseButton", "CREATE BRAND", string.Empty); sectionContentText.gameObject.SetActive(false); brandCreationPanel.SetActive(true);
            editingBrand = null; brandFormTitle.text = "CREATE BRAND"; brandSaveLabel.text = "CREATE BRAND";
            brandNameInput.text = string.Empty; brandColorInput.text = "#FFFFFF"; selectedBrandImagePath = string.Empty;
            brandImageStatus.text = "No image selected"; brandValidationText.text = string.Empty;
            var old = brandImageHost.transform.Find("ImagePreview"); if (old != null) Destroy(old.gameObject);
        }

        private void ShowTvShowCreation()
        {
            SelectSection("MyUniverseButton", "CREATE TV SHOW", string.Empty); sectionContentText.gameObject.SetActive(false); tvShowCreationPanel.SetActive(true);
            editingTvShow = null; tvShowFormTitle.text = "CREATE TV SHOW"; tvShowSaveLabel.text = "CREATE TV SHOW";
            tvShowNameInput.text = string.Empty; tvShowFrequencyDropdown.value = 0; tvShowDayDropdown.value = 1;
            selectedTvShowBrandIds.Clear(); selectedTvShowImagePath = string.Empty; tvShowImageStatus.text = "No image selected"; tvShowValidationText.text = string.Empty;
            var old = tvShowImageHost.transform.Find("ImagePreview"); if (old != null) Destroy(old.gameObject); BuildTvShowBrandMenu();
        }

        private void EditTvShow(TvShowRecord show)
        {
            ShowTvShowCreation(); editingTvShow = show; tvShowFormTitle.text = "EDIT TV SHOW"; tvShowSaveLabel.text = "SAVE CHANGES";
            tvShowNameInput.text = show.name; SetDropdownValue(tvShowFrequencyDropdown, show.frequency); SetDropdownValue(tvShowDayDropdown, show.dayOfWeek);
            selectedTvShowBrandIds.Clear(); selectedTvShowBrandIds.AddRange(show.brandIds); BuildTvShowBrandMenu();
            tvShowImageStatus.text = string.IsNullOrEmpty(show.imagePath) ? "No image selected" : System.IO.Path.GetFileName(show.imagePath);
            var texture = UniverseImageStorage.LoadTexture(show.imagePath);
            if (texture != null) { loadedTextures.Add(texture); SetRuntimePhoto(tvShowImageHost.transform, "ImagePreview", texture, new Vector2(.05f, .25f), new Vector2(.95f, .95f)); }
        }

        private void ToggleTvShowBrandMenu() => tvShowBrandMenu.SetActive(!tvShowBrandMenu.activeSelf);

        private void BuildTvShowBrandMenu()
        {
            availableTvShowBrands = repository.LoadBrands(ActiveUniverseSession.UniverseId);
            for (var index = tvShowBrandMenu.transform.childCount - 1; index >= 0; index--) Destroy(tvShowBrandMenu.transform.GetChild(index).gameObject);
            var count = Mathf.Max(1, availableTvShowBrands.Count); var height = 1f / count;
            for (var index = 0; index < availableTvShowBrands.Count; index++)
            {
                var brand = availableTvShowBrands[index]; var top = 1f - index * height; var bottom = top - height;
                var selected = selectedTvShowBrandIds.Contains(brand.id);
                var row = CreateRuntimeButton("Brand_" + brand.id, tvShowBrandMenu.transform, (selected ? "✓  " : "     ") + brand.name,
                    new Vector2(.02f, bottom), new Vector2(.98f, top), selected ? new Color32(25, 65, 82, 255) : new Color32(9, 15, 29, 255), Color.white);
                row.onClick.AddListener(() => ToggleTvShowBrand(brand.id));
            }
            if (availableTvShowBrands.Count == 0)
                CreateRuntimeText("NoBrands", tvShowBrandMenu.transform, "Create a Brand first", 14, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
            var names = new List<string>(); foreach (var id in selectedTvShowBrandIds)
            { var brand = availableTvShowBrands.Find(item => item.id == id); if (brand != null) names.Add(brand.name); }
            tvShowBrandCaption.text = names.Count == 0 ? "Select one or more brands" : string.Join(", ", names.ToArray());
        }

        private void ToggleTvShowBrand(string brandId)
        {
            if (selectedTvShowBrandIds.Contains(brandId)) selectedTvShowBrandIds.Remove(brandId); else selectedTvShowBrandIds.Add(brandId);
            BuildTvShowBrandMenu(); tvShowBrandMenu.SetActive(true);
        }

        private void PickTvShowImage()
        {
            string path; if (!WindowsImageFilePicker.TryPickImage(out path)) return;
            var texture = UniverseImageStorage.LoadTexture(path);
            if (texture == null) { tvShowValidationText.text = "Unity could not decode that image."; return; }
            loadedTextures.Add(texture); selectedTvShowImagePath = path; tvShowImageStatus.text = System.IO.Path.GetFileName(path);
            SetRuntimePhoto(tvShowImageHost.transform, "ImagePreview", texture, new Vector2(.05f, .25f), new Vector2(.95f, .95f));
        }

        private void SaveTvShow()
        {
            if (string.IsNullOrWhiteSpace(tvShowNameInput.text)) { tvShowValidationText.text = "Show name is required."; return; }
            if (selectedTvShowBrandIds.Count == 0) { tvShowValidationText.text = "Select at least one Parent Brand."; return; }
            try
            {
                var show = new TvShowRecord { id = editingTvShow == null ? Guid.NewGuid().ToString("N") : editingTvShow.id,
                    universeId = ActiveUniverseSession.UniverseId, name = tvShowNameInput.text.Trim(),
                    frequency = tvShowFrequencyDropdown.options[tvShowFrequencyDropdown.value].text,
                    dayOfWeek = tvShowDayDropdown.options[tvShowDayDropdown.value].text,
                    createdUtc = editingTvShow == null ? DateTime.UtcNow.ToString("O") : editingTvShow.createdUtc,
                    imagePath = editingTvShow == null ? string.Empty : editingTvShow.imagePath,
                    brandIds = new List<string>(selectedTvShowBrandIds) };
                if (!string.IsNullOrEmpty(selectedTvShowImagePath)) show.imagePath = UniverseImageStorage.Import(show.universeId, selectedTvShowImagePath, "tvshow_" + show.id);
                repository.SaveTvShow(show); tvShowBrandMenu.SetActive(false); ShowTvShows();
            }
            catch (Exception exception) { Debug.LogException(exception); tvShowValidationText.text = "The TV show could not be saved. Check the Console."; }
        }

        private void RefreshTvShowCards()
        {
            if (repository == null || tvShowGrid == null) return;
            for (var index = tvShowGrid.childCount - 1; index >= 0; index--) Destroy(tvShowGrid.GetChild(index).gameObject);
            var shows = repository.LoadTvShows(ActiveUniverseSession.UniverseId);
            tvShowCountText.text = "TV SHOWS  /  " + shows.Count; emptyTvShowsText.gameObject.SetActive(shows.Count == 0);
            foreach (var show in shows)
            {
                var card = CreateRuntimePanel("TvShow_" + show.id, tvShowGrid, new Color32(14, 23, 40, 255), Vector2.zero, Vector2.one);
                var texture = UniverseImageStorage.LoadTexture(show.imagePath);
                if (texture != null) { loadedTextures.Add(texture); SetRuntimePhoto(card.transform, "ShowImage", texture, new Vector2(.04f, .42f), new Vector2(.96f, .95f)); }
                CreateRuntimeText("Name", card.transform, show.name.ToUpperInvariant(), 18, Color.white, TextAnchor.MiddleLeft,
                    new Vector2(.06f, .27f), new Vector2(.94f, .43f), FontStyle.Bold);
                CreateRuntimeText("Schedule", card.transform, show.frequency.ToUpperInvariant() + "  /  " + show.dayOfWeek.ToUpperInvariant(), 13,
                    new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft, new Vector2(.06f, .16f), new Vector2(.94f, .28f), FontStyle.Bold);
                var edit = CreateRuntimeButton("EditButton", card.transform, "EDIT", new Vector2(.06f, .025f), new Vector2(.94f, .14f),
                    new Color32(25, 45, 65, 255), Color.white); edit.onClick.AddListener(() => EditTvShow(show));
            }
        }

        private void ShowSpecialCreation()
        {
            SelectSection("MyUniverseButton", "CREATE SPECIAL", string.Empty); sectionContentText.gameObject.SetActive(false); specialCreationPanel.SetActive(true);
            editingSpecial = null; specialFormTitle.text = "CREATE SPECIAL"; specialSaveLabel.text = "CREATE SPECIAL";
            specialNameInput.text = string.Empty; specialMonthDropdown.value = 0; specialWeekDropdown.value = 0; specialDayDropdown.value = 0;
            selectedSpecialBrandIds.Clear(); selectedSpecialImagePath = string.Empty; specialImageStatus.text = "No image selected"; specialValidationText.text = string.Empty;
            var old = specialImageHost.transform.Find("ImagePreview"); if (old != null) Destroy(old.gameObject); BuildSpecialBrandMenu();
        }

        private void EditSpecial(SpecialRecord special)
        {
            ShowSpecialCreation(); editingSpecial = special; specialFormTitle.text = "EDIT SPECIAL"; specialSaveLabel.text = "SAVE CHANGES";
            specialNameInput.text = special.name; SetDropdownValue(specialMonthDropdown, special.month); SetDropdownValue(specialWeekDropdown, special.week);
            SetDropdownValue(specialDayDropdown, special.dayOfWeek); selectedSpecialBrandIds.Clear(); selectedSpecialBrandIds.AddRange(special.brandIds); BuildSpecialBrandMenu();
            specialImageStatus.text = string.IsNullOrEmpty(special.imagePath) ? "No image selected" : System.IO.Path.GetFileName(special.imagePath);
            var texture = UniverseImageStorage.LoadTexture(special.imagePath);
            if (texture != null) { loadedTextures.Add(texture); SetRuntimePhoto(specialImageHost.transform, "ImagePreview", texture, new Vector2(.05f, .25f), new Vector2(.95f, .95f)); }
        }

        private void ToggleSpecialBrandMenu() => specialBrandMenu.SetActive(!specialBrandMenu.activeSelf);

        private void BuildSpecialBrandMenu()
        {
            availableSpecialBrands = repository.LoadBrands(ActiveUniverseSession.UniverseId);
            for (var index = specialBrandMenu.transform.childCount - 1; index >= 0; index--) Destroy(specialBrandMenu.transform.GetChild(index).gameObject);
            var count = Mathf.Max(1, availableSpecialBrands.Count); var height = 1f / count;
            for (var index = 0; index < availableSpecialBrands.Count; index++)
            {
                var brand = availableSpecialBrands[index]; var top = 1f - index * height; var bottom = top - height;
                var selected = selectedSpecialBrandIds.Contains(brand.id);
                var row = CreateRuntimeButton("Brand_" + brand.id, specialBrandMenu.transform, (selected ? "✓  " : "     ") + brand.name,
                    new Vector2(.02f, bottom), new Vector2(.98f, top), selected ? new Color32(25, 65, 82, 255) : new Color32(9, 15, 29, 255), Color.white);
                row.onClick.AddListener(() => ToggleSpecialBrand(brand.id));
            }
            if (availableSpecialBrands.Count == 0)
                CreateRuntimeText("NoBrands", specialBrandMenu.transform, "Create a Brand first", 14, new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
            var names = new List<string>(); foreach (var id in selectedSpecialBrandIds)
            { var brand = availableSpecialBrands.Find(item => item.id == id); if (brand != null) names.Add(brand.name); }
            specialBrandCaption.text = names.Count == 0 ? "Select one or more brands" : string.Join(", ", names.ToArray());
        }

        private void ToggleSpecialBrand(string brandId)
        {
            if (selectedSpecialBrandIds.Contains(brandId)) selectedSpecialBrandIds.Remove(brandId); else selectedSpecialBrandIds.Add(brandId);
            BuildSpecialBrandMenu(); specialBrandMenu.SetActive(true);
        }

        private void PickSpecialImage()
        {
            string path; if (!WindowsImageFilePicker.TryPickImage(out path)) return;
            var texture = UniverseImageStorage.LoadTexture(path);
            if (texture == null) { specialValidationText.text = "Unity could not decode that image."; return; }
            loadedTextures.Add(texture); selectedSpecialImagePath = path; specialImageStatus.text = System.IO.Path.GetFileName(path);
            SetRuntimePhoto(specialImageHost.transform, "ImagePreview", texture, new Vector2(.05f, .25f), new Vector2(.95f, .95f));
        }

        private void SaveSpecial()
        {
            if (string.IsNullOrWhiteSpace(specialNameInput.text)) { specialValidationText.text = "Special name is required."; return; }
            if (selectedSpecialBrandIds.Count == 0) { specialValidationText.text = "Select at least one Participating Brand."; return; }
            try
            {
                var special = new SpecialRecord { id = editingSpecial == null ? Guid.NewGuid().ToString("N") : editingSpecial.id,
                    universeId = ActiveUniverseSession.UniverseId, name = specialNameInput.text.Trim(),
                    month = specialMonthDropdown.options[specialMonthDropdown.value].text,
                    week = specialWeekDropdown.options[specialWeekDropdown.value].text,
                    dayOfWeek = specialDayDropdown.options[specialDayDropdown.value].text,
                    createdUtc = editingSpecial == null ? DateTime.UtcNow.ToString("O") : editingSpecial.createdUtc,
                    imagePath = editingSpecial == null ? string.Empty : editingSpecial.imagePath,
                    brandIds = new List<string>(selectedSpecialBrandIds) };
                if (!string.IsNullOrEmpty(selectedSpecialImagePath)) special.imagePath = UniverseImageStorage.Import(special.universeId, selectedSpecialImagePath, "special_" + special.id);
                repository.SaveSpecial(special); specialBrandMenu.SetActive(false); ShowSpecials();
            }
            catch (Exception exception) { Debug.LogException(exception); specialValidationText.text = "The special could not be saved. Check the Console."; }
        }

        private void RefreshSpecialCards()
        {
            if (repository == null || specialGrid == null) return;
            for (var index = specialGrid.childCount - 1; index >= 0; index--) Destroy(specialGrid.GetChild(index).gameObject);
            var specials = repository.LoadSpecials(ActiveUniverseSession.UniverseId);
            specialCountText.text = "SPECIALS  /  " + specials.Count; emptySpecialsText.gameObject.SetActive(specials.Count == 0);
            foreach (var special in specials)
            {
                var card = CreateRuntimePanel("Special_" + special.id, specialGrid, new Color32(14, 23, 40, 255), Vector2.zero, Vector2.one);
                var texture = UniverseImageStorage.LoadTexture(special.imagePath);
                if (texture != null) { loadedTextures.Add(texture); SetRuntimePhoto(card.transform, "SpecialImage", texture, new Vector2(.04f, .42f), new Vector2(.96f, .95f)); }
                CreateRuntimeText("Name", card.transform, special.name.ToUpperInvariant(), 18, Color.white, TextAnchor.MiddleLeft,
                    new Vector2(.06f, .27f), new Vector2(.94f, .43f), FontStyle.Bold);
                CreateRuntimeText("Month", card.transform, special.month.ToUpperInvariant(), 13, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                    new Vector2(.06f, .16f), new Vector2(.94f, .28f), FontStyle.Bold);
                var edit = CreateRuntimeButton("EditButton", card.transform, "EDIT", new Vector2(.06f, .025f), new Vector2(.94f, .14f),
                    new Color32(25, 45, 65, 255), Color.white); edit.onClick.AddListener(() => EditSpecial(special));
            }
        }

        private void InitializeCalendarDate(string startDate)
        {
            DateTime parsed;
            if (!DateTime.TryParseExact(startDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out parsed)) parsed = DateTime.Today;
            calendarYear = parsed.Year;
            if (calendarYearInput != null) calendarYearInput.text = calendarYear.ToString();
            if (calendarMonthDropdown != null)
            {
                calendarMonthDropdown.value = parsed.Month - 1;
                calendarMonthDropdown.RefreshShownValue();
            }
        }

        private void ChangeCalendarYear(int amount)
        {
            calendarYear = Mathf.Clamp(calendarYear + amount, 1, 9999);
            calendarYearInput.text = calendarYear.ToString();
            RefreshCalendar();
        }

        private void ApplyCalendarYearInput()
        {
            int enteredYear;
            if (!int.TryParse(calendarYearInput.text, out enteredYear) || enteredYear < 1 || enteredYear > 9999)
                enteredYear = calendarYear;
            calendarYear = enteredYear;
            calendarYearInput.text = calendarYear.ToString();
            RefreshCalendar();
        }

        private void RefreshCalendar()
        {
            if (repository == null || calendarCells.Count != 28) return;
            var selectedMonth = calendarMonthDropdown.options[calendarMonthDropdown.value].text;
            if (calendarHeadingText != null) calendarHeadingText.text = selectedMonth.ToUpperInvariant() + "  " + calendarYear;
            foreach (var cell in calendarCells)
                for (var index = cell.childCount - 1; index >= 0; index--) Destroy(cell.GetChild(index).gameObject);

            foreach (var show in repository.LoadTvShows(ActiveUniverseSession.UniverseId))
            {
                var day = CalendarDayIndex(show.dayOfWeek);
                if (day < 0) continue;
                var weeks = CalendarWeeksForFrequency(show.frequency);
                foreach (var week in weeks) AddCalendarEvent(week, day, show.id, "TV Show", show.name, show.imagePath, false);
            }

            foreach (var special in repository.LoadSpecials(ActiveUniverseSession.UniverseId))
            {
                if (!string.Equals(special.month, selectedMonth, StringComparison.OrdinalIgnoreCase)) continue;
                var week = Array.IndexOf(MonthWeeks, special.week);
                var day = CalendarDayIndex(special.dayOfWeek);
                if (week >= 0 && day >= 0) AddCalendarEvent(week, day, special.id, "Special", special.name, special.imagePath, true);
            }
        }

        private void AddCalendarEvent(int week, int day, string sourceId, string sourceType, string eventName, string imagePath, bool isSpecial)
        {
            if (week < 0 || week > 3 || day < 0 || day > 6) return;
            var cell = calendarCells[week * 7 + day];
            var eventIndex = cell.childCount;
            var eventHeight = .30f;
            var maxY = .94f - eventIndex * (eventHeight + .04f);
            var minY = Mathf.Max(.02f, maxY - eventHeight);
            if (maxY <= .04f) return;
            var tile = CreateRuntimePanel("Event_" + eventIndex, cell, isSpecial ? new Color32(48, 37, 18, 255) : new Color32(16, 30, 43, 255),
                new Vector2(.035f, minY), new Vector2(.965f, maxY));
            var tileButton = tile.AddComponent<Button>(); tileButton.targetGraphic = tile.GetComponent<Image>();
            var selectedWeek = week; var selectedDay = day; var selectedSourceId = sourceId; var selectedSourceType = sourceType; var selectedName = eventName;
            tileButton.onClick.AddListener(() => OpenShowBooking(selectedSourceId, selectedSourceType, selectedName, selectedWeek, selectedDay));
            var texture = UniverseImageStorage.LoadTexture(imagePath);
            var textMin = .06f;
            if (texture != null)
            {
                loadedTextures.Add(texture);
                SetRuntimePhoto(tile.transform, "EventImage", texture, new Vector2(.035f, .10f), new Vector2(.28f, .90f));
                textMin = .31f;
            }
            CreateRuntimeText("EventName", tile.transform, eventName.ToUpperInvariant(), 10, isSpecial ? new Color32(240, 190, 42, 255) : Color.white,
                TextAnchor.MiddleLeft, new Vector2(textMin, .06f), new Vector2(.97f, .94f), FontStyle.Bold);
        }

        private void OpenShowBooking(string sourceId, string sourceType, string showName, int week, int day)
        {
            var month = calendarMonthDropdown.options[calendarMonthDropdown.value].text;
            var dayName = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" }[day];
            ActiveBookingSession.Begin(ActiveUniverseSession.UniverseId, sourceId, sourceType, showName, calendarYear, month, week + 1, dayName);
            SelectSection("BookingButton", "BOOK " + showName.ToUpperInvariant(), string.Empty);
            sectionContentText.gameObject.SetActive(false); showBookingPanel.SetActive(true);
            bookingShowNameText.text = showName.ToUpperInvariant();
            bookingScheduleText.text = sourceType.ToUpperInvariant() + "  /  " + month.ToUpperInvariant() + " " + calendarYear +
                                       "  /  WEEK " + (week + 1) + "  /  " + dayName.ToUpperInvariant();
            matchStipulationDropdown.value = 0; matchStipulationDropdown.RefreshShownValue();
            RefreshMatchFormats(); matchGenderDropdown.value = 0; matchGenderDropdown.RefreshShownValue();
            PopulateMatchTitleDropdown();
            LoadBookingRoster(sourceId, sourceType); selectedMatchParticipantIds.Clear(); RefreshMatchParticipants();
            editingBookedMatch = null; addMatchToCardLabel.text = "ADD TO CARD"; matchBookingValidationText.text = string.Empty;
            expandedBookedMatchIds.Clear(); RefreshBookedMatchCards();
            matchBookingExpanded = false; segmentBookingExpanded = false; RefreshBookingAccordionLayout();
            if (bookingAccordionScroll != null) bookingAccordionScroll.verticalNormalizedPosition = 1f;
        }

        private void PopulateMatchTitleDropdown()
        {
            matchTitleOptions = repository.LoadTitles(ActiveUniverseSession.UniverseId);
            var options = new List<string> { "None" };
            foreach (var title in matchTitleOptions) options.Add(title.name);
            matchTitleDropdown.ClearOptions(); matchTitleDropdown.AddOptions(options);
            matchTitleDropdown.value = 0; matchTitleDropdown.RefreshShownValue();
        }

        private void LoadBookingRoster(string sourceId, string sourceType)
        {
            activeBookingBrandNames = new List<string>();
            if (sourceType == "TV Show")
            {
                var show = repository.LoadTvShows(ActiveUniverseSession.UniverseId).Find(item => item.id == sourceId);
                if (show != null) activeBookingBrandNames.AddRange(show.brandNames);
            }
            else
            {
                var special = repository.LoadSpecials(ActiveUniverseSession.UniverseId).Find(item => item.id == sourceId);
                if (special != null) activeBookingBrandNames.AddRange(special.brandNames);
            }
        }

        private void RefreshMatchFormats()
        {
            var stipulation = matchStipulationDropdown.options[matchStipulationDropdown.value].text;
            var formats = stipulation == "Tag Team" ? new List<string>(TagTeamMatchFormats) :
                stipulation == "Falls Count Anywhere" ? new List<string>(FallsCountAnywhereMatchFormats) :
                stipulation == "Steel Cage" ? new List<string>(SteelCageMatchFormats) :
                stipulation == "Hell in a Cell" ? new List<string>(HellInACellMatchFormats) :
                stipulation == "Table Match" ? new List<string>(TableMatchFormats) :
                stipulation == "Ladder Match" ? new List<string>(LadderMatchFormats) :
                (stipulation == "Tables, Ladders, and Chairs" || stipulation == "Tables, Ladders and Chairs Match") ?
                    new List<string>(TablesLaddersChairsMatchFormats) :
                (stipulation == "Submission Match" || stipulation == "Last Man Standing" || stipulation == "No Holds Barred" ||
                 stipulation == "Iron Man Match" || stipulation == "Casket Match" || stipulation == "Ambulance Match" ||
                 stipulation == "Dumpster Match" || stipulation == "I Quit Match" || stipulation == "Inferno Match" ||
                stipulation == "Underground Match") ?
                    new List<string>(OneOnOneOnlyMatchFormats) :
                stipulation == "3 Stages of Hell" ? new List<string>(OneOnOneOnlyMatchFormats) :
                stipulation == "Backstage Brawl" ? new List<string>(BackstageBrawlMatchFormats) :
                stipulation == "Gauntlet Match" ? new List<string>(GauntletMatchFormats) :
                stipulation == "Money in the Bank" ? new List<string>(MoneyInTheBankMatchFormats) :
                stipulation == "Elimination Chamber" ? new List<string>(SixWayOnlyMatchFormats) :
                stipulation == "War Games" ? new List<string>(WarGamesMatchFormats) :
                stipulation == "Extreme Rules" ?
                    new List<string>(ExtremeRulesMatchFormats) : new List<string>(MatchFormats);
            matchFormatDropdown.ClearOptions(); matchFormatDropdown.AddOptions(formats); matchFormatDropdown.value = 0; matchFormatDropdown.RefreshShownValue();
            SetThreeStagesControlsVisible(stipulation == "3 Stages of Hell");
            selectedMatchParticipantIds.Clear(); RefreshMatchParticipants();
        }

        private void SetThreeStagesControlsVisible(bool visible)
        {
            if (matchStagesGroup == null) return;
            matchStagesGroup.SetActive(visible);
            if (matchBookingBodyLayout != null) matchBookingBodyLayout.preferredHeight = visible ? 620f : 500f;
            if (matchParticipantSelector != null)
            {
                var rect = matchParticipantSelector.GetComponent<RectTransform>();
                rect.anchorMin = visible ? new Vector2(.035f, .37f) : new Vector2(.035f, .23f);
                rect.anchorMax = visible ? new Vector2(.965f, .49f) : new Vector2(.965f, .45f);
                rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            }
            var validationRect = matchBookingValidationText == null ? null : matchBookingValidationText.rectTransform;
            if (validationRect != null)
            {
                validationRect.anchorMin = visible ? new Vector2(.035f, .105f) : new Vector2(.035f, .15f);
                validationRect.anchorMax = visible ? new Vector2(.72f, .15f) : new Vector2(.72f, .22f);
                validationRect.offsetMin = Vector2.zero; validationRect.offsetMax = Vector2.zero;
            }
            var buttonRect = addMatchToCardButton == null ? null : addMatchToCardButton.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                buttonRect.anchorMin = visible ? new Vector2(.035f, .015f) : new Vector2(.035f, .025f);
                buttonRect.anchorMax = visible ? new Vector2(.965f, .095f) : new Vector2(.965f, .14f);
                buttonRect.offsetMin = Vector2.zero; buttonRect.offsetMax = Vector2.zero;
            }
            Canvas.ForceUpdateCanvases();
            if (bookingAccordionContent != null) LayoutRebuilder.ForceRebuildLayoutImmediate(bookingAccordionContent);
        }

        private void HandleMatchFormatChanged()
        {
            var needed = RequiredMatchParticipants();
            while (selectedMatchParticipantIds.Count > needed) selectedMatchParticipantIds.RemoveAt(selectedMatchParticipantIds.Count - 1);
            RefreshMatchParticipants();
        }

        private int RequiredMatchParticipants()
        {
            if (matchFormatDropdown == null || matchFormatDropdown.options.Count == 0) return 2;
            var format = matchFormatDropdown.options[matchFormatDropdown.value].text;
            if (format == "Triple Threat") return 3;
            if (format == "Fatal 4-Way" || format == "Two on Two" || format == "Two on Two - Mixed Tag" ||
                format == "Two on Two - Tornado Tag" || format == "Handicap - One on Three") return 4;
            if (format == "5-Way") return 5;
            if (format == "6-Way" || format == "Three on Three" || format == "Three on Three - Tornado Tag" ||
                format == "Triple Threat Tornado Tag") return 6;
            if (format == "8-Way" || format == "Four on Four" || format == "4-Way Tornado Tag") return 8;
            if (format == "Handicap - One on Two" || format == "Handicap - One on Two Tornado Tag") return 3;
            if (format == "Handicap - Two on Three") return 5;
            if (format == "4-Way Ladder") return 4;
            if (format == "5-Way Ladder") return 5;
            if (format == "8-Way Ladder") return 8;
            if (format.EndsWith(" Entrants", StringComparison.Ordinal))
            {
                int entrantCount;
                if (int.TryParse(format.Substring(0, format.IndexOf(' ')), out entrantCount)) return entrantCount;
            }
            return 2;
        }

        private void ToggleMatchParticipantMenu()
        {
            matchParticipantMenu.SetActive(!matchParticipantMenu.activeSelf);
            if (matchParticipantMenu.activeSelf) matchParticipantMenu.transform.SetAsLastSibling();
        }

        private void RefreshMatchParticipants()
        {
            if (repository == null || matchParticipantMenu == null || matchGenderDropdown == null) return;
            var gender = matchGenderDropdown.options[matchGenderDropdown.value].text;
            availableMatchParticipants = repository.LoadWrestlers(ActiveUniverseSession.UniverseId).FindAll(wrestler =>
                (activeBookingBrandNames.Count == 0 || activeBookingBrandNames.Contains(wrestler.brand)) &&
                (gender == "Both Genders" || wrestler.gender == gender));
            selectedMatchParticipantIds.RemoveAll(id => availableMatchParticipants.Find(wrestler => wrestler.id == id) == null);
            for (var index = matchParticipantMenu.transform.childCount - 1; index >= 0; index--) Destroy(matchParticipantMenu.transform.GetChild(index).gameObject);
            var count = Mathf.Max(1, availableMatchParticipants.Count); var rowHeight = 1f / count;
            for (var index = 0; index < availableMatchParticipants.Count; index++)
            {
                var wrestler = availableMatchParticipants[index]; var top = 1f - index * rowHeight; var bottom = top - rowHeight;
                var selected = selectedMatchParticipantIds.Contains(wrestler.id);
                var row = CreateRuntimeButton("Participant_" + wrestler.id, matchParticipantMenu.transform, (selected ? "✓  " : "     ") + wrestler.name,
                    new Vector2(.015f, bottom), new Vector2(.985f, top), selected ? new Color32(25, 65, 82, 255) : new Color32(9, 15, 29, 255), Color.white);
                row.onClick.AddListener(() => ToggleMatchParticipant(wrestler.id));
            }
            if (availableMatchParticipants.Count == 0)
                CreateRuntimeText("NoParticipants", matchParticipantMenu.transform, "No eligible wrestlers for this show and filter", 13,
                    new Color32(142, 160, 181, 255), TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
            UpdateMatchParticipantCaption();
        }

        private void ToggleMatchParticipant(string wrestlerId)
        {
            if (selectedMatchParticipantIds.Contains(wrestlerId)) selectedMatchParticipantIds.Remove(wrestlerId);
            else if (selectedMatchParticipantIds.Count < RequiredMatchParticipants()) selectedMatchParticipantIds.Add(wrestlerId);
            UpdateMatchParticipantCaption(); RefreshMatchParticipants(); matchParticipantMenu.SetActive(true);
        }

        private void UpdateMatchParticipantCaption()
        {
            var needed = RequiredMatchParticipants(); var names = new List<string>();
            foreach (var id in selectedMatchParticipantIds)
            { var wrestler = availableMatchParticipants.Find(item => item.id == id); if (wrestler != null) names.Add(wrestler.name); }
            matchParticipantCaption.text = names.Count == 0 ? "Select " + needed + " wrestlers" :
                string.Join(", ", names.ToArray()) + "  (" + names.Count + "/" + needed + ")";
            if (addMatchToCardButton != null) addMatchToCardButton.interactable = names.Count == needed;
        }

        private void AddMatchToCard()
        {
            var needed = RequiredMatchParticipants();
            if (selectedMatchParticipantIds.Count != needed)
            { matchBookingValidationText.text = "Select exactly " + needed + " wrestlers for this format."; return; }
            try
            {
                var current = repository.LoadBookedMatches(ActiveBookingSession.UniverseId, ActiveBookingSession.SourceId,
                    ActiveBookingSession.Year, ActiveBookingSession.Month, ActiveBookingSession.Week, ActiveBookingSession.DayOfWeek);
                var match = new BookedMatchRecord {
                    id = editingBookedMatch == null ? Guid.NewGuid().ToString("N") : editingBookedMatch.id,
                    universeId = ActiveBookingSession.UniverseId, sourceId = ActiveBookingSession.SourceId, sourceType = ActiveBookingSession.SourceType,
                    year = ActiveBookingSession.Year, month = ActiveBookingSession.Month, week = ActiveBookingSession.Week,
                    dayOfWeek = ActiveBookingSession.DayOfWeek, cardPosition = editingBookedMatch == null ? current.Count + 1 : editingBookedMatch.cardPosition,
                    stipulation = matchStipulationDropdown.options[matchStipulationDropdown.value].text,
                    format = matchFormatDropdown.options[matchFormatDropdown.value].text,
                    titleId = matchTitleDropdown.value == 0 ? string.Empty : matchTitleOptions[matchTitleDropdown.value - 1].id,
                    stageOneStipulation = matchStagesGroup.activeSelf ? matchStageOneDropdown.options[matchStageOneDropdown.value].text : string.Empty,
                    stageTwoStipulation = matchStagesGroup.activeSelf ? matchStageTwoDropdown.options[matchStageTwoDropdown.value].text : string.Empty,
                    stageThreeStipulation = matchStagesGroup.activeSelf ? matchStageThreeDropdown.options[matchStageThreeDropdown.value].text : string.Empty,
                    createdUtc = editingBookedMatch == null ? DateTime.UtcNow.ToString("O") : editingBookedMatch.createdUtc,
                    participantIds = new List<string>(selectedMatchParticipantIds) };
                repository.SaveBookedMatch(match); editingBookedMatch = null; selectedMatchParticipantIds.Clear();
                addMatchToCardLabel.text = "ADD TO CARD"; matchBookingValidationText.text = string.Empty; matchBookingExpanded = false;
                RefreshMatchParticipants(); RefreshBookedMatchCards(); RefreshBookingAccordionLayout();
                Canvas.ForceUpdateCanvases(); if (bookingAccordionScroll != null) bookingAccordionScroll.verticalNormalizedPosition = 0f;
            }
            catch (Exception exception) { Debug.LogException(exception); matchBookingValidationText.text = "The match could not be added. Check the Console."; }
        }

        private void RefreshBookedMatchCards()
        {
            if (repository == null || bookedMatchCardList == null || string.IsNullOrEmpty(ActiveBookingSession.SourceId)) return;
            for (var index = bookedMatchCardList.childCount - 1; index >= 0; index--) Destroy(bookedMatchCardList.GetChild(index).gameObject);
            var matches = repository.LoadBookedMatches(ActiveBookingSession.UniverseId, ActiveBookingSession.SourceId,
                ActiveBookingSession.Year, ActiveBookingSession.Month, ActiveBookingSession.Week, ActiveBookingSession.DayOfWeek);
            var heading = CreateRuntimeText("MatchCardHeading", bookedMatchCardList, "MATCH CARD  (" + matches.Count + ")", 15,
                new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft, Vector2.zero, Vector2.one, FontStyle.Bold);
            heading.gameObject.AddComponent<LayoutElement>().preferredHeight = 46;
            var totalHeight = 46f;
            foreach (var match in matches)
            {
                var expanded = expandedBookedMatchIds.Contains(match.id); var height = expanded ? 330f : 68f; totalHeight += height + 12;
                var card = CreateRuntimePanel("BookedMatch_" + match.id, bookedMatchCardList, new Color32(7, 12, 22, 255), Vector2.zero, Vector2.one);
                card.AddComponent<LayoutElement>().preferredHeight = height;
                var header = CreateRuntimeButton("Header", card.transform, string.Empty, new Vector2(0, expanded ? .80f : 0), Vector2.one,
                    new Color32(7, 12, 22, 255), Color.white);
                var oldLabel = header.transform.Find("Label"); if (oldLabel != null) Destroy(oldLabel.gameObject);
                CreateRuntimeText("Title", header.transform, "#" + match.cardPosition + "  " + BuildMatchupLabel(match) + "  [" +
                    match.stipulation.ToUpperInvariant() + " - " + match.format.ToUpperInvariant() + "]",
                    15, Color.white, TextAnchor.MiddleLeft, new Vector2(.035f, .08f), new Vector2(.90f, .92f), FontStyle.Bold);
                CreateRuntimeText("Arrow", header.transform, expanded ? "▲" : "▼", 15, new Color32(190, 198, 210, 255), TextAnchor.MiddleCenter,
                    new Vector2(.92f, .08f), new Vector2(.98f, .92f), FontStyle.Bold);
                header.onClick.AddListener(() => ToggleBookedMatchCard(match.id));
                if (!expanded) continue;
                var body = CreateRuntimePanel("Body", card.transform, new Color32(10, 16, 27, 255), new Vector2(.015f, .03f), new Vector2(.985f, .78f));
                if (match.stipulation == "3 Stages of Hell")
                    CreateRuntimeText("Stages", body.transform, "STAGE 1: " + match.stageOneStipulation.ToUpperInvariant() + "    /    STAGE 2: " +
                        match.stageTwoStipulation.ToUpperInvariant() + "    /    STAGE 3: " + match.stageThreeStipulation.ToUpperInvariant(),
                        11, new Color32(255, 92, 92, 255), TextAnchor.MiddleCenter, new Vector2(.03f, .87f), new Vector2(.97f, .98f), FontStyle.Bold);
                var participantCount = Mathf.Max(1, match.participants.Count); var slotWidth = .94f / participantCount;
                for (var index = 0; index < match.participants.Count; index++)
                {
                    var wrestler = match.participants[index]; var texture = UniverseImageStorage.LoadTexture(wrestler.photoPath);
                    var left = .03f + index * slotWidth; var right = left + slotWidth;
                    if (texture != null) { loadedTextures.Add(texture); SetRuntimePhoto(body.transform, "Wrestler_" + index, texture,
                        new Vector2(left + .01f, .23f), new Vector2(right - .01f, .95f)); }
                    CreateRuntimeText("Name_" + index, body.transform, wrestler.name.ToUpperInvariant(), 12, Color.white, TextAnchor.MiddleCenter,
                        new Vector2(left, .16f), new Vector2(right, .26f), FontStyle.Bold);
                    var teamSplit = MatchTeamSplitIndex(match.format);
                    var showVs = index < match.participants.Count - 1 && (teamSplit == 0 || index + 1 == teamSplit);
                    if (showVs) CreateRuntimeText("Vs_" + index, body.transform, "VS", 13, new Color32(240, 190, 42, 255),
                        TextAnchor.MiddleCenter, new Vector2(right - .025f, .45f), new Vector2(right + .025f, .58f), FontStyle.Bold);
                }
                var edit = CreateRuntimeButton("Edit", body.transform, "EDIT", new Vector2(.03f, .025f), new Vector2(.28f, .14f),
                    new Color32(25, 45, 65, 255), Color.white); edit.onClick.AddListener(() => EditBookedMatch(match));
                var delete = CreateRuntimeButton("Delete", body.transform, "DELETE MATCH", new Vector2(.72f, .025f), new Vector2(.97f, .14f),
                    new Color32(84, 31, 39, 255), Color.white); delete.onClick.AddListener(() => DeleteBookedMatch(match.id));
            }
            bookedMatchCardListLayout.preferredHeight = Mathf.Max(58, totalHeight);
            Canvas.ForceUpdateCanvases(); LayoutRebuilder.ForceRebuildLayoutImmediate(bookingAccordionContent);
        }

        private static string BuildMatchupLabel(BookedMatchRecord match)
        {
            var names = new List<string>(); foreach (var wrestler in match.participants) names.Add(wrestler.name.ToUpperInvariant());
            var split = MatchTeamSplitIndex(match.format);
            if (split <= 0 || split >= names.Count) return string.Join("  VS  ", names.ToArray());
            return string.Join(" AND ", names.GetRange(0, split).ToArray()) + "  VS  " +
                   string.Join(" AND ", names.GetRange(split, names.Count - split).ToArray());
        }

        private static int MatchTeamSplitIndex(string format)
        {
            if (format == "Two on Two" || format == "Two on Two - Mixed Tag" || format == "Two on Two - Tornado Tag" ||
                format == "Handicap - Two on Three") return 2;
            if (format == "Three on Three" || format == "Three on Three - Tornado Tag" || format == "Triple Threat Tornado Tag") return 3;
            if (format == "Four on Four" || format == "4-Way Tornado Tag") return 4;
            if (format == "Handicap - One on Two" || format == "Handicap - One on Two Tornado Tag" || format == "Handicap - One on Three") return 1;
            return 0;
        }

        private void ToggleBookedMatchCard(string matchId)
        {
            if (!expandedBookedMatchIds.Add(matchId)) expandedBookedMatchIds.Remove(matchId);
            RefreshBookedMatchCards();
        }

        private void EditBookedMatch(BookedMatchRecord match)
        {
            editingBookedMatch = match; matchBookingExpanded = true; segmentBookingExpanded = false;
            var savedStipulation = match.stipulation == "Tables, Ladders and Chairs Match" ? "Tables, Ladders, and Chairs" : match.stipulation;
            SetDropdownValue(matchStipulationDropdown, savedStipulation); RefreshMatchFormats(); SetDropdownValue(matchFormatDropdown, match.format);
            if (savedStipulation == "3 Stages of Hell")
            {
                SetDropdownValue(matchStageOneDropdown, match.stageOneStipulation);
                SetDropdownValue(matchStageTwoDropdown, match.stageTwoStipulation);
                SetDropdownValue(matchStageThreeDropdown, match.stageThreeStipulation);
            }
            matchGenderDropdown.value = 0; matchGenderDropdown.RefreshShownValue(); RefreshMatchParticipants();
            selectedMatchParticipantIds.Clear(); selectedMatchParticipantIds.AddRange(match.participantIds); RefreshMatchParticipants();
            var titleIndex = matchTitleOptions.FindIndex(item => item.id == match.titleId); matchTitleDropdown.value = titleIndex < 0 ? 0 : titleIndex + 1;
            matchTitleDropdown.RefreshShownValue(); addMatchToCardLabel.text = "SAVE MATCH"; matchBookingValidationText.text = string.Empty;
            RefreshBookingAccordionLayout(); if (bookingAccordionScroll != null) bookingAccordionScroll.verticalNormalizedPosition = 1f;
        }

        private void DeleteBookedMatch(string matchId)
        {
            repository.DeleteBookedMatch(matchId); expandedBookedMatchIds.Remove(matchId);
            if (editingBookedMatch != null && editingBookedMatch.id == matchId) editingBookedMatch = null;
            RefreshBookedMatchCards(); RefreshBookingAccordionLayout();
        }

        private void ToggleMatchBooking()
        {
            matchBookingExpanded = !matchBookingExpanded;
            if (matchBookingExpanded) segmentBookingExpanded = false;
            if (!matchBookingExpanded && matchParticipantMenu != null) matchParticipantMenu.SetActive(false);
            RefreshBookingAccordionLayout();
            if (bookingAccordionScroll != null && matchBookingExpanded) bookingAccordionScroll.verticalNormalizedPosition = 1f;
        }

        private void ToggleSegmentBooking()
        {
            segmentBookingExpanded = !segmentBookingExpanded;
            if (segmentBookingExpanded) { matchBookingExpanded = false; if (matchParticipantMenu != null) matchParticipantMenu.SetActive(false); }
            RefreshBookingAccordionLayout();
            if (bookingAccordionScroll != null && segmentBookingExpanded) bookingAccordionScroll.verticalNormalizedPosition = 0f;
        }

        private void RefreshBookingAccordionLayout()
        {
            if (matchBookingHeader == null || segmentBookingHeader == null) return;
            matchBookingBody.SetActive(matchBookingExpanded); segmentBookingBody.SetActive(segmentBookingExpanded);
            matchBookingArrow.text = matchBookingExpanded ? "▲" : "▼";
            segmentBookingArrow.text = segmentBookingExpanded ? "▲" : "▼";
            Canvas.ForceUpdateCanvases();
            if (bookingAccordionContent != null) LayoutRebuilder.ForceRebuildLayoutImmediate(bookingAccordionContent);
        }

        private static int CalendarDayIndex(string day)
        {
            var mondayFirst = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            return Array.FindIndex(mondayFirst, item => string.Equals(item, day, StringComparison.OrdinalIgnoreCase));
        }

        private static int[] CalendarWeeksForFrequency(string frequency)
        {
            if (string.Equals(frequency, "Weekly", StringComparison.OrdinalIgnoreCase)) return new[] { 0, 1, 2, 3 };
            if (string.Equals(frequency, "Bi-Weekly", StringComparison.OrdinalIgnoreCase)) return new[] { 0, 2 };
            return new[] { 0 };
        }

        private void EditBrand(BrandRecord brand)
        {
            ShowBrandCreation(); editingBrand = brand; brandFormTitle.text = "EDIT BRAND"; brandSaveLabel.text = "SAVE CHANGES";
            brandNameInput.text = brand.name; brandColorInput.text = brand.colorHex;
            brandImageStatus.text = string.IsNullOrEmpty(brand.imagePath) ? "No image selected" : System.IO.Path.GetFileName(brand.imagePath);
            var texture = UniverseImageStorage.LoadTexture(brand.imagePath);
            if (texture != null) { loadedTextures.Add(texture); SetRuntimePhoto(brandImageHost.transform, "ImagePreview", texture, new Vector2(.05f, .27f), new Vector2(.95f, .95f)); }
        }

        private void PickBrandImage()
        {
            string path; if (!WindowsImageFilePicker.TryPickImage(out path)) return;
            var texture = UniverseImageStorage.LoadTexture(path);
            if (texture == null) { brandValidationText.text = "Unity could not decode that image."; return; }
            loadedTextures.Add(texture); selectedBrandImagePath = path; brandImageStatus.text = System.IO.Path.GetFileName(path);
            SetRuntimePhoto(brandImageHost.transform, "ImagePreview", texture, new Vector2(.05f, .27f), new Vector2(.95f, .95f));
        }

        private void SaveBrand()
        {
            Color parsedColor;
            var colorHex = brandColorInput.text.Trim(); if (!colorHex.StartsWith("#")) colorHex = "#" + colorHex;
            if (string.IsNullOrWhiteSpace(brandNameInput.text)) { brandValidationText.text = "Brand name is required."; return; }
            if (colorHex.Length != 7 || !ColorUtility.TryParseHtmlString(colorHex, out parsedColor))
            { brandValidationText.text = "Use a six-digit HEX color such as #E2231A."; return; }
            try
            {
                var brand = new BrandRecord { id = editingBrand == null ? Guid.NewGuid().ToString("N") : editingBrand.id,
                    universeId = ActiveUniverseSession.UniverseId, name = brandNameInput.text.Trim(), colorHex = colorHex.ToUpperInvariant(),
                    createdUtc = editingBrand == null ? DateTime.UtcNow.ToString("O") : editingBrand.createdUtc,
                    imagePath = editingBrand == null ? string.Empty : editingBrand.imagePath };
                if (!string.IsNullOrEmpty(selectedBrandImagePath))
                    brand.imagePath = UniverseImageStorage.Import(brand.universeId, selectedBrandImagePath, "brand_" + brand.id);
                repository.SaveBrand(brand);
                if (editingBrand != null) repository.RenameBrandAssignments(brand.universeId, editingBrand.name, brand.name);
                ShowBrands();
            }
            catch (Exception exception) { Debug.LogException(exception); brandValidationText.text = "The brand could not be saved. Brand names must be unique."; }
        }

        private void ShowBrandInfo(BrandRecord brand)
        {
            SelectSection("MyUniverseButton", "BRAND INFO", string.Empty); sectionContentText.gameObject.SetActive(false); brandInfoPanel.SetActive(true);
            brandInfoTitle.text = brand.name.ToUpperInvariant() + "  /  ASSIGNED ROSTER";
            var wrestlers = repository.LoadWrestlers(ActiveUniverseSession.UniverseId).FindAll(item => item.brand == brand.name);
            if (wrestlers.Count == 0) brandInfoRoster.text = "NO ROSTER MEMBERS ASSIGNED TO THIS BRAND";
            else
            {
                var lines = new List<string>(); foreach (var wrestler in wrestlers)
                    lines.Add(wrestler.name + "    /    " + wrestler.disposition + "    /    " + wrestler.tier + "    /    OVR " + wrestler.overall);
                brandInfoRoster.text = string.Join("\n\n", lines.ToArray());
            }
        }

        private void RefreshBrandCards()
        {
            if (repository == null || brandGrid == null) return;
            for (var index = brandGrid.childCount - 1; index >= 0; index--) Destroy(brandGrid.GetChild(index).gameObject);
            var brands = repository.LoadBrands(ActiveUniverseSession.UniverseId);
            brandCountText.text = "BRANDS  /  " + brands.Count; emptyBrandsText.gameObject.SetActive(brands.Count == 0);
            foreach (var brand in brands)
            {
                Color accent; if (!ColorUtility.TryParseHtmlString(brand.colorHex, out accent)) accent = new Color32(45, 190, 230, 255);
                var card = CreateRuntimePanel("Brand_" + brand.id, brandGrid, new Color(accent.r * .18f, accent.g * .18f, accent.b * .18f, 1), Vector2.zero, Vector2.one);
                var texture = UniverseImageStorage.LoadTexture(brand.imagePath);
                if (texture != null) { loadedTextures.Add(texture); SetRuntimePhoto(card.transform, "BrandImage", texture, new Vector2(.05f, .38f), new Vector2(.95f, .95f)); }
                CreateRuntimeText("Name", card.transform, brand.name.ToUpperInvariant(), 19, Color.white, TextAnchor.MiddleCenter,
                    new Vector2(.05f, .24f), new Vector2(.95f, .39f), FontStyle.Bold);
                var edit = CreateRuntimeButton("EditButton", card.transform, "EDIT", new Vector2(.05f, .04f), new Vector2(.48f, .20f),
                    new Color32(25, 45, 65, 255), Color.white); edit.onClick.AddListener(() => EditBrand(brand));
                var info = CreateRuntimeButton("InfoButton", card.transform, "INFO", new Vector2(.52f, .04f), new Vector2(.95f, .20f),
                    new Color32(25, 45, 65, 255), Color.white); info.onClick.AddListener(() => ShowBrandInfo(brand));
            }
        }

        private void PopulateBrandDropdown(Dropdown dropdown, string selected = "Unassigned")
        {
            var options = new List<string> { "Unassigned" };
            foreach (var brand in repository.LoadBrands(ActiveUniverseSession.UniverseId)) options.Add(brand.name);
            dropdown.ClearOptions(); dropdown.AddOptions(options); SetDropdownValue(dropdown, selected);
        }

        private void EditLocation(LocationRecord location)
        {
            ShowLocationCreation(); editingLocation = location; locationFormTitle.text = "EDIT LOCATION"; locationSaveLabel.text = "SAVE CHANGES";
            venueNameInput.text = location.venueName; venueLocationInput.text = location.venueLocation; venueCapacityInput.text = location.capacity.ToString();
        }

        private void SaveLocation()
        {
            int capacity;
            if (string.IsNullOrWhiteSpace(venueNameInput.text) || string.IsNullOrWhiteSpace(venueLocationInput.text))
            { locationValidationText.text = "Venue name and location are required."; return; }
            if (!int.TryParse(venueCapacityInput.text, out capacity) || capacity < 0)
            { locationValidationText.text = "Venue capacity must be a valid non-negative number."; return; }
            try
            {
                var location = new LocationRecord { id = editingLocation == null ? Guid.NewGuid().ToString("N") : editingLocation.id,
                    universeId = ActiveUniverseSession.UniverseId, venueName = venueNameInput.text.Trim(), venueLocation = venueLocationInput.text.Trim(),
                    capacity = capacity, createdUtc = editingLocation == null ? DateTime.UtcNow.ToString("O") : editingLocation.createdUtc };
                repository.SaveLocation(location); ShowLocations();
            }
            catch (Exception exception) { Debug.LogException(exception); locationValidationText.text = "The location could not be saved. Check the Console."; }
        }

        private void RefreshLocationCards()
        {
            if (repository == null || locationGrid == null) return;
            for (var index = locationGrid.childCount - 1; index >= 0; index--) Destroy(locationGrid.GetChild(index).gameObject);
            var locations = repository.LoadLocations(ActiveUniverseSession.UniverseId);
            locationCountText.text = "LOCATIONS  /  " + locations.Count; emptyLocationsText.gameObject.SetActive(locations.Count == 0);
            foreach (var location in locations)
            {
                var card = CreateRuntimePanel("Location_" + location.id, locationGrid, new Color32(14, 23, 40, 255), Vector2.zero, Vector2.one);
                CreateRuntimeText("Name", card.transform, location.venueName.ToUpperInvariant(), 19, Color.white, TextAnchor.MiddleLeft,
                    new Vector2(.06f, .63f), new Vector2(.94f, .92f), FontStyle.Bold);
                CreateRuntimeText("Location", card.transform, location.venueLocation.ToUpperInvariant(), 14, new Color32(45, 190, 230, 255), TextAnchor.MiddleLeft,
                    new Vector2(.06f, .43f), new Vector2(.94f, .63f), FontStyle.Bold);
                CreateRuntimeText("Capacity", card.transform, "CAPACITY  /  " + location.capacity.ToString("N0"), 14,
                    new Color32(142, 160, 181, 255), TextAnchor.MiddleLeft, new Vector2(.06f, .22f), new Vector2(.94f, .43f));
                var edit = CreateRuntimeButton("EditButton", card.transform, "EDIT", new Vector2(.06f, .035f), new Vector2(.94f, .19f),
                    new Color32(25, 45, 65, 255), Color.white); edit.onClick.AddListener(() => EditLocation(location));
            }
        }

        private void EditTitle(TitleRecord title)
        {
            ShowTitleCreation(); editingTitle = title; titleFormTitle.text = "EDIT TITLE"; titleSaveLabel.text = "SAVE CHANGES";
            titleNameInput.text = title.name; PopulateBrandDropdown(titleBrandDropdown, title.brand); SetDropdownValue(titleDivisionDropdown, title.division);
            var holderIndex = string.IsNullOrEmpty(title.holderWrestlerId) ? -1 : titleHolderOptions.FindIndex(item => item.id == title.holderWrestlerId);
            titleHolderDropdown.value = holderIndex < 0 ? 0 : holderIndex + 1; titleHolderDropdown.RefreshShownValue();
            titleImageStatus.text = string.IsNullOrEmpty(title.imagePath) ? "No image selected" : System.IO.Path.GetFileName(title.imagePath);
            var texture = UniverseImageStorage.LoadTexture(title.imagePath);
            if (texture != null) { loadedTextures.Add(texture); SetRuntimePhoto(titleImageHost.transform, "ImagePreview", texture, new Vector2(.04f, .28f), new Vector2(.96f, .95f)); }
        }

        private void ShowTitleHistory(TitleRecord title)
        {
            SelectSection("RosterButton", "TITLE HISTORY", string.Empty);
            sectionContentText.gameObject.SetActive(false);
            titleHistoryNameText.text = title.name.ToUpperInvariant();
            titleHistoryPanel.SetActive(true);
        }

        private void PickTitleImage()
        {
            string path; if (!WindowsImageFilePicker.TryPickImage(out path)) return;
            var texture = UniverseImageStorage.LoadTexture(path);
            if (texture == null) { titleValidationText.text = "Unity could not decode that image."; return; }
            loadedTextures.Add(texture); selectedTitleImagePath = path; titleImageStatus.text = System.IO.Path.GetFileName(path);
            SetRuntimePhoto(titleImageHost.transform, "ImagePreview", texture, new Vector2(.04f, .28f), new Vector2(.96f, .95f));
        }

        private void SaveTitle()
        {
            if (string.IsNullOrWhiteSpace(titleNameInput.text)) { titleValidationText.text = "Title name is required."; return; }
            try
            {
                var record = new TitleRecord { id = editingTitle == null ? Guid.NewGuid().ToString("N") : editingTitle.id, universeId = ActiveUniverseSession.UniverseId,
                    name = titleNameInput.text.Trim(), brand = titleBrandDropdown.options[titleBrandDropdown.value].text,
                    division = titleDivisionDropdown.options[titleDivisionDropdown.value].text,
                    createdUtc = editingTitle == null ? DateTime.UtcNow.ToString("O") : editingTitle.createdUtc,
                    imagePath = editingTitle == null ? string.Empty : editingTitle.imagePath };
                if (titleHolderDropdown.value > 0) record.holderWrestlerId = titleHolderOptions[titleHolderDropdown.value - 1].id;
                if (!string.IsNullOrEmpty(selectedTitleImagePath))
                    record.imagePath = UniverseImageStorage.Import(record.universeId, selectedTitleImagePath, "title_" + record.id);
                repository.SaveTitle(record); ShowTitles();
            }
            catch (Exception exception) { Debug.LogException(exception); titleValidationText.text = "The title could not be saved. Check the Console."; }
        }

        private void RefreshTitleCards()
        {
            if (repository == null || titleGrid == null) return;
            for (var index = titleGrid.childCount - 1; index >= 0; index--) Destroy(titleGrid.GetChild(index).gameObject);
            var titles = repository.LoadTitles(ActiveUniverseSession.UniverseId);
            titleCountText.text = "TITLES  /  " + titles.Count; emptyTitlesText.gameObject.SetActive(titles.Count == 0);
            foreach (var title in titles)
            {
                var card = CreateRuntimePanel("Title_" + title.id, titleGrid, new Color32(14, 23, 40, 255), Vector2.zero, Vector2.one);
                var texture = UniverseImageStorage.LoadTexture(title.imagePath);
                if (texture != null) { loadedTextures.Add(texture); SetRuntimePhoto(card.transform, "BeltImage", texture, new Vector2(.04f, .50f), new Vector2(.96f, .94f)); }
                CreateRuntimeText("Name", card.transform, title.name.ToUpperInvariant(), 18, new Color32(240, 190, 42, 255), TextAnchor.MiddleCenter,
                    new Vector2(.05f, .31f), new Vector2(.95f, .50f), FontStyle.Bold);
                CreateRuntimeText("Division", card.transform, title.division.ToUpperInvariant(), 11, new Color32(45, 190, 230, 255), TextAnchor.MiddleCenter,
                    new Vector2(.05f, .23f), new Vector2(.95f, .32f), FontStyle.Bold);
                CreateRuntimeText("Holder", card.transform, "TITLE HOLDER  /  " + title.holderName.ToUpperInvariant(), 13,
                    title.holderWrestlerId.Length == 0 ? new Color32(142, 160, 181, 255) : Color.white,
                    TextAnchor.MiddleCenter, new Vector2(.05f, .13f), new Vector2(.95f, .23f), FontStyle.Bold);
                var edit = CreateRuntimeButton("EditButton", card.transform, "EDIT", new Vector2(.08f, .025f), new Vector2(.48f, .12f),
                    new Color32(25, 45, 65, 255), Color.white); edit.onClick.AddListener(() => EditTitle(title));
                var history = CreateRuntimeButton("HistoryButton", card.transform, "HISTORY", new Vector2(.52f, .025f), new Vector2(.92f, .12f),
                    new Color32(25, 45, 65, 255), Color.white); history.onClick.AddListener(() => ShowTitleHistory(title));
            }
        }

        private void ResetWrestlerForm()
        {
            editingWrestler = null;
            wrestlerFormTitle.text = "SIGN WRESTLER";
            wrestlerSaveLabel.text = "SIGN WRESTLER";
            wrestlerNameInput.text = string.Empty;
            overallInput.text = "75";
            PopulateBrandDropdown(brandDropdown);
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
            PopulateBrandDropdown(brandDropdown, wrestler.brand);
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
                SetRuntimePhoto(card.transform, "Portrait", texture, new Vector2(.04f, .34f), new Vector2(.96f, .97f));
            }
            CreateRuntimeText("Name", card.transform, wrestler.name.ToUpperInvariant(), 16, Color.white, TextAnchor.MiddleLeft,
                new Vector2(.06f, .22f), new Vector2(.72f, .34f), FontStyle.Bold);
            CreateRuntimeText("Overall", card.transform, wrestler.overall.ToString(), 22, new Color32(240, 190, 42, 255), TextAnchor.MiddleRight,
                new Vector2(.72f, .22f), new Vector2(.94f, .34f), FontStyle.Bold);
            CreateRuntimeText("Details", card.transform, wrestler.disposition.ToUpperInvariant() + "  /  " + wrestler.tier.ToUpperInvariant(), 11,
                new Color32(142, 160, 181, 255), TextAnchor.MiddleLeft, new Vector2(.06f, .13f), new Vector2(.94f, .22f));
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
            ApplyCompactWorkspaceLayout(background);
            var existingNavigation = background.Find("WorkspaceNavigation");
            if (existingNavigation != null)
            {
                EnsureRosterDropdown(existingNavigation);
                EnsureMyUniverseDropdown(existingNavigation);
                EnsureBookingDropdown(existingNavigation);
                return;
            }

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
            EnsureMyUniverseDropdown(bar.transform);
            EnsureBookingDropdown(bar.transform);
        }

        private void ApplyCompactWorkspaceLayout(Transform background)
        {
            var title = background.Find("Title");
            var owner = background.Find("Owner");
            var startDate = background.Find("StartDate");
            var workspace = background.Find("FeatureWorkspace");
            if (sectionTitleText != null) SetRuntimeRect(sectionTitleText.rectTransform, new Vector2(.04f, .735f), new Vector2(.6f, .785f));
            if (title != null)
            {
                SetRuntimeRect(title.GetComponent<RectTransform>(), new Vector2(.04f, .665f), new Vector2(.85f, .735f));
                var titleText = title.GetComponent<Text>(); if (titleText != null) titleText.fontSize = 36;
            }
            if (owner != null) SetRuntimeRect(owner.GetComponent<RectTransform>(), new Vector2(.04f, .605f), new Vector2(.46f, .665f));
            if (startDate != null) SetRuntimeRect(startDate.GetComponent<RectTransform>(), new Vector2(.48f, .605f), new Vector2(.9f, .665f));
            if (workspace != null) SetRuntimeRect(workspace.GetComponent<RectTransform>(), new Vector2(.04f, .055f), new Vector2(.96f, .585f));
        }

        private void EnsureBookingDropdown(Transform navigation)
        {
            navigation.SetAsLastSibling();
            var bookingButton = navigation.Find("BookingButton");
            if (bookingButton == null || bookingButton.Find("BookingDropdown") != null) return;

            var menu = CreateRuntimePanel("BookingDropdown", bookingButton, new Color32(5, 9, 20, 255),
                new Vector2(0, -1.55f), new Vector2(1, 0));
            menu.transform.SetAsLastSibling();
            var booking = CreateRuntimeButton("BookingMenuItem", menu.transform, "BOOKING", new Vector2(0, .53f), new Vector2(1, .96f),
                new Color32(9, 15, 29, 255), Color.white); booking.onClick.AddListener(ShowBooking);
            var calendar = CreateRuntimeButton("CalendarMenuItem", menu.transform, "CALENDAR", new Vector2(0, .04f), new Vector2(1, .47f),
                new Color32(9, 15, 29, 255), Color.white); calendar.onClick.AddListener(ShowCalendar);

            var hover = bookingButton.gameObject.AddComponent<NavigationHoverDropdown>();
            hover.Configure(menu);
            bookingButton.gameObject.AddComponent<NavigationHoverRelay>().Configure(hover);
            menu.AddComponent<NavigationHoverRelay>().Configure(hover);
        }

        private void EnsureMyUniverseDropdown(Transform navigation)
        {
            navigation.SetAsLastSibling();
            var universeButton = navigation.Find("MyUniverseButton");
            if (universeButton == null || universeButton.Find("MyUniverseDropdown") != null) return;

            var menu = CreateRuntimePanel("MyUniverseDropdown", universeButton, new Color32(5, 9, 20, 255),
                new Vector2(0, -2.95f), new Vector2(1, 0));
            menu.transform.SetAsLastSibling();
            var brands = CreateRuntimeButton("BrandsMenuItem", menu.transform, "BRANDS", new Vector2(0, .77f), new Vector2(1, .98f),
                new Color32(9, 15, 29, 255), Color.white); brands.onClick.AddListener(ShowBrands);
            var shows = CreateRuntimeButton("TvShowsMenuItem", menu.transform, "TV SHOWS", new Vector2(0, .52f), new Vector2(1, .73f),
                new Color32(9, 15, 29, 255), Color.white); shows.onClick.AddListener(ShowTvShows);
            var specials = CreateRuntimeButton("SpecialsMenuItem", menu.transform, "SPECIALS", new Vector2(0, .27f), new Vector2(1, .48f),
                new Color32(9, 15, 29, 255), Color.white); specials.onClick.AddListener(ShowSpecials);
            var locations = CreateRuntimeButton("LocationsMenuItem", menu.transform, "LOCATIONS", new Vector2(0, .02f), new Vector2(1, .23f),
                new Color32(9, 15, 29, 255), Color.white); locations.onClick.AddListener(ShowLocations);

            var hover = universeButton.gameObject.AddComponent<NavigationHoverDropdown>();
            hover.Configure(menu);
            universeButton.gameObject.AddComponent<NavigationHoverRelay>().Configure(hover);
            menu.AddComponent<NavigationHoverRelay>().Configure(hover);
        }

        private void EnsureRosterDropdown(Transform navigation)
        {
            navigation.SetAsLastSibling();
            var rosterButton = navigation.Find("RosterButton");
            if (rosterButton == null || rosterButton.Find("RosterDropdown") != null) return;

            var menu = CreateRuntimePanel("RosterDropdown", rosterButton, new Color32(5, 9, 20, 255),
                new Vector2(0, -2.25f), new Vector2(1, 0));
            menu.transform.SetAsLastSibling();
            var rosterItem = CreateRuntimeButton("RosterMenuItem", menu.transform, "ROSTER",
                new Vector2(0, .69f), new Vector2(1, .97f), new Color32(9, 15, 29, 255), Color.white);
            rosterItem.onClick.AddListener(ShowRoster);
            var teamsItem = CreateRuntimeButton("TeamsMenuItem", menu.transform, "TEAMS",
                new Vector2(0, .36f), new Vector2(1, .64f), new Color32(9, 15, 29, 255), Color.white);
            teamsItem.onClick.AddListener(ShowTeams);
            var titlesItem = CreateRuntimeButton("TitlesMenuItem", menu.transform, "TITLES",
                new Vector2(0, .03f), new Vector2(1, .31f), new Color32(9, 15, 29, 255), Color.white);
            titlesItem.onClick.AddListener(ShowTitles);

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
