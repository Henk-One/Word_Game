1. TESTING - Contract - Producer side
================================================================================
cd C:\Repo\LearningPath
dotnet test services\Game.Api\tests\Contract\Game.Api.ContractTests.csproj

================================================================================
1. RUN BACKEND
================================================================================
From the repo root:

Use **Ctrl+Shift+B** to run all frontend and backend tests, then build both APIs
and the frontend. From PowerShell, run the same workflow with:

  powershell -NoProfile -ExecutionPolicy Bypass -File scripts\build.ps1

After the build succeeds, start the APIs:

  dotnet run --project services\Game.Api\Game.Api.csproj
  dotnet run --project services\Leaderboard.Api\Leaderboard.Api.csproj

Swagger UI is available at http://localhost:5151/swagger
================================================================================
2. RUN FRONTEND
================================================================================
From the repo root:

cd frontend/game
npm run build
npm run serve

================================================================================
3. PLAY
================================================================================
Open http://localhost:8080 in your browser. 

The frontend calls the game backend at http://localhost:5151 and the leaderboard at http://localhost:5000