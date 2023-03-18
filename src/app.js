const express = require("express")
const crypto = require('crypto')
const mongoose = require('mongoose')
const dotenv = require('dotenv/config')

const api = require("./routes/api")
const data = require('./models/user.js')

const app = express()

app.use(express.json())
app.use("/api",api)

app.get("/kayit",  async (req,res) => {
    res.send("xd")
})

app.get("/",  async (req,res) => {
    res.redirect("https://www.youtube.com/watch?v=dQw4w9WgXcQ")
})

app.listen(process.env.PORT, () => {
    console.log(`✔ Server ${process.env.PORT} Portunu dinlyor`)
    mongoose.connect(process.env.MONGOURL).then(() => console.log('✔ Database'));
});