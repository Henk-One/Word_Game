const esbuild = require("esbuild");
const fs = require("node:fs");
const path = require("path");
const dotenv = require("dotenv");

const envPath = path.resolve(__dirname, "..", ".env");
const envResult = dotenv.config({ path: envPath });

if (envResult.error) {
  throw new Error(`Failed to load frontend environment file at ${envPath}`, {
    cause: envResult.error
  });
}

const gameApiUrl = process.env.GAME_API_URL;
const leaderboardApiUrl = process.env.LEADERBOARD_API_URL;

if (!gameApiUrl || !leaderboardApiUrl) {
  throw new Error("GAME_API_URL and LEADERBOARD_API_URL must be set in frontend/game/.env");
}

async function build() {
  const outputDirectory = path.resolve(__dirname, "..", "dist");
  const publicDirectory = path.resolve(__dirname, "..", "public");

  fs.rmSync(outputDirectory, { recursive: true, force: true });
  fs.mkdirSync(outputDirectory, { recursive: true });

  await esbuild.build({
    entryPoints: [path.resolve(__dirname, "..", "src", "main.ts")],
    bundle: true,
    minify: true,
    sourcemap: true,
    outfile: path.join(outputDirectory, "main.js"),
    define: {
      "process.env.GAME_API_URL": JSON.stringify(gameApiUrl),
      "process.env.LEADERBOARD_API_URL": JSON.stringify(leaderboardApiUrl)
    }
  });

  fs.cpSync(publicDirectory, outputDirectory, {
    recursive: true,
    filter: (source) => path.resolve(source) !== path.join(publicDirectory, "dist")
  });
  console.log("Frontend build completed; public assets copied to dist.");
}

build().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
