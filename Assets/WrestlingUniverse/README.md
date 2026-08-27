# Wrestling Universe Tracker

## Landing page

The landing page uses Unity uGUI (`Canvas`, `Image`, `Text`, `InputField`, and `Button`) so every generated object remains visually editable in the Hierarchy and Inspector.

To rebuild it, allow scripts to compile and choose **Wrestling Universe > Build Landing Page**. The tool creates `Assets/WrestlingUniverse/Scenes/LandingPage.unity`, makes it the sole enabled build scene, and creates the shared `UniverseTheme` ScriptableObject.

Universe saves are stored in a SQLite database named `wrestling-universe.db` under `Application.persistentDataPath`. The database schema is created automatically, and the landing page reloads every universe at startup. Image selection remains a placeholder until the image-import feature is implemented.

SQLite runtime dependencies live in `Assets/Plugins`: `Mono.Data.Sqlite.dll` is Unity's Windows Mono provider, and `x86_64/sqlite3.dll` is the official SQLite 3.53.4 Windows x64 runtime. The native plugin is enabled only for the Windows Editor and Windows 64-bit player.

Owner portraits and promotion logos can be selected from PNG, JPG/JPEG, or BMP files up to 20 MB. Selected files are copied into `Application.persistentDataPath/UniverseImages/<universe-id>` so saves do not break if the original file is moved or deleted.

Each universe card includes a **Manage / Edit** button. It opens the same form with saved values and images preloaded; saving updates the existing SQLite row and card rather than creating another universe.

The adjacent **Load Universe** button selects that universe by its stable ID and opens `UniverseWorkspace.unity`. The workspace reloads its header directly from SQLite and is the shell for future universe-exclusive roster, show, title, team, stable, and history features.

The workspace navigation bar contains **My Universe**, **Roster**, **Booking**, **Results**, and **Analytics**. Buttons currently switch the active styling and placeholder content; each section is ready to receive its feature panel.

The **Roster** section contains a toolbar, signed-talent count, and a five-column portrait-card grid. **Sign Wrestler** captures name, future Brand assignment, disposition, gender, tier, OVR, and an optional photo. Wrestlers are stored in SQLite against the active universe ID and reload as portrait cards.

Each wrestler portrait card includes **Edit**. It reopens the wrestler form with all saved fields and the managed photo preloaded, then updates the same SQLite record.

Hovering **Roster** in the workspace navigation opens a submenu containing **Roster** and **Teams**. The main button and Roster submenu item open signed talent; Teams opens its own blank feature workspace.

Teams are persisted per universe with a name, future Brand assignment, disposition, and one to five roster-member references. The custom member dropdown supports multiple selections with a five-member cap, and each team card can reopen the form for editing.

Teams also support an optional managed photo. It is copied into the universe image directory, persisted in SQLite through schema version 4, shown on team cards, and retained or replaced during editing.
