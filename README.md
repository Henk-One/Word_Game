================================================================================
LEARNINGPATH
Enterprise Cloud, Web Development, Testing, DevOps & AI Learning Project
================================================================================

PURPOSE
-------

Build one complete application while learning modern enterprise software
development, cloud architecture, testing, DevOps, Azure and AI-assisted
development.

Prerequisites
-------------

Install these tools before building or testing the repository:

  - .NET 10 SDK (not only the .NET runtime)
  - Node.js LTS, including npm
  - Git
  - Visual Studio Code (recommended)
  - A modern browser

Docker Desktop is also needed for the container and infrastructure exercises,
but it is not required for the basic frontend build or the in-memory contract
tests.

Verify the required tool versions from a VS Code terminal:

  dotnet --version
  node --version
  npm --version
  git --version

The projects target .NET 10, so the `dotnet --version` command must report a
10.x SDK. If multiple SDK versions are installed, use the repository's
`global.json` when one is present, or install the .NET 10 SDK explicitly.

First-clone setup
-----------------

After cloning the repository, restore the frontend and .NET dependencies before
building:

  cd frontend/game
  npm install

  cd ../../
  dotnet restore services/Game.Api/Game.Api.csproj
  dotnet restore services/Leaderboard.Api/Leaderboard.Api.csproj

The test projects are restored automatically by `dotnet test`, but they can be
restored explicitly when preparing a new machine:

  dotnet restore services/Game.Api/tests/Unit/Game.Api.UnitTests.csproj
  dotnet restore services/Game.Api/tests/Contract/Game.Api.ContractTests.csproj
  dotnet restore services/Game.Api/tests/E2E/Api/Game.Api.E2EApiTests.csproj
  dotnet restore services/Leaderboard.Api/tests/Unit/Leaderboard.Api.UnitTests.csproj
  dotnet restore services/Leaderboard.Api/tests/Integration/Leaderboard.Api.IntegrationTests.csproj

The APIs use separate SQLite databases:

  - `Game.Api` uses `game.db`, applies its EF Core migrations, and seeds the
    word data automatically when it starts.
  - `Leaderboard.Api` uses `leaderboard.db` and applies its EF Core migrations
    automatically when it starts.

The SQLite database files are local runtime data and are not required in the
repository. The first startup creates the local database files and applies the
pending migrations.

The application will be a browser-based language-learning Worde Guess game.

Frontend game quick start
-------------------------

The Html-based browser game lives in `frontend/game` and is validated with the
TypeScript compiler before bundling.

Run locally:

  cd frontend/game
  npm install
  npm run build
  npm run serve

The build checks the scene code and creates a complete static game in `dist/`,
including `index.html`, the bundled game, and word data. The serve command
serves that folder; open `http://127.0.0.1:8080` to play. After building, this
also works directly:

  npx http-server dist -p 8080

`npm run build` runs the frontend unit tests before compiling and bundling.
To run backend unit and integration tests before building both APIs and the
frontend, run the default VS Code build task with **Ctrl+Shift+B**. The same
workflow is available from the repository root with:

  powershell -NoProfile -ExecutionPolicy Bypass -File scripts\build.ps1

The combined build stops before compilation if any test suite fails.
It prints live code coverage metrics to the console after parsing the test
runs' Cobertura XML reports.

The Game API contract test can also be run by itself with:

  dotnet test services\Game.Api\tests\Contract\Game.Api.ContractTests.csproj

The game itself is primarily the vehicle for learning:

  - Modern web development
  - JavaScript
  - TypeScript
  - Node.js
  - npm
  - C#
  - .NET
  - ASP.NET Core
  - REST APIs
  - Microservices
  - Authentication
  - Authorization
  - Microsoft Entra ID
  - Application IDs / Client IDs
  - OAuth 2.0
  - OpenID Connect
  - Docker
  - Kubernetes
  - Azure
  - Azure Container Apps
  - Application Insights
  - Azure Monitor
  - Log Analytics
  - Logging
  - Distributed tracing
  - Git
  - GitHub
  - GitHub Actions
  - CI/CD
  - YAML
  - Unit testing
  - Integration testing
  - End-to-end testing
  - Playwright
  - xUnit
  - Infrastructure/configuration as code
  - Enterprise architecture
  - Microsoft Learn
  - AI-assisted software development
  - AI-assisted testing
  - AI-assisted documentation
  - AI-assisted debugging
  - AI skills and responsible AI usage

IMPORTANT CONSTRAINT
--------------------

The goal is NOT to build the most complicated game possible.

The goal is to build one evolving application that forces the learner to
understand modern enterprise software development.

The game is the motivation.

The repository is the learning record.

The technologies are the learning material.

AI is an assistant and learning accelerator, not a replacement for
understanding the code.

Use the Azure free plan / free services where possible.

Do NOT deliberately create infrastructure that requires ongoing paid
consumption.

Before creating an Azure resource:

  1. Check whether it is permanently free or only temporarily free.
  2. Check the current pricing/free allowance.
  3. Check whether the resource requires paid compute.
  4. Avoid leaving paid resources running.
  5. Prefer local development when Azure is unnecessary for learning.

In particular:

  Kubernetes should initially be learned locally.

Do not create an AKS cluster with paid worker nodes simply for learning
Kubernetes.

================================================================================
1. PROJECT NAME
================================================================================

Repository:

    learningpath

Application:

    LearningPath

Game:

    Word Guess Game

Repository URL:

    The GitHub repository created by the learner.

The repository name is intentionally different from the game name.

The repository represents the entire learning journey.

The game is the main application being developed inside the repository.

================================================================================
2. PROJECT VISION
================================================================================

Build a browser-based language-learning Worde Guess game backed by a modern
enterprise-style cloud architecture.

A user should eventually be able to:

  - Sign in
  - Configure language, difficulty, and topic on the Settings page
  - Start a game
  - Receive a word from the selected language/topic/difficulty
  - Guess letters
  - See the word progressively revealed
  - Receive points for correctly guessing letters while the game is active
  - Lose the current score when Worde Guess is completed
  - Finish the word successfully
  - Submit the score
  - View the top 10 scores for the selected language, difficulty, and topic
  - Keep personal high scores separately for each language, difficulty, and topic
  - View game history
  - Play again

Initial languages:

  - Dutch
  - Indonesian
  - English

Initial difficulty levels:

  - Beginner
  - Intermediate

The word list should initially be stored as application data.

Later, the architecture can evolve to store words in a database.

================================================================================
3. GAME RULES
================================================================================

LANGUAGE SELECTION
------------------

The player chooses a language:

  - Dutch
  - Indonesian
  - English

The main menu displays the current language, difficulty, and topic. The player
changes these defaults from the Settings page. Available topics are Car Parts
(the default) and Beverages. Settings are edited as temporary choices:
Cancel returns to the main menu without changing the saved defaults, while Save
stores all selections in the browser and returns to the main menu.

The game scene displays the active language, difficulty, and topic. Highscores
are stored separately for each combination of language, difficulty, and topic.

Words are currently selected by `Game.Api` through its `IWordProvider`
implementation. The frontend starts a game through the API and receives the
word length; it does not load static word-list JSON files.
Expanding the backend word provider to use curated multilingual lists is future
work.

DIFFICULTY
----------

The player chooses:

  - Beginner
  - Intermediate

High scores are saved separately for each language, difficulty, and topic.
The game scene displays the saved high score matching its current selection.
Changing settings does not overwrite scores for other combinations.

WORD SELECTION
--------------

A word is selected from the word collection matching both the chosen language
and difficulty. Intermediate collections contain more challenging vocabulary
than their beginner counterparts.

For example:

  Dutch / Beginner

  huis
  water
  boek
  kat
  hond

or:

  Indonesian / Beginner

  rumah
  air
  buku
  kucing
  anjing

The initial word lists can be manually maintained.

Do not initially use AI to dynamically generate words during gameplay.

The word list should be deterministic and controlled so that the application
can be tested reliably.

GAMEPLAY
--------

The player guesses letters.

For example:

  Word:

      _ _ _ _

  Guess:

      A

  Result:

      _ A _ _

Each successful letter guess while the Worde Guess game is still active awards:

      +1 point

The exact scoring rules can evolve later.

Worde Guess FAILURE
---------------

If the player completes the Worde Guess failure state:

  - The game ends.
  - No more guesses are allowed.
  - The score for that game is reset to 0.
  - The failed game may still be recorded in game history.

Successful completion:

  - The player correctly guesses the complete word.
  - The game ends successfully.
  - The accumulated score can be submitted.
  - The score may appear on the leaderboard if it qualifies.

IMPORTANT:

  The scoring rule must be implemented on the server side.

Do not trust the browser to calculate or submit an arbitrary score.

The frontend may display the score, but the backend must validate the game
state and determine whether the score is valid.

================================================================================
4. LEADERBOARD
================================================================================

Scores are grouped into top-10 leaderboards by language, difficulty, and topic.
The examples below show one selected difficulty/topic category per language.

Examples:

  Dutch
  -----

  1. Player A
  2. Player B
  3. Player C
  ...
  10. Player J

  Indonesian
  ----------

  1. Player A
  ...
  10. Player J

  English
  -------

  1. Player A
  ...
  10. Player J

The leaderboard exposes:

  - Player name/display name
  - Score
  - Language
  - Difficulty
  - Topic
  - Date/time

The backend `Leaderboard.Api` stores submitted scores in its own database.
The current browser high-score screen still reads its leaderboard from local
storage; loading the server-backed leaderboard in the UI remains future work.

Potential later additions:

  - Average score
  - Games played
  - Words successfully completed
  - Win percentage
  - Personal best
  - Weekly leaderboard
  - Monthly leaderboard

Do not implement all of these initially.

================================================================================
5. CURRENT AND TARGET ARCHITECTURE
================================================================================

Current service interaction:

                           +----------------+

                           |    Browser     |
                           |                |
                           | TypeScript     |
                           | Html           |
                           +-------+--------+
                                   |
                                   |
                              Game API
                                   |
                                   v
                         +-------+---------+

                         | Game.Api        |
                         | game sessions   |
                         +---+---------+---+

                             |         |
               Game SQLite  |         | HTTP score submission
                             v         v
                      +----------+  +-------------------+

                      | Game DB  |  | Leaderboard.Api   |
                      +----------+  +---------+---------+
