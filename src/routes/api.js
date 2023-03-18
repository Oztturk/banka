const express = require("express");
const router = express.Router();
const mongoose = require("mongoose");
const crypto = require("crypto");
const bcrypt = require("bcrypt");
const rateLimit = require('express-rate-limit')

const data = require("../models/user");

const saltRounds = 10;



router.use((req, res, next) => {
  console.log("Time: ", Date.now());
  next();
});


const Kayitlimiter = rateLimit({
	windowMs: 1 * 20 * 1000,
	max: 2, 
	standardHeaders: true, 
	legacyHeaders: false, 
})

// Passed
router.post("/kayit",Kayitlimiter, async (req, res) => {

  if (req.body.username == null || req.body.password == null) return res.status(406).send("Bu alanı boş bırakamazsın.")

  const username = req.body.username;

  const authData = await data.findOne({ user: username });
  if (authData) return res.status(201).send("Bu kullanıcı adı başka birisi tarafından kullanılıyor");


  const salt = bcrypt.genSaltSync(saltRounds);
  const password = bcrypt.hashSync(req.body.password, salt);
  

  new data({
    user: username,
    password: password,
    para: 1000,
    nakit: 50,
    Deneme: 0,
    logs: []

  })
    .save()
    .then(() =>
      res.json({
        Status: 200,
        Response: "Kayit Başarılı",
      })
    );
});

// Passed
router.post("/login", async (req, res) => {
  if (req.body.username == null || req.body.password == null) return res.status(406).send("Bu alanı boş bırakamazsın.")
  const salt = bcrypt.genSaltSync(saltRounds);
  const hash = bcrypt.hashSync(req.body.password, salt)

  const authData = await data.findOne({
    user: req.body.username,
    //password: req.body.password,
  });

  if (!authData) return res.status(201).send("Kullanıcı adı yada şifre yanlış");
  try{
    if (await bcrypt.compare(req.body.password, authData.password)){
      console.log("gecti")
      res.send({
        ad: authData.user,
        ID: authData._id,
        para: authData.para,
        nakit: authData.nakit,
        deneme: authData.Deneme,
        logs: authData.logs
      });
    }else{
      res.status(202).send("Kullanıcı adı yada şifre yanlış")
    }
  }catch{
    res.status(202).send("not allowed")
  }
  
});

// Passed
router.post("/paragonder", async (req, res) => {

  if (req.body.usernametosend == null || req.body.para == null) return res.status(406).send("Bu alanı boş bırakamazsın.")

  const gonderenId = req.body.userid;
  const gonderilecekUsername = req.body.usernametosend;
  
  const authData = await data.findOne({_id: req.body.userid})

  if (!authData) return res.status(201).send("Böyle Bir Kullanıcı yok");
  if (req.body.para < 1) return res.status(202).send("negatif veya nötr değer giremezsin.");
  if (authData.user == gonderilecekUsername) return res.status(202).send("Kendine para gönderemezsin.")
  if (authData.para >= req.body.para) {
    let gunceltutar = authData.para -= req.body.para;
    const filter = { _id: req.body.userid };
    //const update = { para: gunceltutar, $push: {logs: {"Para Gönderme":`${gonderilecekUsername} Adlı kullanıcıya ${req.body.para}₺ para gönderdin `}}};
    const update = { para: gunceltutar, $push: {logs: `Para Gönderme: ${gonderilecekUsername} Adlı kullanıcıya ${req.body.para}TL para gönderdin`}};
    console.log("update isteği");

    const filter2 = { user: gonderilecekUsername };
    //const update2 = { para: req.body.para, $push: {logs: {"Hesabına para geldi":` ${authData.user} Adlı Kullanıcıdan hesabına ${req.body.para}₺ para aktarıldı `}}};
    const update2 = { para: req.body.para, $push: {logs: `Hesabına para geldi: ${authData.user} Adlı Kullanıcıdan hesabına ${req.body.para}TL para aktarıldı` }};
    await data.findOneAndUpdate(filter, update).then(() => res.status(200).send(`Para başarılı bir şekilde ${gonderilecekUsername} adlı üye'ye gönderildi`));
    await data.findOneAndUpdate(filter2, update2);


  }else{
    res.status(201).send("Bu kadar paran yok")
  }
})


/*
  cek ✔
  yatir ✔
  password ✔
*/
router.post("/update", async (req, res) => {
  const authData = await data.findOne({
    _id: req.body.userid,
  });

  if (!authData) return res.status(201).send("Böyle Bir Kullanıcı yok");

  if (req.body.updatetype == "cek") {
    let para = authData.para;

    if (req.body.para == null) return res.status(406).send("Bu alanı boş bırakamazsın.")

    if (req.body.para < 1) return res.status(406).send("negatif veya nötr değer giremezsin.");
    if (para >= req.body.para) {

      let gunceltutar = para - req.body.para;
      let guncelnakit = authData.nakit + req.body.para;

      const filter = { _id: req.body.userid };
      const update = { para: gunceltutar, nakit: guncelnakit, $push: {logs: `Para çekme: Hesabından ${req.body.para}TL Çekildi `} };

      console.log("update isteği");

      await data.findOneAndUpdate(filter, update).then(() => res.status(200).send("Data güncellendi"));

    } else return res.status(202).send("yeterli paran yok.");

  } else if (req.body.updatetype == "yatir") {
    if (req.body.para == null) return res.status(406).send("Bu alanı boş bırakamazsın.")

    let nakit = authData.nakit;
    let para = authData.para;

    if (req.body.para < 1) return res.status(406).send("negatif veya nötr değer giremezsin.");
    if (nakit >= req.body.para) {
      let gunceltutar = para + req.body.para;
      let guncelnakit = authData.nakit - req.body.para;

      const filter = { _id: req.body.userid };
      const update = { para: gunceltutar, nakit: guncelnakit, $push: {logs: `Para Yatırma: Hesabına ${req.body.para}TL Para Yatırıldı.`} };

      console.log("update isteği");

      await data.findOneAndUpdate(filter, update).then(() => res.status(200).send("Data güncellendi"));

    } else return res.status(202).send("yeterli nakitin yok.");
  } else if (req.body.updatetype == "password") {

    if (req.body.password == null) return res.status(406).send("Bu alanı boş bırakamazsın.")

    const salt = bcrypt.genSaltSync(saltRounds);
    const password = bcrypt.hashSync(req.body.password, salt);

    const filter = { _id: req.body.userid };
    const update = { password: password,$push: {logs: `Şifre Değişikliği: Hesabının şifresi değiştirildi`} };

    console.log("update password isteği");

    await data.findOneAndUpdate(filter, update).then(() => res.status(200).send("Şifreniz güncellendi çıkış yapılıyor."));
  }
});


/*
router.post("/updatePassword",  async (req,res) => {
  const authData = await data.findOne({
      user: req.body.username,
  })
  if (!authData) return res.status(201).send("Böyle Bir Kullanıcı yok")

  const filter = { user: req.body.username };
  const update = { password: req.body.password };
  console.log("update password isteği")
  await data.findOneAndUpdate(filter,update).then(() => res.status(200).send("Data güncellendi"));

})
*/



module.exports = router;
