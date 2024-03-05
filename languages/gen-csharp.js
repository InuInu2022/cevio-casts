const { execSync } = require('child_process')
const fs = require("fs")

const stdout = execSync('quicktype ./data/data.json -o ./languages/csharp/CevioCasts/Definitions.cs -l cs --namespace CevioCasts --features complete --framework SystemTextJson -S ./model/schema.json')
console.log(`stdout: ${stdout.toString("utf8")}`)

let path = "./languages/csharp/CevioCasts/Definitions.cs";
let src = fs.readFileSync(path,"utf-8");

src = src.replace(/CeVioAi/g, "CeVIO_AI");
src = src.replace(/CeVioCs/g, "CeVIO_CS");

fs.writeFileSync(path, src);

console.log("finish gen-csharp");