|
| Leaderboard SQLite
v
+---------------+
| Leaderboard DB|
+---------------+
The browser's high-score screen currently uses browser local storage; it is
not yet connected to the Leaderboard API's GET endpoint. Player.Api is
reserved for future work. The services own separate databases; they do not
share a data store.
Future platform goals include an API gateway, Microsoft Entra ID, GitHub
Actions deployment, Azure Container Apps, centralized telemetry, and
Kubernetes/AKS where appropriate. These are not part of the current runtime.

================================================================================
6. TECHNOLOGY STACK


FRONTEND / GAME

TypeScript
HTML5
Canvas
Node.js
npm
Why:
The learner already has JavaScript experience.
TypeScript is therefore a natural progression.
Potential later addition:
React
React should not be introduced merely because it is popular.
Introduce it when there is a genuine learning objective for component-based
web UI development.

BACKEND

C#
.NET
ASP.NET Core
Initial services:
Game.Api
Player.Api
Leaderboard.Api
Potential later services:
Word.Api
Statistics.Api
Notification.Api
Do not create every possible microservice at the beginning.
Microservices should be introduced gradually so their trade-offs can
actually be understood.

TESTING

Unit testing:
xUnit
Integration testing:
ASP.NET Core integration testing
WebApplicationFactory
Keep tests beside the application they verify:
services//tests/
Unit/          Isolated backend business logic
Integration/   Backend components working across real boundaries
Contract/      Compatibility with published or consumed API contracts
E2E/Api/       Complete scenarios through a running backend API
frontend//tests/
Unit/          Isolated frontend logic and components
Integration/   Frontend components working together
Accessibility/ Accessibility checks for frontend pages and components
E2E/UI/        Complete browser-based user journeys
This keeps backend and frontend tests with their target code while test
categories remain clear in the path. Add infrastructure-specific validation
beside the infrastructure code it exercises.
End-to-end testing:
Playwright
TypeScript

CONTAINERS

Docker
Docker Compose

CI/CD

GitHub Actions
YAML

CLOUD

Microsoft Azure
Important areas:
Microsoft Entra ID
Azure Container Apps
Application Insights
Azure Monitor
Log Analytics
Managed Identity
Key Vault concepts
Container Registry concepts
Kubernetes
AKS architecture

SOURCE CONTROL

Git
GitHub

LEARNING

Microsoft Learn
Azure Architecture Center
Official Microsoft documentation
Official GitHub documentation

AI

ChatGPT
GitHub Copilot where available
AI-assisted development
AI-assisted testing
AI-assisted documentation
AI-assisted debugging
AI-assisted learning

================================================================================
7. REPOSITORY STRUCTURE

Everything should eventually live in ONE Git repository.
Suggested structure:
learningpath/
|
+-- README.md
|
+-- docs/
|   |
|   +-- architecture/
|   |   +-- system-overview.md
|   |   +-- frontend-architecture.md
|   |   +-- backend-architecture.md
|   |   +-- api-design.md
|   |   +-- data-model.md
|   |
|   +-- learning/
|   |   +-- daily-journal.md
|   |   +-- roadmap.md
|   |
|   +-- decisions/
|   +-- runbooks/
|   +-- troubleshooting/
|   +-- microsoft-learn/
|       +-- modules.md
|
+-- frontend/
|   |
|   +-- game/
|       |
|       +-- src/
|       +-- public/
|       +-- tests/
|           +-- Unit/
|           +-- Integration/
|           +-- Accessibility/
|           +-- E2E/
|               +-- UI/
|       +-- package.json
|       +-- tsconfig.json
|
+-- services/
|   |
|   +-- Game.Api/
|       +-- tests/
|           +-- Unit/
|           +-- Integration/
|           +-- Contract/
|           +-- E2E/
|               +-- Api/
|   |
|   +-- Player.Api/
|   |
|   +-- Leaderboard.Api/
|       +-- tests/
|           +-- Leaderboard.Api.UnitTests.csproj
|           +-- LeaderboardSubmissionTests.cs
|
+-- data/
|   |
|   +-- words/
|       |
|       +-- english/
|       +-- dutch/
|       +-- indonesian/
|
+-- infrastructure/
|   |
|   +-- docker/
|   |
|   +-- kubernetes/
|   |
|   +-- helm/
|   |
|   +-- azure/
|
+-- .github/
|   |
|   +-- workflows/
|       |
|       +-- ci.yml
|       +-- e2e.yml
|       +-- deploy.yml
|
+-- docker-compose.yml
|
+-- LearningPath.sln
The repository should become part of the learning material.

================================================================================
8. AI INSIDE THE REPOSITORY

AI is a first-class part of the learning project.
The objective is NOT:
"Let AI build the entire application."
The objective is:
"Learn how to use AI effectively as a software-development assistant
while still understanding and validating the resulting software."
AI should be used for:
• Learning concepts
• Explaining unfamiliar code
• Generating small examples
• Creating exercises
• Suggesting implementations
• Reviewing code
• Finding possible bugs
• Suggesting tests
• Explaining compiler errors
• Explaining runtime errors
• Creating documentation drafts
• Creating architecture diagrams
• Reviewing architecture
• Generating test cases
• Explaining Azure configuration
• Explaining GitHub Actions YAML
• Reviewing Dockerfiles
• Reviewing Kubernetes manifests
• Helping troubleshoot CI failures
• Helping interpret logs
• Generating SQL examples
• Explaining security concepts
• Creating Microsoft Learn study questions
AI should NOT automatically be trusted.
For generated code:
1. Understand it.
2. Review it.
3. Test it.
4. Run security checks where appropriate.
5. Modify it if necessary.
6. Commit it only when understood.

================================================================================
9. AI LEARNING RECORD

Create:
docs/ai/
Suggested files:
docs/ai/prompts.md
docs/ai/lessons-learned.md
docs/ai/reviews.md
Record useful AI interactions and lessons.
For example:
Problem:
Playwright test fails after authentication.
AI suggestion:
Explanation of possible causes.
What I learned:
The browser authentication state was not being persisted.
Actual solution:
...
This turns AI usage into part of the learning record.

================================================================================
10. AI DEVELOPMENT RULES

Rule 1:
Do not paste code into the project simply because AI generated it.
Rule 2:
Ask AI to explain unfamiliar code.
Rule 3:
Prefer small AI-assisted changes over huge generated implementations.
Rule 4:
Ask AI to generate tests for code you have written.
Rule 5:
Ask AI to find edge cases.
Rule 6:
Ask AI to review your implementation rather than always asking it
to write the implementation.
Rule 7:
Verify security-sensitive code independently.
Rule 8:
Never expose secrets, passwords, access tokens, API keys or other
sensitive credentials to an AI system unnecessarily.
Rule 9:
Treat AI-generated Azure configuration as unverified until checked
against current Microsoft documentation.
Rule 10:
The learner remains responsible for understanding and validating
the final code.

================================================================================
11. AI SKILLS TO LEARN

The project should deliberately teach AI-assisted engineering skills.
Learn:
[ ] Writing precise prompts
[ ] Providing useful context
[ ] Asking AI for explanations
[ ] Asking AI for alternatives
[ ] Asking AI to review code
[ ] Asking AI to identify edge cases
[ ] Asking AI to create tests
[ ] Asking AI to debug errors
[ ] Asking AI to explain logs
[ ] Asking AI to review architecture
[ ] Asking AI to review security
[ ] Asking AI to improve documentation
[ ] Asking AI to summarize documentation
[ ] Asking AI to create learning exercises
[ ] Verifying AI-generated information
[ ] Detecting hallucinations
[ ] Protecting secrets and private information
[ ] Understanding AI limitations
[ ] Using AI without becoming dependent on it
Advanced AI skills:
[ ] AI-assisted refactoring
[ ] AI-assisted test generation
[ ] AI-assisted API design
[ ] AI-assisted architecture review
[ ] AI-assisted incident investigation
[ ] AI-assisted documentation maintenance
[ ] AI-assisted code review
[ ] AI-assisted CI/CD troubleshooting

================================================================================
12. CHATGPT / COPILOT STRATEGY

Use AI tools differently depending on the task.
ChatGPT:
• Learning
• Architecture discussions
• Explanations
• Debugging
• Planning
• Reviewing code
• Designing exercises
• Creating documentation
• Explaining Azure concepts
• Explaining errors
• Learning new technologies
GitHub Copilot:
• Code completion
• Small coding tasks
• Boilerplate
• Refactoring assistance
• Local development assistance
• Generating small test cases
Something went wrong and an AI response wasn't generated.

create it again, something went wrong

Hier is het volledige, gecorrigeerde document in één enkel platte-tekstblok, klaar om direct gekopieerd te worden naar learningpath.txt. De naam van het spel is overal consequent bijgewerkt naar Word Guess Game en de afgebroken tekst aan het einde is volledig hersteld.
text
================================================================================
LEARNINGPATH
Enterprise Cloud, Web Development, Testing, DevOps & AI Learning Project
================================================================================

PURPOSE
-------

Build one complete application while learning modern enterprise software
development, cloud architecture, testing, DevOps, Azure and AI-assisted
development.

The application will be a browser-based language-learning Worde Guess game.

Frontend game quick start
-------------------------

The Html-based browser game lives in `frontend/game` and is validated with the
TypeScript compiler before bundling.

Run locally:

  cd frontend/game
  npm install
  npm run build
  npm run serve

The build checks the scene code and creates a complete static game in `dist/`,
including `index.html`, the bundled game, and word data. The serve command
serves that folder; open `http://127.0.0.1:8080` to play. After building, this
also works directly:

  npx http-server dist -p 8080

The game itself is primarily the vehicle for learning:

  - Modern web development
  - JavaScript
  - TypeScript
  - Node.js
  - npm
  - C#
  - .NET
  - ASP.NET Core
  - REST APIs
  - Microservices
  - Authentication
  - Authorization
  - Microsoft Entra ID
  - Application IDs / Client IDs
  - OAuth 2.0
  - OpenID Connect
  - Docker
  - Kubernetes
  - Azure
  - Azure Container Apps
  - Application Insights
  - Azure Monitor
  - Log Analytics
  - Logging
  - Distributed tracing
  - Git
  - GitHub
  - GitHub Actions
  - CI/CD
  - YAML
  - Unit testing
  - Integration testing
  - End-to-end testing
  - Playwright
  - xUnit
  - Infrastructure/configuration as code
  - Enterprise architecture
  - Microsoft Learn
  - AI-assisted software development
  - AI-assisted testing
  - AI-assisted documentation
  - AI-assisted debugging
  - AI skills and responsible AI usage

