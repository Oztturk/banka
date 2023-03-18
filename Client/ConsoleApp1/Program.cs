using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace BankOfYusuf
{
    internal class Program
    {
        const string tag = " | Bank Of Yusuf";
        static void Main(string[] args)
        {
            /*
                TODO:

                14/03/23 - 16/03/23
                [x] Template hazırla
                [x] Template'i API ile entegre et
                [x] API'dan dönen veriyi konsola yazdır
                [x] Update methodunu düzelt
                [x] API Bölümünde şifre değişikliği yapıldığında şifreyi hashlemesini ayarla
                [x] POST ve GET request'lerini yeni versiyona geçir                
                [x] Kullanıcı adı ve şifre bölümündeki WriteLine'ları Write yap
                [x] POST Response'larında dönen status code'ları string'den int'e çevir 
            */

            var client = new RestClient("http://127.0.0.1:3030/api"); /// LocalHost
            Console.Clear();

            BASADON:
            Console.WriteLine("Yapmak istediğiniz işlemi seçin." + tag);
            Console.WriteLine();
            Console.WriteLine("[1] Giriş yap [2] Kayıt Ol");
            int ISLEM = int.Parse(Console.ReadLine());
            if (ISLEM == 1)
            {
                Console.Clear();

                Console.WriteLine("Giriş yap" + tag);
                Console.WriteLine("");
                Console.Write("Kullanıcı adı > ");

                string kADI = Console.ReadLine();
                Console.WriteLine();
                Console.Write("Şifre > ");
                string Pass = Console.ReadLine();
                Console.Clear();

                RestRequest request = new RestRequest("/login/", Method.Post); // Kullanıcı bilgilerini doğrulamak için API'a post atar
                request.AddJsonBody(new { username = kADI, password = Pass }); // Gönderilecek bilgiyi json yapar
                var response = client.Execute(request); // request satırını çalıştırır

                if ( ((int)response.StatusCode) == 200)
                {

                    Main:
                    var repost = client.Post(request); // Aynı işlemi tekrar yapar kullanıcı main'e döndüğünde bilgileri günceller
                    JObject KullaniciDatasi = JObject.Parse(repost.Content); // API'Dan dönen veriyi json yapar

                    Console.WriteLine(string.Format("Hoşgeldin {0}{1}", KullaniciDatasi["ad"],tag ));
                    Console.WriteLine("[1] Para çek \n[2] Para Yatır\n[3] Para gönder\n[4] Bakiye Sorgula\n[5] Şifre değiştir\n[6] Loglar\n[7] Çıkış yap");


                    int BANKAISLEM = int.Parse(Console.ReadLine());
                    if (BANKAISLEM == 1)
                    {
                        Console.Clear();

                         PARACEK:
                        int bakiye = (int)KullaniciDatasi["para"];
                        int nakit = (int)KullaniciDatasi["nakit"];

                        Console.WriteLine(string.Format("Bakiye: {0} / nakit : {1} || -1 Geri", bakiye, nakit));
                        Console.Write("Çekmek istediğiniz miktar > ");
                        int miktar = int.Parse(Console.ReadLine());
                        if (miktar == -1) { Console.Clear(); goto Main; } // Girilen miktar -1'e eşitse geri döner
                        if (miktar > bakiye)  // Girilen miktar bakiyeden büyükse bu kadar paran yok der ve paraçek menüsüne geri döner
                        {
                            Console.WriteLine("Bu kadar paran yok");
                            Thread.Sleep(1500);
                            Console.Clear();
                            goto PARACEK;
                        }
                        else 
                        {
                            bakiye -= miktar; 
                            nakit += miktar;
                            KullaniciDatasi["para"] = bakiye;
                            KullaniciDatasi["nakit"] = nakit;

                            RestRequest request2 = new RestRequest("/update/", Method.Post); // Update sayfasına post atar
                            request2.AddJsonBody(new { userid = ((string)KullaniciDatasi["ID"]), updatetype = "cek", para = miktar }); // Kullanıcıdan alınan bilgileri json formatına çevirir
                            var res2 = client.Execute(request2); // request2'yi çalıştırır

                            if (((int)res2.StatusCode) == 200) //statusCode 200'e eşitse aşağıdaki bloğu çalıştırır
                            {
                                Console.WriteLine(string.Format("Para çekme işlemi başarılı Kalan Bakiye: {0}", bakiye));
                                Thread.Sleep(1500);
                                Console.Clear();

                                goto Main;
                            }

                        }

                    }
                    else if (BANKAISLEM == 2)
                    {
                        Console.Clear();

                        PARACEK:
                        int bakiye = (int)KullaniciDatasi["para"];
                        int nakit = (int)KullaniciDatasi["nakit"];

                        Console.WriteLine(string.Format("Bakiye: {0} / nakit : {1} || -1 Geri", bakiye, nakit));
                        Console.Write("Yatırmak istediğiniz miktar > ");
                        int miktar = int.Parse(Console.ReadLine());
                        if (miktar == -1) { Console.Clear(); goto Main; } // Girilen miktar -1'e eşitse geri döner
                        if (miktar > nakit) // Girilen miktar bakiyeden büyükse bu kadar paran yok der ve paraçek menüsüne geri döner
                        {
                            Console.WriteLine("Bu kadar paran yok");
                            Thread.Sleep(1500);
                            Console.Clear();
                            goto PARACEK;
                        }
                        else
                        {
                            bakiye += miktar;
                            nakit -= miktar;
                            KullaniciDatasi["para"] = bakiye;
                            KullaniciDatasi["nakit"] = nakit;

                            RestRequest request2 = new RestRequest("/update/", Method.Post); // Update sayfasına post atar
                            request2.AddJsonBody(new { userid = ((string)KullaniciDatasi["ID"]), updatetype = "yatir", para = miktar }); // Kullanıcıdan alınan bilgileri json formatına çevirir
                            var res2 = client.Execute(request2); // request2'yi çalıştırır


                            if (((int)res2.StatusCode) == 200) //statusCode 200'e eşitse aşağıdaki bloğu çalıştırır
                            {
                                Console.WriteLine(string.Format("Para yatırma işlemi başarılı Bakiye: {0}", bakiye));
                                Thread.Sleep(1500);
                                Console.Clear();

                                goto Main;
                            }
                        }

                    }else if(BANKAISLEM == 3)
                    {

                        Console.Clear();
                        Console.WriteLine("-1 Geri " + tag);
                        Console.Write("Para göndermek istediğiniz kişinin adı > ");
                        string kullanici = Console.ReadLine();

                        if (kullanici == "-1") { Console.Clear(); goto Main; } // Girilen miktar -1'e eşitse geri döner

                        Console.WriteLine();
                        Console.Write("göndermek istediğiniz miktar > ");
                        int paraMiktar = int.Parse(Console.ReadLine());

                        if (paraMiktar == -1) { Console.Clear(); goto Main; } // Girilen miktar -1'e eşitse geri döner

                        int bakiye = (int)KullaniciDatasi["para"];
                        int guncelBakiye = bakiye - paraMiktar;
                        KullaniciDatasi["para"] = guncelBakiye;

                        RestRequest requestParaGonder = new RestRequest("/paragonder/", Method.Post); // Para gönder sayfasına post atar
                        requestParaGonder.AddJsonBody(new { userid = ((string)KullaniciDatasi["ID"]) ,usernametosend = kullanici,para = paraMiktar }); // Kullanıcıdan alınan bilgileri json yapar
                        var resPara = client.Execute(requestParaGonder); // requestParaGonder'i çalıştırır

                        if (((int)resPara.StatusCode) == 200) // requestParaGonder'den dönen status code 200 ise alttaki bloğu çalıştırır
                        {
                            Console.WriteLine(resPara.Content.ToString()); // requestParaGonder'den dönen mesajı yazdırır
                            Console.WriteLine();
                            Thread.Sleep(1500);
                            Console.Clear();
                            goto Main;
                        }
                        else if(((int)resPara.StatusCode) == 201 || ((int)resPara.StatusCode) == 202 || ((int)resPara.StatusCode) == 406) //StatusCode 201 / 202 / 406' ise alttaki bloğu çalıştırır
                        {
                            Console.WriteLine(resPara.Content.ToString()); // requestParaGonder'den dönen mesajı yazdırır 
                            Thread.Sleep(1500);
                            Console.Clear();
                            goto Main;
                        }

                    }
                    else if(BANKAISLEM == 4)
                    {
                        Console.Clear();
                        var para = ((int)KullaniciDatasi["para"]);
                        var nakit = ((int)KullaniciDatasi["nakit"]);
                        Console.WriteLine(string.Format("Bankadaki bakiyeniz: {0}\nNakit: {1}",para,nakit));
                        Console.WriteLine("R Tuşuna basarak geri dönebilirsiniz");
                        ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
                        if (consoleKeyInfo.Key == ConsoleKey.R)
                        {
                            Console.Clear();
                            goto Main;
                        }
                    }
                    else if(BANKAISLEM == 5)
                    {
                        Console.Clear();
                        Console.WriteLine("-1 Geri" + tag);
                        Console.Write("Yeni şifre > ");
                        string sifre1 = Console.ReadLine();

                        if (sifre1 == "-1") { Console.Clear(); goto Main; } // -1 Girildiğinde geri döner
                        RestRequest request2 = new RestRequest("/update/", Method.Post); // update sayfasına post atar
                        request2.AddJsonBody(new { userid = ((string)KullaniciDatasi["ID"]), updatetype = "password", password = sifre1}); // kullanıcıdan alınan veriyi json yapar
                        var res2 = client.Execute(request2); // request2'yi çalıştırır

                        if (((int)res2.StatusCode) == 200) // request2'den dönen status code 200 ise alttaki bloğu çalıştırır
                        {
                            Console.WriteLine("Şifreniz güncellendi çıkış yapılıyor.");
                            Thread.Sleep(1500);
                            Console.Clear();
                            goto BASADON;
                        }
                    }else if(BANKAISLEM == 6)
                    {
                        Console.Clear();
                        foreach (var item in KullaniciDatasi["logs"]) // foreach döngüsü ile hesap haraketlerini alır
                        {

                            Console.WriteLine(item);
                        }
                        Console.WriteLine("-------------------------------------------");
                        Console.WriteLine("R Tuşunda basarak geri dönebilirsin.");
                        ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
                        if (consoleKeyInfo.Key == ConsoleKey.R)
                        {
                            Console.Clear();
                            goto Main;
                        }
                    }else if(BANKAISLEM == 7)
                    {
                        Console.Clear();
                        Console.WriteLine("Çıkış Yapılıyor");
                        Thread.Sleep(1700);
                        Console.Clear();
                        goto BASADON;
                    }
                    else
                    {
                        goto BASADON;
                    }
                }
                else
                {
                    Console.WriteLine(response.Content);
                    Thread.Sleep(1500);
                    Console.Clear();
                    goto BASADON;
                }
            }
            else if(ISLEM == 2)
            {
                REGISTERFLAG:
                Console.Clear();
                Console.WriteLine("Kayıt ol || -1 Geri");
                Console.Write("Kullanıcı adı > ");
                string kADI = Console.ReadLine();

                if (kADI == "-1") { Console.Clear(); goto BASADON; } 

                Console.WriteLine();
                Console.Write("Şifre > ");

                string Pass = Console.ReadLine();
                if (Pass == "-1") { Console.Clear(); goto BASADON; }
                
                RestRequest request = new RestRequest("/kayit/", Method.Post);// üstteki işlem ile aynı yazma üşendim
                request.AddJsonBody(new { username = kADI, password = Pass });
                var res = client.Execute(request);

                if (((int)res.StatusCode) == 201)
                {
                    Console.WriteLine(res.Content.ToString());
                    Thread.Sleep(1500);
                    goto REGISTERFLAG;
                }else if(((int)res.StatusCode) == 200)
                {
                    Console.WriteLine("Kayıt işlemi başarılı");
                    Thread.Sleep(1500);
                    Console.Clear();
                    goto BASADON;
                }else if(((int)res.StatusCode) == 429 || ((int)res.StatusCode) == 406)
                {
                    Console.WriteLine(res.Content.ToString());
                    Thread.Sleep(1500);
                    Console.Clear();
                    goto BASADON;
                }

            }
            else
            {
                Thread.Sleep(1500);
                Console.Clear();
                goto BASADON;
            }

            

        }
    }
}
