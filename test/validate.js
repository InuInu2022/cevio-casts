const Ajv = require('ajv');
const ajv = new Ajv({ allErrors: true });
const schema = require("../model/schema.json");

const testInvalidJson = require("../data/data.json");
valid = ajv.validate(schema, testInvalidJson);
if (!valid) console.log(ajv.errors);
else console.log("ok!");