IMPORTANT CONSTRAINT
--------------------

The goal is NOT to build the most complicated game possible.

The goal is to build one evolving application that forces the learner to
understand modern enterprise software development.

The game is the motivation.

The repository is the learning record.

The technologies are the learning material.

AI is an assistant and learning accelerator, not a replacement for
understanding the code.

Use the Azure free plan / free services where possible.

Do NOT deliberately create infrastructure that requires ongoing paid
consumption.

Before creating an Azure resource:

  1. Check whether it is permanently free or only temporarily free.
  2. Check the current pricing/free allowance.
  3. Check whether the resource requires paid compute.
  4. Avoid leaving paid resources running.
  5. Prefer local development when Azure is unnecessary for learning.

In particular:

  Kubernetes should initially be learned locally.

Do not create an AKS cluster with paid worker nodes simply for learning
Kubernetes.

================================================================================
1. PROJECT NAME
================================================================================

Repository:

    learningpath

Application:

    LearningPath

Game:

    Word Guess Game

Repository URL:

    The GitHub repository created by the learner.

The repository name is intentionally different from the game name.

The repository represents the entire learning journey.

The game is the main application being developed inside the repository.

================================================================================
2. PROJECT VISION
================================================================================

Build a browser-based language-learning Worde Guess game backed by a modern
enterprise-style cloud architecture.

A user should eventually be able to:

  - Sign in
  - Configure language, difficulty, and topic on the Settings page
  - Start a game
  - Receive a word from the selected language/topic/difficulty
  - Guess letters
  - See the word progressively revealed
  - Receive points for correctly guessing letters while the game is active
  - Lose the current score when Worde Guess is completed
  - Finish the word successfully
  - Submit the score
  - View the top 10 scores for the selected language, difficulty, and topic
  - Keep personal high scores separately for each language, difficulty, and topic
  - View game history
  - Play again

Initial languages:

  - Dutch
  - Indonesian
  - English

Initial difficulty levels:

  - Beginner
  - Intermediate

The word list should initially be stored as application data.

Later, the architecture can evolve to store words in a database.

================================================================================
3. GAME RULES
================================================================================

LANGUAGE SELECTION
------------------

The player chooses a language:

  - Dutch
  - Indonesian
  - English

The main menu displays the current language, difficulty, and topic. The player
changes these defaults from the Settings page. Available topics are Car Parts
(the default) and Beverages. Settings are edited as temporary choices:
Cancel returns to the main menu without changing the saved defaults, while Save
stores all selections in the browser and returns to the main menu.

The game scene displays the active language, difficulty, and topic. Highscores
are stored separately for each combination of language, difficulty, and topic.

Words are currently selected by `Game.Api` through its `IWordProvider`
implementation. The frontend starts a game through the API and receives the
word length; it does not load static word-list JSON files.
Expanding the backend word provider to use curated multilingual lists is future
work.

DIFFICULTY
----------

The player chooses:

  - Beginner
  - Intermediate

High scores are saved separately for each language, difficulty, and topic.
The game scene displays the saved high score matching its current selection.
Changing settings does not overwrite scores for other combinations.

WORD SELECTION
--------------

A word is selected from the word collection matching both the chosen language
and difficulty. Intermediate collections contain more challenging vocabulary
than their beginner counterparts.

For example:

  Dutch / Beginner

  huis
  water
  boek
  kat
  hond

or:

  Indonesian / Beginner

  rumah
  air
  buku
  kucing
  anjing

The initial word lists can be manually maintained.

Do not initially use AI to dynamically generate words during gameplay.

The word list should be deterministic and controlled so that the application
can be tested reliably.

GAMEPLAY
--------

The player guesses letters.

For example:

  Word:

      _ _ _ _

  Guess:

      A

  Result:

      _ A _ _

Each successful letter guess while the Word Guess Game is still active awards:

      +1 point

The exact scoring rules can evolve later.

Worde Guess FAILURE
---------------

If the player completes the Worde Guess failure state:

  - The game ends.
  - No more guesses are allowed.
  - The score for that game is reset to 0.
  - The failed game may still be recorded in game history.

Successful completion:

  - The player correctly guesses the complete word.
  - The game ends successfully.
  - The accumulated score can be submitted.
  - The score may appear on the leaderboard if it qualifies.

IMPORTANT:

  The scoring rule must be implemented on the server side.

Do not trust the browser to calculate or submit an arbitrary score.

The frontend may display the score, but the backend must validate the game
state and determine whether the score is valid.

================================================================================
4. LEADERBOARD
================================================================================

Scores are grouped into top-10 leaderboards by language, difficulty, and topic.
The examples below show one selected difficulty/topic category per language.

Examples:

  Dutch
  -----

  1. Player A
  2. Player B
  3. Player C
  ...
  10. Player J

  Indonesian
  ----------

  1. Player A
  ...
  10. Player J

  English
  -------

  1. Player A
  ...
  10. Player J

The leaderboard exposes:

  - Player name/display name
  - Score
  - Language
  - Difficulty
  - Topic
  - Date/time

The backend `Leaderboard.Api` stores submitted scores in its own database.
The current browser high-score screen still reads its leaderboard from local
storage; loading the server-backed leaderboard in the UI remains future work.

Potential later additions:

  - Average score
  - Games played
  - Words successfully completed
  - Win percentage
  - Personal best
  - Weekly leaderboard
  - Monthly leaderboard

Do not implement all of these initially.

================================================================================
5. CURRENT AND TARGET ARCHITECTURE
================================================================================

Current service interaction:

                           +----------------+

                           |    Browser     |
                           |                |
                           | TypeScript     |
                           | Html           |
                           +-------+--------+
                                   |
                                   |
                              Game API
                                   |
                                   v
                         +-------+---------+

                         | Game.Api        |
                         | game sessions   |
                         +---+---------+---+

                             |         |
               Game SQLite  |         | HTTP score submission
                             v         v
                      +----------+  +-------------------+

                      | Game DB  |  | Leaderboard.Api   |
                      +----------+  +---------+---------+
Wees voorzichtig met code.
|
| Leaderboard SQLite
v
+---------------+
| Leaderboard DB|
+---------------+
The browser's high-score screen currently uses browser local storage; it is
not yet connected to the Leaderboard API's GET endpoint. Player.Api is
reserved for future work. The services own separate databases; they do not
share a data store.
Future platform goals include an API gateway, Microsoft Entra ID, GitHub
Actions deployment, Azure Container Apps, centralized telemetry, and
Kubernetes/AKS where appropriate. These are not part of the current runtime.

================================================================================
6. TECHNOLOGY STACK


FRONTEND / GAME

TypeScript
HTML5
Canvas
Node.js
npm
Why:
The learner already has JavaScript experience.
TypeScript is therefore a natural progression.
Potential later addition:
React
React should not be introduced merely because it is popular.
Introduce it when there is a genuine learning objective for component-based
web UI development.

BACKEND

C#
.NET
ASP.NET Core
Initial services:
Game.Api
Player.Api
Leaderboard.Api
Potential later services:
Word.Api
Statistics.Api
Notification.Api
Do not create every possible microservice at the beginning.
Microservices should be introduced gradually so their trade-offs can
actually be understood.

TESTING

Unit testing:
xUnit
Integration testing:
ASP.NET Core integration testing
WebApplicationFactory
Keep tests beside the application they verify:
services//tests/
Unit/          Isolated backend business logic
Integration/   Backend components working across real boundaries
Contract/      Compatibility with published or consumed API contracts
E2E/Api/       Complete scenarios through a running backend API
frontend//tests/
Unit/          Isolated frontend logic and components
Integration/   Frontend components working together
Accessibility/ Accessibility checks for frontend pages and components
E2E/UI/        Complete browser-based user journeys
This keeps backend and frontend tests with their target code while test
categories remain clear in the path. Add infrastructure-specific validation
beside the infrastructure code it exercises.
End-to-end testing:
Playwright
TypeScript

CONTAINERS

Docker
Docker Compose

CI/CD

GitHub Actions
YAML

CLOUD

Microsoft Azure
Important areas:
Microsoft Entra ID
Azure Container Apps
Application Insights
Azure Monitor
Log Analytics
Managed Identity
Key Vault concepts
Container Registry concepts
Kubernetes
AKS architecture

SOURCE CONTROL

Git
GitHub

LEARNING

Microsoft Learn
Azure Architecture Center
Official Microsoft documentation
Official GitHub documentation

AI

ChatGPT
GitHub Copilot where available
AI-assisted development
AI-assisted testing
AI-assisted documentation
AI-assisted debugging
AI-assisted learning

================================================================================
7. REPOSITORY STRUCTURE

Everything should eventually live in ONE Git repository.
Suggested structure:
learningpath/
|
+-- README.md
|
+-- docs/
|   |
|   +-- architecture/
|   |   +-- system-overview.md
|   |   +-- frontend-architecture.md
|   |   +-- backend-architecture.md
|   |   +-- api-design.md
|   |   +-- data-model.md
|   |
|   +-- learning/
|   |   +-- daily-journal.md
|   |   +-- roadmap.md
|   |
|   +-- decisions/
|   +-- runbooks/
|   +-- troubleshooting/
|   +-- microsoft-learn/
|       +-- modules.md
|
+-- frontend/
|   |
|   +-- game/
|       |
|       +-- src/
|       +-- public/
|       +-- tests/
|           +-- Unit/
|           +-- Integration/
|           +-- Accessibility/
|           +-- E2E/
|               +-- UI/
|       +-- package.json
|       +-- tsconfig.json
|
+-- services/
|   |
|   +-- Game.Api/
|       +-- tests/
|           +-- Unit/
|           +-- Integration/
|           +-- Contract/
|           +-- E2E/
|               +-- Api/
|   |
|   +-- Player.Api/
|   |
|   +-- Leaderboard.Api/
|       +-- tests/
|           +-- Leaderboard.Api.UnitTests.csproj
|           +-- LeaderboardSubmissionTests.cs
|
+-- data/
|   |
|   +-- words/
|       |
|       +-- english/
|       +-- dutch/
|       +-- indonesian/
|
+-- infrastructure/
|   |
|   +-- docker/
|   |
|   +-- kubernetes/
|   |
|   +-- helm/
|   |
|   +-- azure/
|
+-- .github/
|   |
|   +-- workflows/
|       |
|       +-- ci.yml
|       +-- e2e.yml
|       +-- deploy.yml
|
+-- docker-compose.yml
|
+-- LearningPath.sln
The repository should become part of the learning material.

