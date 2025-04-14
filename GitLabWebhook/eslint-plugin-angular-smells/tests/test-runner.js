// test-runner.js
const fs = require("fs");
const path = require("path");

const testDir = path.join(__dirname, ".");

fs.readdirSync(testDir)
  .filter((file) => file.endsWith("test.js"))
  .forEach((file) => {
    console.log(`\nRunning ${file}`);
    require(path.join(testDir, file));
  });
