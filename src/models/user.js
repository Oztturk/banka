const { model, Schema } = require("mongoose");

const schema = new Schema({
  user: String,
  password: String,
  para: Number,
  nakit: Number,
  Deneme: Number,
  logs: []
});

module.exports = model("user", schema, "users");