================================================================================
8. AI INSIDE THE REPOSITORY

AI is a first-class part of the learning project.
The objective is NOT:
"Let AI build the entire application."
The objective is:
"Learn how to use AI effectively as a software-development assistant
while still understanding and validating the resulting software."
AI should be used for:
• Learning concepts
• Explaining unfamiliar code
• Generating small examples
• Creating exercises
• Suggesting implementations
• Reviewing code
• Finding possible bugs
• Suggesting tests
• Explaining compiler errors
• Explaining runtime errors
• Creating documentation drafts
• Creating architecture diagrams
• Reviewing architecture
• Generating test cases
• Explaining Azure configuration
• Explaining GitHub Actions YAML
• Reviewing Dockerfiles
• Reviewing Kubernetes manifests
• Helping troubleshoot CI failures
• Helping interpret logs
• Generating SQL examples
• Explaining security concepts
• Creating Microsoft Learn study questions
AI should NOT automatically be trusted.
For generated code:
1. Understand it.
2. Review it.
3. Test it.
4. Run security checks where appropriate.
5. Modify it if necessary.
6. Commit it only when understood.

================================================================================
9. AI LEARNING RECORD

Create:
docs/ai/
Suggested files:
docs/ai/prompts.md
docs/ai/lessons-learned.md
docs/ai/reviews.md
Record useful AI interactions and lessons.
For example:
Problem:
Playwright test fails after authentication.
AI suggestion:
Explanation of possible causes.
What I learned:
The browser authentication state was not being persisted.
Actual solution:
...
This turns AI usage into part of the learning record.

================================================================================
10. AI DEVELOPMENT RULES

Rule 1:
Do not paste code into the project simply because AI generated it.
Rule 2:
Ask AI to explain unfamiliar code.
Rule 3:
Prefer small AI-assisted changes over huge generated implementations.
Rule 4:
Ask AI to generate tests for code you have written.
Rule 5:
Ask AI to find edge cases.
Rule 6:
Ask AI to review your implementation rather than always asking it
to write the implementation.
Rule 7:
Verify security-sensitive code independently.
Rule 8:
Never expose secrets, passwords, access tokens, API keys or other
sensitive credentials to an AI system unnecessarily.
Rule 9:
Treat AI-generated Azure configuration as unverified until checked
against current Microsoft documentation.
Rule 10:
The learner remains responsible for understanding and validating
the final code.

================================================================================
11. AI SKILLS TO LEARN
================================================================================

The project should deliberately teach AI-assisted engineering skills.

Learn:

  [ ] Writing precise prompts
  [ ] Providing useful context
  [ ] Asking AI for explanations
  [ ] Asking AI for alternatives
  [ ] Asking AI to review code
  [ ] Asking AI to identify edge cases
  [ ] Asking AI to create tests
  [ ] Asking AI to debug errors
  [ ] Asking AI to explain logs
  [ ] Asking AI to review architecture
  [ ] Asking AI to review security
  [ ] Asking AI to improve documentation
  [ ] Asking AI to summarize documentation
  [ ] Asking AI to create learning exercises
  [ ] Verifying AI-generated information
  [ ] Detecting hallucinations
  [ ] Protecting secrets and private information
  [ ] Understanding AI limitations
  [ ] Using AI without becoming dependent on it

Advanced AI skills:

  [ ] AI-assisted refactoring
  [ ] AI-assisted test generation
  [ ] AI-assisted API design
  [ ] AI-assisted architecture review
  [ ] AI-assisted incident investigation
  [ ] AI-assisted documentation maintenance
  [ ] AI-assisted code review
  [ ] AI-assisted CI/CD troubleshooting

================================================================================
12. CHATGPT / COPILOT STRATEGY
================================================================================

Use AI tools differently depending on the task.

ChatGPT:

  - Learning
  - Architecture discussions
  - Explanations
  - Debugging
  - Planning
  - Reviewing code
  - Designing exercises
  - Creating documentation
  - Explaining Azure concepts
  - Explaining errors
  - Learning new technologies

GitHub Copilot:

  - Code completion
  - Small coding tasks
  - Boilerplate
  - Refactoring assistance
  - Local development assistance
  - Generating small test cases

GitHub:

  - Source control
  - Issues
  - Pull requests
  - Repository
  - Git history
  - GitHub Actions
  - CI/CD
  - Documentation

Do not rely on Copilot as the primary teacher.

Be conscious that GitHub/Copilot plans have usage limits.

The learning project should continue to work even if AI assistance is
temporarily unavailable.

================================================================================
13. LEARNING PHILOSOPHY
================================================================================

Do NOT learn everything first and then build the project.

Instead:

    Learn a concept
         |
         v
    Build a small example
         |
         v
    Add it to LearningPath
         |
         v
    Test it
         |
         v
    Ask AI to review it
         |
         v
    Document what was learned
         |
         v
    Commit to Git
         |
         v
    Continue

For every significant technology, document:

  - What is it?
  - Why are we using it?
  - What problem does it solve?
  - How does it work?
  - How did we configure it?
  - What went wrong?
  - How did we fix it?
  - What did AI suggest?
  - Was AI suggestion correct?
  - What did I learn?

================================================================================
PHASE 0 - DEVELOPMENT ENVIRONMENT
================================================================================

Goal:

  Prepare the development machine.

Learn:

  [ ] VS Code
  [ ] Git
  [ ] GitHub
  [ ] Node.js
  [ ] npm
  [ ] TypeScript
  [ ] C#
  [ ] .NET SDK
  [ ] Docker
  [ ] PowerShell
  [ ] Basic terminal usage

Current status:

  [x] VS Code installed
  [x] Node.js LTS installed
  [x] Node.js installed through Chocolatey/UniGetUI
  [ ] Verify node --version
  [ ] Verify npm --version
  [ ] Verify Node works in VS Code terminal
  [ ] Verify Git installation
  [ ] Configure Git user name/email
  [ ] Verify GitHub access
  [ ] Install/configure .NET SDK
  [ ] Install Docker Desktop
  [ ] Verify Docker works

Exercises:

  [ ] Create a small Node.js program
  [ ] Create a small TypeScript program
  [ ] Create a small C# console application
  [ ] Put them in Git
  [ ] Commit them
  [ ] Push them to GitHub

Milestone:

  A working development environment and GitHub repository.

================================================================================
PHASE 1 - JAVASCRIPT TO TYPESCRIPT
================================================================================

Goal:

  Move from existing JavaScript knowledge to TypeScript.

Learn:

  [ ] Type annotations
  [ ] Interfaces
  [ ] Types
  [ ] Union types
  [ ] Generics
  [ ] Classes
  [ ] Modules
  [ ] async / await
  [ ] npm
  [ ] package.json
  [ ] TypeScript compiler
  [ ] tsconfig.json

Exercises:

  [ ] Convert a small JavaScript program to TypeScript
  [ ] Create interfaces for game objects
  [ ] Create interfaces for language/word data
  [ ] Create a small TypeScript application
  [ ] Compile it
  [ ] Run it with Node.js

Milestone:

  Comfortable reading and writing basic TypeScript.

================================================================================
PHASE 2 - BUILD THE FIRST VERSION OF THE GAME
================================================================================

Goal:

  Create a small playable browser game.

Technology:

  TypeScript
  HTML5
  Canvas

Do NOT introduce:

  - Azure
  - Microservices
  - Authentication
  - Kubernetes

Keep this version simple.

Game:

  Word Guess Game

Implement:

  [ ] Main menu
  [ ] Language selection
  [ ] Dutch
  [ ] Indonesian
  [ ] English
  [ ] Beginner difficulty
  [ ] Intermediate difficulty
  [ ] Word selection
  [ ] Letter input
  [ ] Display guessed letters
  [ ] Worde Guess state
  [ ] Score
  [ ] Game over
  [ ] Keep the completed game and revealed word visible until the player chooses
      what to do next
  [ ] Stop returns to the start page
  [ ] Restart starts a fresh game without clearing the high score
  [ ] Successful completion

Scoring:

  Correct letter guessed while game is active:

      +1 point

  Worde Guess completed:

      Game ends
      No more guesses
      Score becomes 0
      Reveal the word and keep the game-over screen visible until the player
      chooses Stop or Restart

At this stage all game data can be local.

Milestone:

  A playable language game running locally in a browser.

================================================================================
PHASE 3 - FIRST C# API
================================================================================

Goal:

  Introduce backend development.

Create:

  Game.Api

Technology:

  C#
  .NET
  ASP.NET Core

Learn:

  [ ] HTTP
  [ ] REST
  [ ] GET
  [ ] POST
  [ ] PUT
  [ ] DELETE
  [ ] JSON
  [ ] Controllers or Minimal APIs
  [ ] Dependency Injection
  [ ] Configuration
  [ ] Logging
  [ ] Middleware
  [ ] HTTP status codes

Create endpoints such as:

  GET  /api/game
  POST /api/game/start
  POST /api/game/guess
  POST /api/game/finish

The browser game should communicate with the API.

Important:

  The backend should become authoritative for game state and scoring.

Milestone:

  Browser game -> ASP.NET Core API.

================================================================================
PHASE 4 - DATABASE / DATA STORAGE
================================================================================

Goal:

  Persist game information.

Learn:

  [ ] Relational database concepts
  [ ] Tables
  [ ] Primary keys
  [ ] Foreign keys
  [ ] Relationships
  [ ] SQL
  [ ] Entity Framework Core
  [ ] Database migrations
  [ ] Connection strings
  [ ] Configuration

Possible entities:

  Player
  Game
  Word
  Score
  GameHistory

Start locally.

Do not immediately move the database to Azure.

Milestone:

  The game can save and retrieve data.

================================================================================
PHASE 5 - WORD DATA
================================================================================

Goal:

  Create controlled multilingual game data.

Create:

  English word lists
  Dutch word lists
  Indonesian word lists

For each language:

  Beginner words
  Intermediate words

Example structure:

  services/Game.Api/
    word data or provider-backed storage

Learn:

  [ ] JSON
  [ ] Data validation
  [ ] DTOs
  [ ] Data loading
  [ ] Data ownership
  [ ] Localization concepts

Important:

  The word list is application data.

  AI may assist with creating candidate lists, but all words should be
  reviewed and validated before becoming official game data.

Potential future enhancement:

  Add metadata such as:

    word
    translation
    category
    difficulty
    pronunciation
    hint

Do not implement everything initially.

================================================================================
PHASE 6 - UNIT TESTING
================================================================================

Goal:

  Learn what a real unit test is.

Technology:

  xUnit

Learn:

  [ ] Arrange
  [ ] Act
  [ ] Assert
  [ ] Test naming
  [ ] Test isolation
  [ ] Test doubles
  [ ] Mocking
  [ ] Boundary testing
  [ ] Edge cases
  [ ] Code coverage

Important business rules to test:

  [ ] Correct letter increases score
  [ ] Incorrect letter does not increase score
  [ ] Repeated guesses are handled correctly
  [ ] Completed word ends the game
  [ ] Worde Guess failure ends the game
  [ ] Worde Guess failure resets score
  [ ] Guesses after game completion are rejected
  [ ] Score cannot be supplied directly by the client
  [ ] Language must be valid
  [ ] Difficulty must be valid

Create:

  services/Game.Api/tests/Unit
  services/Player.Api/tests/Unit
  services/Leaderboard.Api/tests/Unit

Use AI to:

  [ ] Suggest edge cases
  [ ] Review tests
  [ ] Identify missing scenarios
  [ ] Explain why a test is useful

Milestone:

  Backend business logic has meaningful automated unit tests.

================================================================================

PHASE 7 - INTEGRATION TESTING

Goal:
Learn the difference between unit and integration tests.
Technology:
ASP.NET Core
WebApplicationFactory
Test:
HTTP request
|
v
ASP.NET Core
|
v
Business logic
|
v
Data access
|
v
Test database
Learn:
[ ] Integration testing
[ ] Test web hosts
[ ] HTTP integration tests
[ ] Test databases
[ ] Test configuration
[ ] Test environments
[ ] Test fixtures
Milestone:
The API can be tested as a running application rather than
only as isolated classes.

================================================================================
PHASE 8 - CONTRACT TESTING

Goal:
Learn how to verify API compatibility while consumers and providers are
developed and released together, and later when they are versioned separately.
Learn:
[ ] Provider and consumer contracts
[ ] Request and response schemas
[ ] Compatibility and breaking changes
[ ] Contract validation in automated tests
[ ] Extend the monorepo OpenAPI contract checks to every frontend-used API operation
[ ] Learn consumer-driven contracts with Pact
[ ] Publish versioned contracts and provider verification results to a Pact Broker
[ ] Check frontend compatibility with supported backend versions before release
Milestone:
API changes can be checked for compatibility in CI before consumers and
providers are integrated or released.

================================================================================
PHASE 9 - END-TO-END API TESTING

Goal:
Test API behavior through real HTTP requests and verify game business rules.
Test:
[ ] Start a game with a valid language and difficulty
[ ] Submit correct and incorrect letter guesses
[ ] Complete a word and verify the result
[ ] Complete Worde Guess and verify the score resets
[ ] Reject invalid input and guesses after game completion
Milestone:
Automated API tests verify complete game scenarios and their business rules.

================================================================================
PHASE 10 - END-TO-END UI ACCESSIBILITY TESTING

Goal:
Check that the game interface can be used by people with different
accessibility needs.
Test:
[ ] Navigate the game using a keyboard
[ ] Verify controls have accessible names and roles
[ ] Check focus visibility and focus order
[ ] Check status and game-result announcements
[ ] Run automated accessibility checks and review their findings
Milestone:
Core game interactions are keyboard-accessible and accessibility issues are
identified and addressed.

================================================================================
PHASE 11 - END-TO-END UI TESTING

Goal:
Test complete game behavior through the browser as a player would use it.
Test:
[ ] Select a language and difficulty
[ ] Start a game and guess letters
[ ] Verify correct and incorrect guesses update the game
[ ] Complete a word and verify the final score
[ ] Lose a game and verify the score reset
[ ] Start another game
Milestone:
Automated browser tests verify complete player journeys and game rules.

================================================================================
PHASE 12 - MICROSERVICES

Goal:
Learn what microservices are and why they introduce complexity.
Split the backend gradually:
Game.Api
Player.Api
Leaderboard.Api
Potential future:
Word.Api
Architecture:
+-------------+
|   Frontend  |
+------+------+
|
+------+------+
|             |
v             v
+---------+   +---------+
| Game API|   |Player API|
+---------+   +---------+
|
v
+-------------+
|Leaderboard  |
|     API     |
+-------------+
Learn:
[ ] Service boundaries
[ ] Independent deployment
[ ] Service communication
[ ] REST between services
[ ] Configuration
[ ] Failure handling
[ ] Timeouts
[ ] Retries
[ ] Health checks
[ ] Service discovery concepts
[ ] Distributed systems
[ ] Data ownership
[ ] Plan separate frontend and backend repositories for independent versions and releases
[ ] Evaluate whether backend services need separate repositories or can share a backend repository
[ ] Keep contract compatibility checks working across repository boundaries
Important lesson:
Microservices are not automatically better.
Part of this phase is learning the costs of microservices.
Milestone:
You can explain why the application is split into services and what
problems that creates. The frontend and backend can be versioned and released
independently, with API compatibility checked automatically.

================================================================================
PHASE 13 - DOCKER

Goal:
Containerize the application.
Create containers for:
[ ] Game API
[ ] Player API
[ ] Leaderboard API
[ ] Frontend if appropriate
Learn:
[ ] Dockerfile
[ ] Image
[ ] Container
[ ] Registry
[ ] Port mapping
[ ] Environment variables
[ ] Volumes
[ ] Networks
[ ] Multi-stage builds
[ ] Docker Compose
Create:
docker-compose.yml
Local architecture:
+-----------+
| Frontend  |
+-----+-----+
|
+--------------+--------------+
|              |              |
v              v              v
Game API      Player API    Leaderboard API
|              |              |
+--------------+--------------+
|
Database
Milestone:
Run the complete application locally using Docker Compose.

================================================================================
PHASE 14 - GITHUB WORKFLOW AND CI

Goal:
Automatically build and test the application.
Use:
GitHub Actions
YAML
Create:
.github/workflows/ci.yml
Pipeline:
Git Push
|
v
Checkout
|
v
Restore dependencies
|
v
Build
|
+----------+
|          |
v          v
Unit tests  Integration tests
|          |
+----------+
|
v
Success
Learn:
[ ] GitHub Actions
[ ] YAML
[ ] Jobs
[ ] Steps
[ ] Runners
[ ] Artifacts
[ ] Environment variables
[ ] Secrets
[ ] Pull requests
[ ] Branches
[ ] Branch protection
[ ] Build status
Milestone:
Every push / pull request automatically builds and tests the project.

================================================================================
PHASE 15 - MICROSOFT ENTRA ID

Goal:
Learn enterprise authentication and authorization.
Learn:
[ ] Microsoft Entra ID
[ ] Tenant
[ ] App registration
[ ] Application / Client ID
[ ] Tenant ID
[ ] Redirect URI
[ ] Client secret
[ ] OAuth 2.0
[ ] OpenID Connect
[ ] Access tokens
[ ] ID tokens
[ ] JWT
[ ] Claims
[ ] Scopes
[ ] Permissions
[ ] Authentication
[ ] Authorization
Architecture:
+----------------+
|    Browser     |
+-------+--------+
|
v
+------------------+
| Microsoft Entra  |
|       ID         |
+--------+---------+
|
Access Token
|
v
+------------------+
|     Game API     |
+------------------+
Create an App Registration.
Learn the difference between:
User
Application
Service Principal
Managed Identity
Client ID
Tenant ID
Milestone:
A user can authenticate and call a protected API.

================================================================================
PHASE 16 - AUTHORIZATION

Goal:
Learn that authentication and authorization are different.
Authentication:
"Who are you?"
Authorization:
"What are you allowed to do?"
Implement concepts such as:
[ ] Authenticated users
[ ] User claims
[ ] Roles
[ ] Scopes
[ ] Protected API endpoints
[ ] Admin functionality
Example:
GET /api/profile
GET /api/leaderboard/{language}/{difficulty}/{topic}
POST /api/game/finish
Administrative endpoint:
POST /api/admin/reset
Milestone:
Different users have different permissions.
================================================================================
PHASE 17 - PLAYWRIGHT E2E TESTING
================================================================================

Goal:

  Test the application as a real user would.

Technology:

  Playwright
  TypeScript

Test:

  Open browser
       |
       v
  Login
       |
       v
  Select language
       |
       v
  Select difficulty
       |
       v
  Select topic
       |
       v
  Start Word Guess Game
       |
       v
  Guess letters
       |
       v
  Finish word
       |
       v
  Score submitted
       |
       v
  Leaderboard updated

Learn:

  [ ] Playwright
  [ ] Browser automation
  [ ] Locators
  [ ] Assertions
  [ ] Test fixtures
  [ ] Authentication state
  [ ] Screenshots
  [ ] Traces
  [ ] Test retries
  [ ] Headless testing

Create:

  services/Game.Api/tests/E2E/Api
  frontend/game/tests/E2E/UI

Milestone:

  A complete user journey can be tested automatically.

================================================================================
PHASE 18 - CI + E2E
================================================================================

Goal:

  Run browser tests automatically.

Create:

  .github/workflows/e2e.yml

Pipeline:

  Git Push
      |
      v
  Build
      |
      v
  Start application
      |
      v
  Run unit tests
      |
      v
  Run integration tests
      |
      v
  Run Playwright
      |
      v
  Store test results
      |
      v
  Success / Failure

Learn:

  [ ] CI environments
  [ ] Test configuration
  [ ] Environment variables
  [ ] Secrets
  [ ] Test artifacts
  [ ] Screenshots
  [ ] Browser traces

================================================================================
PHASE 19 - AZURE CONTAINER APPS
================================================================================

Goal:

  Deploy containerized microservices to Azure.

Use Azure Container Apps where appropriate.

Learn:

  [ ] Containers in Azure
  [ ] Container Apps
  [ ] Revisions
  [ ] Scaling
  [ ] Scale to zero
  [ ] Environment variables
  [ ] Secrets
  [ ] Networking concepts
  [ ] Service-to-service communication
  [ ] Deployment

Architecture:

                       Azure
                         |
                +--------+--------+

                | Container Apps  |
                | Environment     |
                +--------+--------+
                         |
          +--------------+--------------+

          |                             |
          v                             v
       Game API                  Leaderboard API
  (calls Leaderboard)            (stores scores)

Milestone:

  The application runs in Azure.

================================================================================
PHASE 20 - GITHUB ACTIONS -> AZURE
================================================================================

Goal:

  Automate deployment.

Pipeline:

  Developer
      |
      v
  git push
      |
      v
  GitHub
      |
      v
  GitHub Actions
      |
      +--------------------+

      |                    |
      v                    v
   Build/Test          Build Images
                           |
                           v
                       Deploy
                           |
                           v
                 Azure Container Apps

Learn:

  [ ] Deployment workflows
  [ ] GitHub environments
  [ ] Deployment secrets
  [ ] OIDC
  [ ] Workload identity
  [ ] Azure authentication
  [ ] Automated deployment
  [ ] Deployment history
  [ ] Rollback concepts

Important:

  Prefer short-lived identity / OIDC where appropriate rather than
  storing long-lived Azure credentials unnecessarily.

Milestone:

  A successful GitHub push can automatically deploy the application.

================================================================================
PHASE 21 - APPLICATION INSIGHTS AND AZURE MONITOR
================================================================================

Goal:

  Learn enterprise observability.

Learn:

  [ ] Logging
  [ ] Structured logging
  [ ] Metrics
  [ ] Traces
  [ ] Exceptions
  [ ] Dependencies
  [ ] Application Insights
  [ ] Azure Monitor
  [ ] Log Analytics
  [ ] Queries
  [ ] Alerts
  [ ] Dashboards
  [ ] Distributed tracing
  [ ] Correlation IDs
  [ ] OpenTelemetry concepts

Architecture:

     Browser
        |
        v
     Game API  -------->  Leaderboard API

        |                       |
        v                       v
     Game DB             Leaderboard DB
     (SQLite)               (SQLite)

        |                       |
        +-----------+-----------+
                    |
                    v
         +----------------------+

         | Application Insights |
         +----------+-----------+
                    |
                    v
         +----------------------+

         |    Azure Monitor     |
         +----------+-----------+
                    |
                    v
              Log Analytics

Exercises:

  [ ] Find successful requests
  [ ] Find failed requests
  [ ] Find slow requests
  [ ] Find exceptions
  [ ] Find dependencies
  [ ] Trace a request through services
  [ ] Create an alert
  [ ] Create a dashboard
  [ ] Deliberately introduce an error
  [ ] Diagnose the error using telemetry

Milestone:

  You can investigate a production-style problem using telemetry.

================================================================================
PHASE 22 - HISTORY AND AUDITING
================================================================================

Learn the different types of history.

Git history:

  [ ] Commits
  [ ] Branches
  [ ] Pull requests
  [ ] Diffs
  [ ] Reverts

CI/CD history:

  [ ] Builds
  [ ] Test runs
  [ ] Deployments
  [ ] Failed deployments
  [ ] Deployment history

Application history:

  [ ] Player actions
  [ ] Games
  [ ] Scores
  [ ] Login events

Observability history:

  [ ] Requests
  [ ] Traces
  [ ] Exceptions
  [ ] Dependencies
  [ ] Metrics

Azure history/auditing:

  [ ] Resource changes
  [ ] Activity logs
  [ ] Identity-related events
  [ ] Deployment events

Important lesson:

  These are different concepts and should not be treated as the same thing.

================================================================================
PHASE 23 - KUBERNETES LOCALLY
================================================================================

Goal:

  Learn Kubernetes without paying for AKS compute.

Start locally.

Learn:

  [ ] Kubernetes
  [ ] Cluster
  [ ] Node
  [ ] Pod
  [ ] Deployment
  [ ] Service
  [ ] Ingress
  [ ] Namespace
  [ ] ConfigMap
  [ ] Secret
  [ ] Replica
  [ ] Liveness probe
  [ ] Readiness probe
  [ ] Resource limits
  [ ] Volumes
  [ ] Networking

Architecture:

                    Kubernetes Cluster

              +-------------------------+

              |                         |
              |   +------+  +--------+  |
              |   | Game |  | Leader |  |
              |   | Pod  |  |  Pod   |  |
              |   +------+  +--------+  |
              |     |           ^       |
              |     +-----------+       |
              | (Service Communication) |
              +-------------------------+

Deploy:

  [ ] Game API
  [ ] Leaderboard API
  [ ] Frontend if appropriate

Milestone:

  The complete backend runs in a local Kubernetes cluster.             v

================================================================================
PHASE 24 - HELM
================================================================================

Goal:

  Learn how Kubernetes applications are packaged.

Learn:

  [ ] Helm
  [ ] Chart
  [ ] Values
  [ ] Templates
  [ ] Releases
  [ ] Configuration

Create:

  infrastructure/helm/

Milestone:

  Deploy LearningPath to Kubernetes using Helm.

================================================================================
PHASE 25 - AKS ARCHITECTURE
================================================================================

Goal:

  Understand how local Kubernetes concepts map to Azure Kubernetes Service.

Learn:

  [ ] AKS
  [ ] Control plane
  [ ] Worker nodes
  [ ] Networking
  [ ] Ingress
  [ ] Identity
  [ ] Managed identity
  [ ] Workload identity
  [ ] Azure Monitor integration
  [ ] Container Registry
  [ ] Scaling
  [ ] Security
  [ ] Cluster upgrades

Important:

  Learn AKS architecture without necessarily running an AKS cluster.

  AKS worker nodes consume Azure resources and can generate charges.

Milestone:

  Understand how the local Kubernetes deployment would map to AKS.

================================================================================
PHASE 26 - MANAGED IDENTITY AND KEY VAULT
================================================================================

Goal:

  Learn enterprise secrets and identity management.

Learn:

  [ ] Managed Identity
  [ ] System-assigned identity
  [ ] User-assigned identity
  [ ] Azure Key Vault
  [ ] Secret management
  [ ] Certificates
  [ ] Avoiding hard-coded secrets
  [ ] Workload identity

Replace insecure configuration such as:

  PASSWORD=abc123

with appropriate managed identity / secret-management approaches.

================================================================================
PHASE 27 - ENTERPRISE RESILIENCE
================================================================================

Goal:

  Learn what happens when distributed systems fail.

Introduce:

  [ ] Timeouts
  [ ] Retries
  [ ] Circuit breaker concepts
  [ ] Rate limiting
  [ ] Health checks
  [ ] Readiness checks
  [ ] Liveness checks
  [ ] Graceful failure
  [ ] Dependency failures
  [ ] Idempotency
  [ ] Correlation IDs

Deliberately break services.

Example:

  Game API
      |
      v
  Leaderboard API
      |
      X
    FAILURE

Observe:

  [ ] What happens?
  [ ] What does the user see?
  [ ] What does the log show?
  [ ] What does Application Insights show?
  [ ] How does the system recover?

Use AI to help investigate the failure.

Do not accept an AI diagnosis without checking the actual logs and behavior.

================================================================================
PHASE 28 - ASYNCHRONOUS COMMUNICATION
================================================================================

Goal:

  Understand messaging instead of always using synchronous REST calls.

Learn concepts:

  [ ] Message
  [ ] Queue
  [ ] Topic
  [ ] Publisher
  [ ] Subscriber
  [ ] Event
  [ ] Consumer
  [ ] Retry
  [ ] Dead-letter queue
  [ ] Eventual consistency

Potential technology:

  Azure Service Bus

But first learn the concepts locally if possible.

Example:

  Game API
      |
      | "GameFinished"
      v
    Queue
      |
      +------------+

      |            |
      v            v
  Leaderboard   Statistics

Milestone:

  Understand when asynchronous communication is useful.

================================================================================
PHASE 29 - SECURITY
================================================================================

Learn:

  [ ] Authentication
  [ ] Authorization
  [ ] HTTPS
  [ ] Token validation
  [ ] Secrets
  [ ] Dependency vulnerabilities
  [ ] Container scanning
  [ ] Secure CI/CD
  [ ] Least privilege
  [ ] Managed identities
  [ ] OIDC
  [ ] Security headers
  [ ] Input validation
  [ ] Rate limiting

Add security checks to GitHub Actions where practical.

Use AI to:

  [ ] Review code for common security problems
  [ ] Suggest security test cases
  [ ] Explain vulnerabilities
  [ ] Explain why a mitigation works

Always verify security advice against authoritative documentation.

================================================================================
PHASE 30 - DATABASE AND DATA ARCHITECTURE
================================================================================

Learn:

  [ ] Database ownership
  [ ] Database per service
  [ ] Shared database trade-offs
  [ ] Transactions
  [ ] Consistency
  [ ] Eventual consistency
  [ ] Migrations
  [ ] Indexes
  [ ] Query performance
  [ ] Backups
  [ ] Data retention

Important:

  Do not force a separate database for every microservice just because
  microservices are being used.

Learn the architectural trade-offs.

================================================================================
PHASE 31 - API DESIGN
================================================================================

Learn:

  [ ] REST
  [ ] Resource design
  [ ] HTTP methods
  [ ] HTTP status codes
  [ ] API versioning
  [ ] Validation
  [ ] Error responses
  [ ] Pagination
  [ ] Filtering
  [ ] Authentication
  [ ] Authorization
  [x] OpenAPI 3.1 document generation (.NET 10 compatible)
  [x] Swagger UI in Development

Implemented backend APIs:

  POST /api/game/start
  POST /api/game/next-round
  POST /api/game/guess
  POST /api/game/hint
  POST /api/game/finish
  POST /api/game/stop
  GET  /api/leaderboard/{language}/{difficulty}/{topic}
  POST /api/leaderboard/submit

Player APIs, a standalone language/word API, and the older profile/history
examples remain future work.

In Development, each API publishes its OpenAPI 3.1 document at
`/openapi/v1.json` and Swagger UI at `/swagger`.

Milestone:

  APIs are documented and predictable.

================================================================================
PHASE 32 - CI/CD MATURITY
================================================================================

Improve the pipeline.

Eventually:

  Pull Request
       |
       v
  Build
       |
       v
  Unit Tests
       |
       v
  Integration Tests
       |
       v
  Publish Test Results
       |
       v
  Security Checks
       |
       v
  Build Containers
       |
       v
  E2E Tests
       |
       v
  Deploy to Environment
       |
       v
  Smoke Tests
       |
       v
  Monitoring

Learn:

  [ ] Pull request checks
  [ ] Branch policies
  [ ] Environments
  [ ] Approvals
  [ ] Artifacts
  [ ] Deployment strategies
  [ ] Rollbacks
  [ ] Smoke tests
  [ ] Release management
  [ ] Publish .NET TRX and frontend test results from CI
  [ ] Connect automated tests to Azure DevOps Test Plans test cases
  [ ] Update linked test-point execution results with actual pass/fail/skip outcomes
  [ ] Preserve CI failure when reporting a failed test execution
  [ ] Secure Azure DevOps test-management integration with least-privilege identity

Future test-management milestone:

  An automated test run updates the execution state of its linked Azure
  DevOps test case with the actual result, while failures remain visible in CI.

================================================================================
PHASE 33 - AI-ASSISTED CI/CD
================================================================================

Use AI to help understand CI/CD failures.

For example:

  GitHub Actions
       |
       v
  Build fails
       |
       v
  Read logs
       |
       v
  Ask AI to explain possible causes
       |
       v
  Check the actual project
       |
       v
  Fix
       |
       v
  Run pipeline again

Learn:

  [ ] Reading CI logs
  [ ] Understanding exit codes
  [ ] Diagnosing failed builds
  [ ] Diagnosing failed tests
  [ ] Diagnosing Docker failures
  [ ] Diagnosing deployment failures
  [ ] Asking AI useful debugging questions
  [ ] Verifying AI suggestions

Important:

  AI should help interpret the evidence.

  The evidence remains the logs, source code, configuration and
  actual application behavior.

================================================================================
PHASE 34 - AI-ASSISTED TEST ENGINEERING
================================================================================

Use AI deliberately for testing.

For each important feature:

  1. Define expected behavior.
  2. Ask AI for possible test cases.
  3. Review the suggestions.
  4. Implement tests.
  5. Run tests.
  6. Add missing edge cases.

Example:

  Feature:

      Guessing a letter in Word Guess Game.

Possible categories:

  [ ] Correct guess
  [ ] Incorrect guess
  [ ] Duplicate guess
  [ ] Last remaining letter
  [ ] Invalid input
  [ ] Uppercase input
  [ ] Lowercase input
  [ ] Guess after game completion
  [ ] Guess after Word Guess Game failure
  [ ] Unauthorized request
  [ ] Tampered score

Use AI to help find cases that might otherwise be overlooked.

================================================================================
PHASE 35 - AI-ASSISTED DOCUMENTATION
================================================================================

Use AI to help maintain documentation.

Documentation can include:

  [ ] README
  [ ] Architecture
  [ ] API documentation
  [ ] Setup instructions
  [ ] Troubleshooting
  [ ] Learning notes
  [ ] Architecture decisions
  [ ] Deployment instructions
  [ ] Testing instructions

Use AI to help keep files up to date as the application evolves.

Always review AI documentation changes against the actual implementation
and architecture.

================================================================================
PHASE 36 - DOCUMENTATION
================================================================================

The repository should contain documentation for everything important. The
current docs structure is shown in the repository tree above; some directories
are empty and reserved for documentation to be added as the project grows.

README.md

Should explain:

  - What LearningPath is
  - What the game is (Word Guess Game)
  - How to run it
  - Architecture
  - Technologies
  - Testing
  - Deployment
  - AI usage

docs/architecture/

Should contain:

  - docs/architecture/system-overview.md: system context and major components
  - docs/architecture/frontend-architecture.md: frontend structure and design
  - docs/architecture/backend-architecture.md: backend services and design
  - docs/architecture/api-design.md: API conventions and endpoints
  - docs/architecture/testing-architecture.md: test strategy and result reporting
  - docs/architecture/data-model.md: entities, relationships, and data storage

docs/learning/

Should contain:

  - docs/learning/roadmap.md: the staged learning path from development setup
    through end-to-end UI testing (Phases 0-11)
  - docs/learning/daily-journal.md: day-to-day learning reflections

docs/decisions/

Record architectural decisions in numbered ADR files as decisions are made.

Example:

  Why use Container Apps instead of AKS initially?

  Why use Playwright?

  Why use xUnit?

  Why split Game and Leaderboard services?

  Why not split the database immediately?

    Why use TypeScript?

docs/runbooks/

Should contain practical instructions for setting up the development
environment, running the frontend and backend, and troubleshooting common
problems.

docs/troubleshooting/

Record solutions to problems encountered while developing or running the
application.

docs/microsoft-learn/modules.md

Track Microsoft Learn modules and how their concepts are applied to the project.

AI-assisted development is part of the learning process. AI notes can be
documented alongside the relevant learning or project material.

================================================================================
PHASE 37 - MICROSOFT LEARN INTEGRATION
================================================================================

Do not treat Microsoft Learn as a separate course.

Use it alongside the project.

Workflow:

  Microsoft Learn
        |
        v
  Learn concept
        |
        v
  Apply to LearningPath
        |
        v
  Test it
        |
        v
  Document it
        |
        v
  Commit it
        |
        v
  Ask AI to explain/review what was learned

Areas to study:

  [ ] Azure fundamentals
  [ ] Cloud concepts
  [ ] Azure architecture
  [ ] Microsoft Entra ID
  [ ] ASP.NET Core
  [ ] Containers
  [ ] Azure Container Apps
  [ ] Azure Monitor
  [ ] Application Insights
  [ ] GitHub Actions
  [ ] CI/CD
  [ ] Kubernetes
  [ ] AKS
  [ ] Security
  [ ] DevOps
  [ ] Microservices
  [ ] AI-assisted development
  [ ] Responsible AI concepts

================================================================================
PHASE 38 - AZURE COST STRATEGY
================================================================================

Before using an Azure service:

  [ ] Check current pricing
  [ ] Check whether it is always free
  [ ] Check whether the free allowance is temporary
  [ ] Check whether compute is charged
  [ ] Check whether storage is charged
  [ ] Check whether requests are charged
  [ ] Check whether leaving it running creates charges
  [ ] Set appropriate budgets/alerts where available
  [ ] Delete resources that are no longer required

Prefer:

  Local development
       |
       v
  Docker
       |
       v
  Kubernetes locally
       |
       v
  Free Azure services where appropriate
       |
       v
  Paid Azure infrastructure only if deliberately chosen later

Do not assume that because a service appears in an Azure tutorial it is
free to run.

================================================================================
PHASE 39 - FINAL TARGET ARCHITECTURE
================================================================================

At the end of the learning journey, LearningPath should approximately
demonstrate:

                              USERS
                                |
                                v
                         +-------------+

                         |   Browser   |
                         +------+------+
                                |
                    +-----------+-----------+

                    |                       |
                    v                       v
                 Web UI                Html Game
              (Static App)          (Word Guess Game)

                    |                       |
                    +-----------+-----------+
                                |
                                v
                         Authentication
                                |
                                v
                       Microsoft Entra ID
                                |
                                v
                         API entry point
                                |
              +-----------------+-----------------+

              |                                   |
              v                                   v
        +-----------+                       +-----------+

        | Game API  | --------------------> |Leaderboard|
        |           |  (Score Submission)   |    API    |
        +-----+-----+                       +-----+-----+

              |                                   |
              v                                   v
         Game DB (SQLite)                  Leaderboard DB (SQLite)

                         Observability
                                |
              +-----------------+----------------+

              |                                  |
              v                                  v
      Application Insights                 Azure Monitor

              |                                  |
              +------------------+---------------+
                                 |
                                 v
                            Diagnostics

                         Source Control
                                |
                                v
                             GitHub
                                |
                                v
                         GitHub Actions
                                |
              +-----------------+----------------+

              |                 |                |
              v                 v                v
            Build             Test             Deploy

              |                 |                |
              |        +--------+--------+       |
              |        |        |        |       |
              |        v        v        v       |
              |      xUnit   Integration Playwright

              |        |        |        |       |
              +--------+--------+--------+-------+
                                |
                                v
                         Azure deployment

                    Container / Kubernetes
                                |
              +-----------------+----------------+

              |                                  |
              v                                  v
       Docker / Compose                    Kubernetes

              |                                  |
              v                                  v
       Local development                  Local cluster
                                                 |
                                                 v
                                                AKS
                                           (architecture /
                                           optional deployment)

                         AI DEVELOPMENT
                                |
              +-----------------+----------------+

              |                 |                |
              v                 v                v
          Development         Testing       Documentation

              |                 |                |
              +-----------------+----------------+
                                |
                                v
                          AI assistance
                                |
                                v
                         Human validation

================================================================================
PHASE 40 - DEFINITION OF "DONE"
================================================================================

The project is considered a successful learning project when you can:

  [ ] Explain the architecture without reading the documentation
  [ ] Create a TypeScript application
  [ ] Explain Node.js and npm
  [ ] Build a browser game
  [ ] Write C# comfortably
  [ ] Build an ASP.NET Core API
  [ ] Explain REST and HTTP
  [ ] Persist data
  [ ] Write meaningful unit tests
  [ ] Write integration tests
  [ ] Write Playwright E2E tests
  [ ] Explain the difference between the three types of tests
  [ ] Build Docker images
  [ ] Run multiple services with Docker Compose
  [ ] Explain microservice boundaries
  [ ] Explain the disadvantages of microservices
  [ ] Create a GitHub Actions pipeline
  [ ] Read and write YAML
  [ ] Configure CI/CD
  [ ] Explain Microsoft Entra ID
  [ ] Explain Client ID / Application ID
  [ ] Explain Tenant ID
  [ ] Explain OAuth 2.0 / OpenID Connect
  [ ] Protect an API
  [ ] Deploy containers to Azure
  [ ] Understand Azure Container Apps
  [ ] Use Application Insights
  [ ] Use Azure Monitor
  [ ] Investigate application failures
  [ ] Understand distributed tracing
  [ ] Understand Kubernetes
  [ ] Deploy services to local Kubernetes
  [ ] Use Helm
  [ ] Explain how Kubernetes maps to AKS
  [ ] Understand managed identity
  [ ] Understand Key Vault
  [ ] Understand CI/CD deployment identity
  [ ] Understand OIDC/workload identity
  [ ] Understand asynchronous messaging
  [ ] Understand enterprise observability
  [ ] Understand cloud cost considerations
  [ ] Document architectural decisions
  [ ] Use AI effectively for software development
  [ ] Review AI-generated code
  [ ] Identify AI-generated mistakes
  [ ] Use AI to generate useful tests
  [ ] Use AI to help debug CI/CD
  [ ] Use AI to improve documentation
  [ ] Protect secrets when using AI
  [ ] Explain when AI should and should not be trusted
  [ ] Explain the entire project to another developer

================================================================================
41. FIRST 10 STEPS
================================================================================

Do NOT try to do everything above immediately. Follow Phases 0-11 in
docs/learning/roadmap.md first; the enterprise topics in the later phases build
on that foundation.

Start here:

STEP 1
------

Install and verify the development environment.

  [ ] Install VS Code
  [ ] Install Node.js LTS
  [ ] node --version
  [ ] npm --version
  [ ] Install and configure Git
  [ ] git --version
  [ ] Verify GitHub access
  [ ] Install the .NET SDK
  [ ] dotnet --version
  [ ] Install Docker Desktop
  [ ] docker --version

STEP 2
------

Create the GitHub repository.

Name:

    learningpath

STEP 3
------

Clone the repository locally.

STEP 4
------

Create the README and documentation folders.

Explain in README.md:

  - LearningPath
  - The language Word Guess Game
  - The learning objectives

Create:

    docs/
    docs/learning/
    docs/architecture/
    docs/decisions/
    docs/runbooks/
    docs/microsoft-learn/
    docs/troubleshooting/

STEP 5
------

Create small Node.js, TypeScript, and C# programs.

Run each program locally, then commit and push the exercises to GitHub.

STEP 6
------

Learn TypeScript fundamentals and create a small TypeScript project.

Learn:

  - npm
  - package.json
  - TypeScript
  - tsconfig.json

STEP 7
------

Build a playable Word Guess Game with TypeScript and Html.

Implement:

  - Main menu
  - Settings page for choosing language, difficulty, and topic
  - Cancel and Save settings actions
  - Show saved selections on the main menu
  - Dutch, Indonesian, and English
  - Beginner and intermediate difficulty
  - Word loading, letter guesses, and Word Guess Game state
  - Scoring, game completion, and restart
  - Show the active language, difficulty, and matching high score during play
  - Track high scores separately for each language/difficulty/topic combination

STEP 8
------

Commit the playable game to Git.

Example concept:

    git add frontend/game
    git commit -m "Build first playable Word Guess Game"

STEP 9
------

Push to GitHub.

Verify:

  - Repository exists
  - README is visible
  - Source code is visible
  - Git history exists

STEP 10
-------

Create the first C# ASP.NET Core API (Game.Api).

Learn REST, JSON, dependency injection, logging, and middleware.

Connect:

      Html
       |
       v
    ASP.NET Core
       |
       v
    Game logic

After these ten steps, STOP. Continue with Phases 4-11 in
docs/learning/roadmap.md: database, word data, unit and integration testing,
contract testing, API end-to-end testing, UI accessibility testing, and UI
end-to-end testing.

Review what has been learned.

Document the lessons.

Ask AI to review the architecture and explain anything that is unclear.

Then continue to the next phase.

================================================================================
38. RECOMMENDED LEARNING LOOP
================================================================================

For each new feature, use this loop:

  1. DEFINE
     |
     v
     What should the feature do?

  2. LEARN
     |
     v
     Read Microsoft Learn / official documentation.

  3. EXPERIMENT
     |
     v
     Build a tiny example.

  4. RED
     |
     v
     Write the smallest failing test that describes the expected behavior.
     Run only that test and confirm it fails for the right reason.

  5. IMPLEMENT
     |
     v
     Add the minimum code to satisfy the failing test.

  6. GREEN
     |
     v
     Run the targeted tests again.
     Make the test pass before adding more complexity.

  7. REFACTOR
     |
     v
     Clean up the code while keeping the behavior validated by tests.
     Add relevant edge-case tests when the behavior is unclear.

  8. TEST
     |
     v
     Unit test
     Integration test
     E2E test where appropriate

  9. ASK AI
     |
     v
     Ask AI to review:
       - implementation
       - edge cases
       - tests
       - security
       - architecture

  10. VERIFY
      |
      v
      Check AI suggestions against:
        - documentation
        - source code
        - tests
        - actual behavior

  11. DOCUMENT
      |
      v
      Record what was learned.

  12. COMMIT
      |
      v
      Commit the change to Git.

  13. REVIEW
      |
      v
      Explain the feature without AI assistance.

TDD PRACTICE FOR THIS PROJECT
-----------------------------

Use a simple red-green-refactor loop for every meaningful behavior:

  1. Write a failing test for one small requirement.
  2. Name the test after the business behavior, not the implementation.
  3. Keep the test focused on one idea: valid score, invalid word list,
     guessed letter already used, etc.
  4. Run the test and confirm the failure is expected.
  5. Implement only the code needed to pass.
  6. Rerun the targeted test.
  7. Refactor carefully while preserving the passing test.
  8. Add one more test only when a new edge case appears.

Good TDD questions to ask while coding:

  - What behavior am I trying to prove?
  - What is the smallest failing test that captures it?
  - What code would make this pass without guessing?
  - What edge cases should also be protected by tests?

This is especially useful for the core game rules in Word Guess Game, such as:

  - score and solved words validation
  - level advancement and lives configuration per level
  - letter guessing rules and repeat guess penalties
  - game-over behavior when lives reach zero
  - hint usage limitations and bonus hint reward progress
  - leaderboard submission and persistence rules

================================================================================
39. CORE GAME DOMAIN
================================================================================

The game should eventually contain concepts such as:

Player
------

  Id
  DisplayName
  CreatedAt

Language
--------

  Id
  Name
  Code

Examples:

  English
  en

  Dutch
  nl

  Indonesian
  id

Difficulty
----------

  Beginner
  Intermediate

Word
----

  Id
  Language
  Difficulty
  Text

Game
----

  Id
  PlayerId
  Language
  Difficulty
  WordId
  StartedAt
  CompletedAt
  Score
  Status

Guess
-----

  Id
  GameId
  Letter
  IsCorrect
  CreatedAt

LeaderboardEntry
----------------

  PlayerId
  Language
  Difficulty
  Score
  CreatedAt

The exact domain model may change as the application evolves.

The purpose is to learn domain modeling and architecture.

================================================================================
40. IMPORTANT SECURITY DESIGN
================================================================================

Never trust the browser for:

  - Player identity
  - Score
  - Game completion
  - Word selection
  - Authorization

The browser is an untrusted client.

The server should determine:

  - Who the user is
  - Which game belongs to the user
  - Which word is being played
  - Which letters have been guessed
  - Whether a guess is valid
  - Whether the game has ended
  - What the final score is

Example:

Bad:

    Browser says:

        "I scored 100 points."

Better:

    Browser says:

        "I guessed the letter A."

    Server determines:

        Is this game valid?
        Is A already guessed?
        Is A in the word?
        Is the game still active?
        What is the new score?

This provides an excellent practical introduction to application security.

================================================================================
41. POSSIBLE FUTURE AI GAME FEATURES
================================================================================

AI can eventually become part of the application itself.

These features should be considered AFTER the core game works.

Possible examples:

  - AI-generated hints
  - AI explanation of a word
  - AI-generated example sentences
  - AI pronunciation assistance
  - AI difficulty adjustment
  - AI-generated practice questions
  - AI language-learning explanations

However:

  These features should be treated as separate learning projects.

Do not add AI to the actual game simply because AI is available.

The initial game should remain deterministic and testable.

================================================================================
42. ENTERPRISE CONCEPTS TO PRACTICE
================================================================================

The project should deliberately expose the learner to enterprise concepts.

Architecture:

  [ ] Separation of concerns
  [ ] Dependency injection
  [ ] Service boundaries
  [ ] API contracts
  [ ] Configuration
  [ ] Environment separation
  [ ] Data ownership

Security:

  [ ] Authentication
  [ ] Authorization
  [ ] Identity
  [ ] Secrets
  [ ] Managed identities
  [ ] Least privilege

Reliability:

  [ ] Health checks
  [ ] Retries
  [ ] Timeouts
  [ ] Failure handling
  [ ] Idempotency

Testing:

  [ ] Unit
  [ ] Integration
  [ ] E2E
  [ ] Regression testing
  [ ] Test automation

Operations:

  [ ] Logging
  [ ] Metrics
  [ ] Tracing
  [ ] Monitoring
  [ ] Alerts
  [ ] Incident investigation

Delivery:

  [ ] Git
  [ ] Pull requests
  [ ] CI
  [ ] CD
  [ ] Containers
  [ ] Deployment
  [ ] Rollback

Cloud:

  [ ] Azure
  [ ] Container Apps
  [ ] Identity
  [ ] Monitoring
  [ ] Storage
  [ ] Networking

AI:

  [ ] AI-assisted coding
  [ ] AI-assisted testing
  [ ] AI-assisted debugging
  [ ] AI-assisted documentation
  [ ] AI-assisted architecture review
  [ ] AI validation
  [ ] Responsible AI usage

================================================================================
43. THE CORE RULE
================================================================================

The goal is NOT:

    "Build a complicated game."

The goal is:

    "Build one evolving application that forces me to learn modern
     enterprise software development, cloud engineering, DevOps,
     testing and AI-assisted development."

The game is the motivation.

The repository is the learning record.

Git is the history.

GitHub is the collaboration platform.

GitHub Actions is the CI/CD platform.

C# / ASP.NET Core is the backend.

TypeScript / Dotnet is the desktop/game.

TypeScript / Html is the frontend/game.

Node.js / npm is the JavaScript ecosystem.

xUnit is the unit-testing platform.

ASP.NET integration testing is the integration-testing layer.

Playwright is the E2E-testing layer.

Docker is the container platform.

Kubernetes is the orchestration platform.

Microsoft Entra ID is the identity platform.

Azure Container Apps is the initial cloud deployment target.

Application Insights and Azure Monitor are the observability layer.

Microsoft Learn is the structured learning material.

AI is the development and learning assistant.

The learner remains responsible for understanding the system.

And LearningPath is the project that ties everything together.

================================================================================
END OF PLAN
================================================================================


