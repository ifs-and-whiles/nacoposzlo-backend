using System;
using System.Collections.Generic;
using System.Linq;
using Billy.PolishReceiptRecognitionAlgorithm.Geometry;
using Billy.PolishReceiptRecognitionAlgorithm.Grammar;
using Billy.PolishReceiptRecognitionAlgorithm.OcrJson;
using Billy.PolishReceiptRecognitionAlgorithm.StringProcessing;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.Receipts
{
    public class TestReceipts
    {
        private static readonly Dictionary<string, ExpectedReceipt> ExpectedReceipts =
            new Dictionary<string, ExpectedReceipt>
            {
                {
                    "receipt_id_13_text_annotations.json", new ExpectedReceipt(
                        products: new[]
                        {
                            Product("SALATKA SELER 320G.B", "2", "3,59", "7,18", "B"),
                            Product("KUKURDZA ZLOC 340 .B", "2", "4,29", "8,58", "B"),
                            Product("BROKUL SZT", "1", "3,99", "3,990", "C"),
                            Product("KALAFIOR SZT", "1", "4,99", "4,99", "C"),
                            Product("FASOLKA.CZERW KON .B", "2", "2,99", "5,98", "B"),
                            Product("ANAN.KAW.GIA 565 g", "2", "5,69", "11,38", "B"),
                            Product("BRZOS.GIANA 820 g", "1", "7,49", "7,49", "B"),
                            Product("WOD CISOW NIEG 1.5.A", "6", "1,59", "9,54", "A"),
                            Product("BISZKOPTY LU 120G A", "4", "3,39", "13,56", "A"),
                            Product("DANIO 4X140G WANIL", "1", "5,49", "5,49", "C"),
                            Product("MLEKO LAC 3.2% 1L", "4", "2,59", "10,36", "C"),
                            Product("CHRUPKI TS KUKURYD", "4", "1,19", "4,76", "C"),
                            Product("SER GOUDA 250 g", "1", "5,99", "5,99", "C"),
                            Product("WAF JAB/RYZ BIO35G.D", "4", "3,99", "15,96", "C"),
                            Product("TS SAL RZYM MINI", "1", "3,99", "3,99", "C"),
                            Product("PLYN D/N LUD 1.5C .A", "1", "7,99", "7,99", "A"),
                            Product("CUKIERKIKROWKA2509", "1", "5,99", "5,99", "A"),
                            Product("PA TO TES3W BIA10R", "1", "12,99", "12,99", "A"),
                            Product("WOR TS TO 20L30SZT", "2", "2,59", "5,18", "A"),
                            Product("APRT ALOE GLIC ZAP", "2", "3,49", "6,98", "A"),
                            Product("PALUSZKI JUNI 280g", "2", "3,19", "6,38", "A"),
                            Product("GELLWE SERNIK 170g", "1", "5,49", "5,49", "B"),
                            Product("TORTILLA FUN 4X25", "2", "4,99", "9,98", "A"),
                            Product("CUKTER PUDER 500G", "1", "2, 39", "2,39", "B"),
                            Product("IS ZIEM GALA 2kg", "1", "4,99", "4,99", "C"),
                            Product("TS KASZA MANN 500G", "1", "1,99", "1,99", "C"),
                            Product("MUSLI 5 TROP 300G", "1", "4,99", "4,99", "C"),
                            Product("MUSLI 5 BAK 300G", "1", "4,99", "4,99", "C"),
                            Product("KAWA DALL CREM 1KG", "1", "89,99", "89,99", "A"),
                            Product("WKLAD ANT 50SZT", "1", "8,99", "8,99", "B"),
                            Product("POMIDORY KROJO 400", "2", "1,99", "3,98", "B"),
                            Product("KOSZT DOSTAWY... A", "1", "11,99", "11,99", "A"),
                            Product("KOSZT TOREB", "1", "2,00", "2,00", "A"),
                            Product("Rabat", "6", "8,34", "-1,20", "A", "ZA"),
                        },
                        seller: Seller("TESCO /POLSKR/ Sp. 2 0.0"),
                        date: Date("2019-10-03", "2019-10-03 18443"),
                        taxNumber: TaxNumber("526-10-37-737"),
                        amount: Amount(315.32m, "SUMA PLN 315,32"))
                },
                {
                    "receipt_id_14.json", new ExpectedReceipt(
                        products: new[]
                        {
                            Product("2220691050049 0BUWIE H107-SERENA-0", "1", "139, 99", "139, 99", "A", "SZT")
                        },
                        seller: Seller("CCC"),
                        date: Date("2018-03-26", "2018-03-26 nr wydr. 280059"),
                        taxNumber: TaxNumber("NIP 692-22 00-609"),
                        amount: Amount(139.99M, "SUMA PLN 139, 99"))
                },
                {
                    "receipt_id_15.json", new ExpectedReceipt(
                        products: new[]
                        {
                            Product("HYDROMARIN DLA CALEJ RODZINY RODZI . 3519B", "1", "12, 70", "12, 70", "B", "op"),
                            Product("PLAST. NA ODCISKI Z KWASEM SALICYL . 11880B", "1", "12, 50", "12,50", "B", "op"),
                        },
                        seller: Seller("Apteka Farmaceutow"),
                        date: Date("2019-10-08", "2019-10-08 nr wydr. 036720"),
                        taxNumber: TaxNumber("NIP 644-187-76-00"),
                        amount: Amount(25.20M, "SUMA PLN 25, 20"))
                },
                {
                    "receipt_id_16.json", new ExpectedReceipt(
                        products: new[]
                        {
                            Product("5902224690539 OBUWIE 8PH61 11 VM6 39", "1", "169,99", "169. 99", "A"),
                            Product("5907728785170 AKCESORIA RENOWATOR CZER", "1", "22,99", "22,99", "A"),
                        },
                        seller: Seller("\"BRAWO\" Sp Z o.0. ul Dwor cowa 19"),
                        date: Date("13-05-2019", "13-05-2019 4042400"),
                        taxNumber: TaxNumber("NIP: 551-21-85-838"),
                        amount: Amount(192.98M, "SUMa: PLN 192. 98"))
                },
                {
                    "receipt_id_17_text_annotations.json", new ExpectedReceipt(
                        products: new[]
                        {
                            Product("BABYDREAM 1 NEWBO \\ BL", "1", "10 , 99", "10 , 99", "B"),
                            Product("Uuzgl . rabat : - 3 , 00 st . C", null, null, "13 , 99", null),
                            Product("BABYDREAM 2 MINI \\ BL", "1", "19 , 99", "19 , 99", "B"),
                            Product("PREGNAPLUS W KAPS \\ BL", "1", "39 , 99", "39 , 99", "B"),
                            Product("BATISTE DRY SHAMP \\ AX", "1", "11 , 99", "11 , 99", "A"),
                            Product("Uwzgl . rabat : - 3 , 30 st . c", null, null, "15 , 29", null),
                            Product("Rabat Rossne _ cia basic", null, null, "- 0 , 33", "B"),
                            Product("Rabat KLUB Babydream 2 Min", null, null, "- 4 , 50", "B"),
                            Product("Rabat KLUB PregnaPlus w ka", null, null, "- 8 , 00", "B")
                        },
                        seller: Seller("Rossmann SDP Sklep nr 1187"),
                        date: Date("2019-10-23 15:20", "2019 - 10 - 23 15 : 20 NIP 727 - 001 - 91 - 83 9748"),
                        taxNumber: TaxNumberWithPrefix("2019 - 10 - 23 15 : 20 NIP 727 - 001 - 91 - 83 9748"),
                        amount: Amount(70.13m, "SUMA PLN 70 , 13"))
                },
                {
                    "receipt_id_18.json", new ExpectedReceipt(
                        products: new[]
                        {
                            Product("badanie usg ciaza", "1", "300.00", "300. 0", "QE"),
                        },
                        seller: Seller("MWU DOBREUSG"),
                        date: Date("30-10-2019", "30-10-2019 WO06159"),
                        taxNumber: TaxNumber("NIP: 9452210261"),
                        amount: Amount(300M, "SUMA : PLN 300.00"))
                },
                {
                    "receipt_id_19.json", new ExpectedReceipt(
                        products: new[]
                        {
                            Product("KONCENTRAT 200 g", "3", "2, 89", "8, 67", "B"),
                            Product("DROZDZE INSTANT A", "2", "1 , 29", "2, 58", "A"),
                            Product("SOK JA MAR BAN 30 .D", "2", "3, 89", "7, 780", null),
                            Product("KUKR ZLOC 425 ML B", "2", "4, 29", "8 , 58", "B"),
                            Product("TS PIECZARK500g", "1", "4, 99", "4, 99", "C"),
                            Product("CEBULA LUZ", "1, 492", "3, 99", "5, 95", "C"),
                            Product("CYTRYNY LUZ.", "1, 300", "4, 79", "6, 23", "B"),
                            Product("MARCHEW LUZ.", "1, 492", "2, 99", "4, 46", "C"),
                            Product("TS PIET NAT PECZ", "2", "1, 49", "2, 98", "C"),
                            Product("SELER KORZEN LUZ", "0, 638", "4, 99", "3, 18", "C"),
                            Product("JABLKA LIGOL LUZ", "1, 500", "2, 99", "4, 49", "C"),
                            Product("SER WIEJS . 200G PIA . D", "2", "1 , 59", "3, 18", "C"),
                            Product("FASOLKA. CZERW KON . B", "1", "2 , 99", "2, 99", "B"),
                            Product("WOD CISOW NIEG 1.5. A", "12", "1 , 59", "19, 08", "A"),
                            Product("MAKARON 500G MUSZE . D", "2", "3, 89", "7, 78", "C"),
                            Product("DANONKI GR/BR 4X90", "1", "5, 89", "5, 89", "C"),
                            Product("CHRUPKI TS KUKURYD", "6", "1 , 19", "7, 140", null),
                            Product("MASLO EX POLS 200G. D", "2", "5, 99", "11, 98", "C"),
                            Product("JOGURT NATU 180g", "2", "1 , 39", "2, 78", "C"),
                            Product("SOK BF J BRZ M 300", "2", "3, 89", "7, 78", "C"),
                            Product("PIETR KORZ LUZ I", "1, 110", "6, 99", "7, 76", "C"),
                            Product("MAKA BASIA TORTOWA", "2", "3, 49", "6, 98", "C"),
                            Product("OSOLE SOL 1 KG", "1", "1 , 79", "1, 79", "A"),
                            Product("PIEL/M PAMP BOX 5", "1", "79 , 99", "79 , 99", "B"),
                            Product("Mini kokardka 400g", "2", "3, 99", "7, 98", "C"),
                            Product("GAE MLE FRE 500ML", "1", "3, 99", "3, 99", "A"),
                            Product("MYD BARW MNISZ100", "2", "1 , 69", "3, 38", "A"),
                            Product("Papier nawilzany", "4", "2, 99", "11, 96", "A"),
                            Product("PLYN D/T ZIAJ OLIW", "1", "10 , 99", "10, 99", "A"),
                            Product("TS ZIEM GALA 2kg", "2", "6, 99", "13, 98", "C"),
                            Product("KOPER WLOSKI 20X2g", "1", "5, 29", "5, 29", "B"),
                            Product("JAJA W/WYB L 10SZT", "1", "8, 99", "8, 990", null),
                            Product("PASSATA POM BIO", "1", "4, 99", "4, 99", "B"),
                            Product("KAW ZI EXT BAR 1Kg", "1", "79, 99", "79, 99", "A"),
                            Product("POMIDORY KROJO 400", "2", "1 , 99", "3, 98", "B"),
                            Product("PAPIER TOAL. FREDF", "2", "3, 99", "7, 98", "A"),
                            Product("SOL D/K PF FLOWER", "1", "3, 49", "3, 49", "A"),
                            Product("KOSZT DOSTAWY ...... A", "1", "13, 99", "13, 99", "A"),
                            Product("Rabat", "12", "15, 48", "-3, 60", "A", "ZA"),
                            Product("Rabat", "2", "7", "-1, 56", "C", "ZA"),
                        },
                        seller: Seller("TESCO /POLSKA/ Sp. 2 0. 0."),
                        date: Date("05-11-2019", "2019-11-05 203811"),
                        taxNumber: TaxNumber("NIP 526-10-37-737"),
                        amount: Amount(400.83M, "SUMA PLN 400 , 83"))
                },
                {
                    "receipt_id_20.json", new ExpectedReceipt( //todo heavy header problems - unexpected format
                        products: new[]
                        {
                            Product("04. Piwne Podziemie Whats Up? 0,5", "1", "15, 00", "15, 00", "A"),
                            Product("Patatas bravas", "1", "12 , 00", "12, 00", "B"),
                        },
                        seller: Seller("Upojeni Sp. Z 0.0."),
                        date: Date("25.10.2019 18:02", "0008199 #001 KIEROWNIK 2019-10-25 18:02"),
                        taxNumber: TaxNumber("NIP 6342841332 nr : 10024"),
                        amount: Amount(27M, "SUMA PLN 27 , 00"))
                },
                {
                    "receipt_id_22.json", new ExpectedReceipt(
                        products: new[]
                        {
                            Product("DOMESTOS PLYN X", "1", "9, 49", "9, 49", "A"),
                            Product("WAWEL KROWKA", "0, 09", "19, 90", "1,79", "A"),
                            Product("CUKIERKI MICHALKI X", "0, 234", "19, 90", "4, 66", "A"),
                            Product("KWASEK CYTRYNOWY", "4", "2,99", "11,96", "A"),
                            Product("PRZYPRAWA CURRY *", null, "0, 79", "3, 16"),
                            Product("PRZYPRAWA MAJERANEK *", null, "0,69", "2,07", "B"),
                            Product("PRZYPRAWA TYMIANEK *", null, "0, 99", "2,97"),
                            Product("WRIGLEYS GUMA II *", null, "3,49", "3, 49"),
                            Product("PRZYPRAWA KEBAB-GYRO *", null, "0, 99", "2,97"),
                            Product("PRZYP . ZIOLA TOSKAN.", null, null, "8, 97"),
                            Product("BIO CYTRYNY SWIEZE", "2", "5, 99", "11, 98"),
                            Product("BIO ZIEMNIAK SWIEZY", "1", "7,49", "7, 49"),
                            Product("WORKI NA SMIECI 35L", null, null, "5, 18", "A"),
                            Product("OCET SPIRYTUSOWY", null, "39", "1,89", "A"),
                            Product("JACOBS KR_NUNG KAWA", "1", "21 , 79", "21,79", "A"),
                            Product("BIO KALAF . SW. , LUZ", "0,668", "12, 99", "8, 68", "C"),
                            Product("WINIARY MAJONEZ", "1", "4, 79", "4,79"),
                            Product("JAJA EKO M, L, XL", "2", "9, 29", "18,58"),
                            Product("HERBATKA ZIOLOWA *", null, "3", "3,95"),
                            Product("HERB . ZIOL . EKSP.", null, "2,29", "2,29", "A"),
                            Product("HERB . ZIOL , EKSP. :", null, "2, 29", "2,29", "A"),
                            Product("CHRUPKI KUKURYDZ . *", null, "1, 19", "2,38", "C"),
                            Product("CHRUPKI KUKURYDZ. *", null, "1, 19", "2, 38", "C")
                        },
                        seller: Seller("Lidl sp. zo.0. sp. k."),
                        date: Date("09.03.2019", "2019-03-09 nr wydr. 134698"),
                        taxNumber: TaxNumber("NIP 781-18-97-358"),
                        amount: Amount(145.20m, "SUMA PLN 145,20"))
                },
                {
                    "receipt_id_23.json", new ExpectedReceipt(
                        products: new[]
                        {
                            Product("DOMESTOS PLYN", "1", "9, 49", "9, 49", "A"),
                            Product("WAWEL KROWKA", "0,09", "19, 90", "1, 79", "A"),
                            Product("CUKIERKI MICHALKI", "0, 234", "19,90", "4, 66", "A"),
                            Product("KWASEK CYTRYNOWY 4 *", "2", "99", "11,96"),
                            Product("PRZYPRAWA CURRY", "4", "0.79", "3, 16"),
                            Product("PRZYPRAWA MAJERANEK", "3", "0, 69", "2, 07", "B"),
                            Product("PRZYPRAWA TYMIANEK", "3", "0, 99", "2, 97"),
                            Product("WRIGLEYS GUMA II", "1", "3,49", "3, 49", "B"),
                            Product("PRZYPRAWA KEBAB-GYRO", "3", "0,99", "2, 97"),
                            Product("PRZYP . ZIOLA TOSKAN 3 *", "2", "99", "8,97", "B"),
                            Product("BIO CYTRYNY SWIEZE", "2", "5, 99", "11 ,98", "B"),
                            Product("BIO ZIEMNIAK SWIEZY", "1", "7,49", "49", "C"),
                            Product("WORKI NA SMIECI", null, null, "35", "L"),
                            Product("OCET SPIRYTUSOWY", null, "1", "7"),
                            Product("JACOBS KR_NUNG KAWA", "17", "21,79", "21,79", "A"),
                            Product("BIO KALAF . SW. , LUZ", "0, 668", "12,99", "8, 68", "C"),
                            Product("WINIARY MAJONEZ *", null, "4,79", "4,79", "B"),
                            Product("JAJA EKO M, L, XL", "2", "9,29", "18, 58", "C"),
                            Product("HERBATKA ZIOLOWA *", null, "3, 95", "3, 95", "A"),
                            Product("HERB . ZIOL . EKSP", "1", "2 ,29", "2 , 29"),
                            Product("HERB . ZIOL . EKSP", "1", "2,29", "2,29", "A"),
                            Product("CHRUPKI KUKURYDZ", "2", "1, 19", "2, 38", "C"),
                            Product("CHRUPKI KUKURYDZ .", "2", "1, 19", "2, 38", "C"),
                        },
                        seller: Seller("Lidl sp. z 0.0, sp. k."),
                        date: Date("09.03.2019", "2019-03-09 nr wydr. 134698"),
                        taxNumber: TaxNumber("NIP 781-18-97-358"),
                        amount: Amount(145.20m, "SUMA PLN 145,20"))
                },
                {
                    "receipt_id_24.json", new ExpectedReceipt(
                        products: new[]
                        {
                            Product("Zk.PALUSZ.RYBNE.45", "1", "13,88", "13,88", "C", null),
                            Product("PALUSZKI SEROWE 27", "2", "11,77", "23,54", "C", null),
                            Product("PEPSI. LIME 2X1,5L", "1", "5,99", "5,99", "A", null),
                            Product("PEPSI. LIME 2X1,5L", "1", "5,99", "5,99", "A", null),
                            Product("SOK. .POMAR. 1L 69597", "2", "6,59", "13,18", "C", null),
                            Product("ARRAB. 400G. SOS2972", "1", "6,98", "6,98", "B", null),
                            Product("OL CZA.KROJ.860/55", "1", "9,09", "9,09", "C", null),
                            Product("MLEKO UHT 3,2%.1L", "1", "2,38", "2,38", "C", null),
                            Product("MLEKO UHT 3,2%.1L", "1", "2,38", "2,38", "C", null),
                            Product("MLEKO UHT 3,2%.1L", "1", "2,38", "2,38", "C", null),
                            Product("MLEKO UHT 3,2%.1", "1", "2,38", "2,38", "C", null),
                            Product("KUKURYDZA.340/285", "1", "2,19", "2,19", "C", null),
                            Product("SER WE. Z PAP. .MYSLI", "0,084", "39,99", "3,36", "C", null),
                            Product("SER JANOS 949392 2", "1", "5,97", "5,97", "C", null),
                            Product("CZEK.BIALA.KARM. 0", "1", "3,48", "3,48", "A", null),
                            Product("LINDOR.ASSORT.C.20", "1", "15,99", "15,99", "A", null),
                            Product("MARCEPAN.230G", "1", "3,99", "3,99", "A", null),
                            Product("MARCEPAN.2300", "1", "3,99", "3,99", "A", null),
                            Product("ROS.BURG.KLAS.170G", "1", "8,99", "8,99", "B", null),
                            Product("D.CZOS.SUSZ.GRAN.2", "1", "1,19", "1,19", "C", null),
                            Product("CAM.N/CIE.ZUR.200G", "1", "9,98", "9,98", "C", null),
                            Product("CAM.N/CIE.ZUR.2000", "1", "9,98", "9,98", "C", null),
                            Product("GROSZ.KONSERW.40", "1", "1,79", "1,79", "C", null),
                            Product("GROSZ.KONSERW.40", "1", "1,79", "1,79", "C", null),
                            Product("ARRAB. .400G.SOS2972", "1", "6,98", "6,98", "B", null),
                            Product("TOSOS WEDZ.ATL.100", "2", "6,99", "13,98", "C", null),
                            Product("BURGER. LIMOUS 4 400", "1", "9,99", "9,99", "C", null),
                            Product("MIEL. Z SZYN.500G M", "1", "5,99", "5,99", "C", null),
                            Product("KUR ZAGRODOWY FILE", "0,413", "12,50", "5,16", "C", null),
                            Product("D BANANY LUZ 34041", "3,462", "4,99", "17,28", "C", null),
                            Product("KUKURYDZA.340/285", "1", "2,19", "2,19", "C", null),
                            Product("StON.GIC.PRAZ13", "1", "2,49", "2,49", "C", null),
                            Product("KOT.PIK.MIFSZ.PIEF", "1", "1,60", "1,60", "B", null),
                            Product("PRZYP DO KURCZ 30G", "1", "1,19", "1,19", "B", null),
                            Product(".PRZYPRAWA DO GYR", "1", "1,58", "1,58", "B", null),
                            Product("CHEDDAR REP PLA150", "1", "6,49", "6,49", "C", null),
                            Product("CHEDDAR REP PLA150", "1", "6,49", "6,49", "C", null),
                            Product("PTASIE ML .KARM.360", "1", "11,99", "11,99", "A", null),
                            Product("ANANAS.PLASTRY.34", "1", "4,79", "4,79", "C", null),
                            Product("KABAN.Z LOSOSIA 10", "1", "3,89", "3,89", "C", null),
                            Product("HER. .ROOIBOS SEN.HO", "1", "3,58", "3,58", "A", null),
                            Product("HERBATA EXPR.ROOIB", "1", "2,80", "2,80", "C", null),
                            Product("D CEBULA 1KG SIATK", "1", "1,98", "1,98", "C", null),
                            Product("D.GRAP.POMELO LUZ.", "0,854", "4,99", "4,26", "C", null),
                            Product("ACTKA PIETRUSZ PEC", "2", "1,39", "2,78", "C", null),
                            Product("ZENTIS.KART 100G", "2", "5,97", "11,94", "A", null),
                            Product("RECZNIK. FOXY MEGA", "1", "5,99", "5,99", "A", null),
                            Product("CZOSNEK BIALY 250G", "1", "5,99", "5,99", "C", null),
                            Product("CUKINIA LUZ", "0,516", "6,79", "3,50", "C", null),
                            Product("PAPRYKA.CZERWONA", "0,47", "8,98", "4,22", "C", null),
                            Product("D.GRAP POMELO LUZ.", "0,782", "4,99", "3,90", "C", null),
                            Product("LAYS-BAK GRILL P 2", "1", "6,27", "6,27", "C", null),
                            Product("LAYS.BAKED YOG/H 2", "1", "6,27", "6,27", "C", null),
                            Product("LAYS-BAKED SAL 200", "1", "6,27", "6,27", "C", null),
                            Product("LAYS-ZIELONA CEBU", "1", "6,84", "6,84", "C", null),
                            Product("CHIO-NACH-CHILL-19", "1", "5,29", "5,29", "C", null),
                            Product("LAJKON.PALUSZKI 18", "1", "3,13", "3,13", "C", null),
                            Product("RABAT DO PTASIE ML.KARM.", null, "360", "-2,00", "A", null),
                        },
                        seller: Seller("HIPERMARKET AUCHAN"),
                        date: Date("22.12.2020", "2020-12-22 nr wydr. 170471/0417"),
                        taxNumber: TaxNumber("NIP 526-03-09-174"),
                        amount: Amount(345.95m, "SUMA PLN 345,95")
                    )
                },
                {
                    "receipt_id_26.json", new ExpectedReceipt(
                        products: new[]
                        {
                            Product("ZIOLA PROWANS 8g", "4", "1,09", "4,36", "B", null),
                            Product("KONCENTRAT 200 g", "3", "2,59", "7,77", "B", null),
                            Product("BANANY LUZ", "3,058", "2,99", "9,14", "B", null),
                            Product("OLEJ KUJAWSKI 1L .D", "1", "6,99", "6,99", "C", null),
                            Product("KUKR ZLOC 425 ML B", "1", "3,39", "3,39", "B", null),
                            Product("TS PIECZARK500g", "1", "4,99", "4,99", "C", null),
                            Product("CEBULA LUZ", "1,034", "3,99", "4,13", "C", null),
                            Product("MARCHEW LUZ.", "2,220", "0,99", "2,200", null, null),
                            Product("JABLKA LIGOL LUZ", "2,210", "3,49", "7,71", "C", null),
                            Product("ORZE ZIEM 400g LIG", "1", "11,49", "11,49", "B", null),
                            Product("CIAST JEZY CLA CZE", "2", "3,99", "7,98", "A", null),
                            Product("WOD CISOW NIEG 1.5.A", "12", "1,59", "19,08", "A", null),
                            Product("PRZYPR CURRY KOT .A", "4", "1,09", "4,36", "A", null),
                            Product("ZUP POMI KUR R 190.B", "2", "4,79", "9,58", "B", null),
                            Product("MLEKO LAC 3.2% 1L", "2", "2,89", "5,78", "C", null),
                            Product("MASLO EX POLS 200G.D", "4", "5,99", "23,96", "C", null),
                            Product("SMIETANKA 30% 200G.D", "1", "3,09", "3,09", "C", null),
                            Product("RYZ 1kg RISA PAP", "1", "4,99", "4,99", "C", null),
                            Product("WODA MAMA NIEG 1,5", "12", "1, 79", "21,48", "A", null),
                            Product("PIETR KORZ LUZ I", "1,490", "0,99", "1,48", "C", null),
                            Product("DAN BV MAKARON B", "2", "4,79", "9,58", "B", null),
                            Product("WODA DOBROW NG1.5L.A", "12", "1,59", "19,08", "A", null),
                            Product("PL.CORN FL PEWNIAK.D", "1", "3,00", "3,00", "C", null),
                            Product("DAN BV IND Z/W 190", "2", "4,79", "9,58", "B", null),
                            Product("L.UB.SPAGHET.500G", "1", "3,89", "3,89", "C", null),
                            Product("MUSZT MIOD PR 185G", "1", "2,49", "2,49", "A", null),
                            Product("Mini kokardka 400g", "2", "3,99", "7,98", "C", null),
                            Product("MUS HIPP J/B/M 100", "3", "4,19", "12,57", "B", null),
                            Product("MUS HIPP B/G/P 120", "3", "4,69", "14,07", "B", null),
                            Product("PALUSZKI JUNI 280g", "1", "4,19", "4,19", "A", null),
                            Product("LAYS PIE PAP 125g", "2", "4,99", "9,98", "B", null),
                            Product("NAP COLA 0.85", "4", "3,79", "15,16", "A", null),
                            Product("KUBUS MUS BANA 0.1", "4", "1,99", "7,96", "C", null),
                            Product("JAJA W/WYB L 10SZT", "1", "8,99", "8,99", "C", null),
                            Product("MUSLI 5 BAK 300G", "2", "4,99", "9,98", "C", null),
                            Product("PIE/MAJ PAMPERS 48", "2", "39,99", "79,98", "B", null),
                            Product("SER CHEDDAR MATU L", "0,314", "44,90", "14,10", "C", null),
                            Product("WAF KUKUR 100g", "4", "3,49", "13,96", "C", null),
                            Product("1 PASSATA POM BIO x", null, "4,99", "4,99", "B", null),
                            Product("BAT VAR EN AA 4SZT", "2", "8,99", "17,98", "A", null),
                            Product("KOSZT DOSTAWY A", "1", "11,99", "11,99", "A", null),
                            Product("Rabat", "6", "8,34", "-2,40", "A", "ZA"),
                            Product("Rabat", "2", "8,98", "-1,00", "B", "ZA"),
                        },
                        seller: Seller("/ -"),
                        date: Date("29.12.2019", "2019-12-29 73291"),
                        taxNumber: TaxNumber("NIP 526-10-37-737"),
                        amount: Amount(442.05m, "SUMA PLN 442,05")
                    )
                }
            };

        private static readonly TestReceipt[] AllReceipts =
        {
            new TestReceipt(
                id: 1,
                lines: new[]
                {
                    "Big Star Limited Społka z o. o.",
                    "Al. Wojska Polskiego 21/21a",
                    "62-800 Kalisz",
                    "Sklep Firmowy 8627",
                    "ul. Jana III Sobieskiego 6A",
                    "41-300 Dabrowa Gornicza",
                    "NIP 618-00-31-613",
                    "2018-03-26 19:11 41225",
                    "PARAGON FISKALNY",
                    "CADEN 307 W34 L32 X 1 x249,99 249,99A",
                    "RONALD 408 W34 L32 X 1 x179,99 179,99A",
                    "Rabat -90",
                    "89,99A",
                    "SPRZEDAZ OPODATKOWANA A 339,98",
                    "PTU A 23,00 % 63,57",
                    "SUMA PTU 63,57",
                    "SUMA PLN 339.98",
                    "00025 #Kasa 1 Kasjer nr 1",
                    "2018-03-26 19:11",
                    "0E41EOFF3A5712312312309113120983129083120938F",
                    "CAO 1501364705",
                    "NR sys 5820",
                    "Karta platnicza 339.98 PLN",
                    "Nr transakcji 5820"
                },
                headerLines: new LinesRange(0, 8),
                productLines: new LinesRange(9, 12),
                taxesLines: new LinesRange(13, 15),
                amountLine: 16,
                footerLines: new LinesRange(17, 23),
                actualProducts: new IReceiptProduct[]
                {
                    Product("CADEN 307 W34 L32 X", "1", "249,99", "249,99", "A"),
                    Product("RONALD 408 W34 L32 X", "1", "179,99", "179,99", "A"),
                    Product("Rabat", null, null, "-90"),
                    Unrecognized("89,99A"),
                },
                actualSeller: Seller("Big Star Limited Społka z o. o."),
                actualDate: Date("2018-03-26 19:11", "2018-03-26 19:11 41225"),
                actualTaxNumber: TaxNumber("NIP 618-00-31-613"),
                actualReceiptAmount: Amount(339.98M, "SUMA PLN 339.98")),

            new TestReceipt(
                id: 2,
                lines: new[]
                {
                    "KLOS M.KRZYCA SP.JAWNA",
                    "40-664 KATOWICE, UL.NAPIERSKIEGO 43",
                    "SKLEP FIRMOWY - KAWIARENKA",
                    "NIP 954-264-02-26",
                    "2019-10-07 nr wydr.647997",
                    "PARAGON FISKALNY",
                    "CHLEB WIEJSKI MALY 1 szt * 4,30 = 4,30 C",
                    "BULKA MASLANA 0.05 KG",
                    "3 szt * 0,85 =2,55 C",
                    "SERNIK Z BRZOSKWINIA",
                    "0,56 kg * 32,00 = 17,92 B",
                    "CIASTO Z WISNIA",
                    "0,52 kg * 25,00 = 13,00 B",
                    "Sprzed. opod. PTU B 30,92",
                    "Kwota B 08,00% 2,29",
                    "Sprzed. opod. PTU C 6,85",
                    "Kwota C 05,00% 0,33",
                    "Podatek PTU 2,62",
                    "SUMA PLN 37,77",
                    "0234/1729 #01 16:48",
                    "19S82-7YFB3-LoXYY-R5WRM-HHI70",
                    "BEE 13409073",
                    "Sposób zapłaty: Karta: 37,77",
                    "Numer 50/77365",
                    "Numer Kod: 7884"
                },
                headerLines: new LinesRange(0, 5),
                productLines: new LinesRange(6, 12),
                taxesLines: new LinesRange(13, 17),
                amountLine: 18,
                footerLines: new LinesRange(19, 25),
                actualProducts: new []
                {
                    Product("CHLEB WIEJSKI MALY", "1", "4,30", "4,30", "C", "szt"),
                    Product("BULKA MASLANA 0.05 KG", "3", "0,85", "2,55", "C", "szt"),
                    Product("SERNIK Z BRZOSKWINIA", "0,56", "32,00", "17,92", "B", "kg"),
                    Product("CIASTO Z WISNIA", "0,52", "25,00", "13,00", "B", "kg"),
                },
                actualSeller: Seller("KLOS M.KRZYCA SP.JAWNA"),
                actualDate: Date("2019-10-07", "2019-10-07 nr wydr.647997"),
                actualTaxNumber: TaxNumber("NIP 954-264-02-26"),
                actualReceiptAmount: Amount(37.77M, "SUMA PLN 37,77")),

            new TestReceipt(
                id: 3,
                lines: new[]
                {
                    "SKLEP DROB-WĘDLINY KONACH BEATA",
                    "UL. SZEWSKA 11, 40-649 KATOWICE",
                    "kasa nr 1",
                    "NIP: 6442323482",
                    "08-10-2019 W101474",
                    "PARAGON FISKALNY",
                    "KMINKOWA 1.000*0.19 0.19C",
                    "POLED/SCHAB 0.188*39.00 7.33C",
                    "RAZEM: 7.52",
                    "##STRONO##",
                    "KMINKOWA -1.000*0.19 -0.19C",
                    "KMINKOWA 0.190*33.50 6.37C",
                    "RAZEM: 13.70",
                    "SP.OP.C: 13.70 PTU 5.00% 0.65",
                    "SUMA PTU 0.65",
                    "SUMA: PLN 13.70",
                    "F100041 #1 B.Administrator",
                    "08-102019 08:51",
                    "Karta: 13.70"
                },
                headerLines: new LinesRange(0, 5),
                productLines: new LinesRange(6,12),
                taxesLines: new LinesRange(13, 14),
                amountLine: 15,
                footerLines: new LinesRange(16, 18),
                actualProducts: new IReceiptProduct[]
                {
                    Product("KMINKOWA", "1.000", "0.19", "0.19", "C"),
                    Product("POLED/SCHAB", "0.188", "39.00", "7.33", "C"),
                    Product("RAZEM",null, null, "7.52"),
                    Unrecognized("##STRONO##"),
                    Product("KMINKOWA", "-1.000", "0.19", "-0.19", "C"),
                    Product("KMINKOWA", "0.190", "33.50", "6.37", "C"),
                    Product("RAZEM", null, null, "13.70")
                },
                actualSeller: Seller("SKLEP DROB-WĘDLINY KONACH BEATA"),
                actualDate: Date("08-10-2019", "08-10-2019 W101474"),
                actualTaxNumber: TaxNumber("NIP: 6442323482"),
                actualReceiptAmount: Amount(13.70M, "SUMA: PLN 13.70")),

            new TestReceipt(
                id: 4,
                lines: new []{
                    "BIEDRONKA ''CODZIENNIE NISKIE CENY'' B2362",
                    "42-436 PILICA UL. KRAKOWSKA",
                    "JERONIMO MARTINS POLSKA S.A.",
                    "62-025 KOSTRZYN UL.ZNIWNA 5",
                    "NIP 779-10-11-327",
                    "2018-05-08 Wt 399372",
                    "PARAGON FISKALNY",
                    "Torba T-SHIRT A 2,000 x 0,25 0,50",
                    "WodaMinNiskCechi2L A 12.000 x 1.69 20,28",
                    "MydłoWpłynieLinda1 A 2,000 x 4,99 9,98",
                    "Melon żółty luz C 0,798 x 4,99 3,98",
                    "rabat -0,83",
                    "CiastoDigesGull250g A 1,000 x 4,99 4,99",
                    "Ser Rycki Ed 150g C 2,000 x 3,89 7,78",
                    "Sal.Obsyp.Nuss.100g C 2,000 x 3,99  7,98",
                    "Kostka Fala 2szt A 1,000 x 4,99 4,99",
                    "KawaMielonaPeru250G A 1,000 x 9,99 9,99",
                    "KawaMielonaHonduras250g A 1,000 x 9,99 9,99",
                    "Kwiat Saintpaulia B 3,000 x 8,99 26,97",
                    "Sprzedaż opodatk. A 60,72",
                    "Kwota PTU 23% 11,35",
                    "Sprzedaż opodatk. B 26,97",
                    "Kwota PTU B 8% 2,00",
                    "Sprzedaż opodatk. C 18,91",
                    "Kwota PTU C 5% 0,90",
                    "Suma PTU 14,25",
                    "Suma PLN 106,60",
                    "Stopka paragonu"
                }, 
                headerLines: new LinesRange(0,6),
                productLines:new LinesRange(7, 18),
                taxesLines: new LinesRange(19, 25),
                amountLine: 26,
                footerLines: new LinesRange(27, 27),
                actualProducts: new IReceiptProduct[]
                {
                    Product("Torba T-SHIRT A", "2,000", "0,25", "0,50"),
                    Product("WodaMinNiskCechi2L A", "12.000", "1.69", "20,28"),
                    Product("MydłoWpłynieLinda1 A", "2,000", "4,99", "9,98"),
                    Product("Melon żółty luz C", "0,798", "4,99", "3,98"),
                    Product("rabat", null, null, "-0,83"),
                    Product("CiastoDigesGull250g A", "1,000", "4,99", "4,99"),
                    Product("Ser Rycki Ed 150g C", "2,000", "3,89", "7,78"),
                    Product("Sal.Obsyp.Nuss.100g C", "2,000", "3,99", "7,98"),
                    Product("Kostka Fala 2szt A", "1,000", "4,99", "4,99"),
                    Product("KawaMielonaPeru250G A", "1,000", "9,99", "9,99"),
                    Product("KawaMielonaHonduras250g A", "1,000", "9,99", "9,99"),
                    Product("Kwiat Saintpaulia B", "3,000", "8,99", "26,97"),
                },
                actualSeller: Seller("BIEDRONKA ''CODZIENNIE NISKIE CENY'' B2362"),
                actualDate: Date("2018-05-08","2018-05-08 Wt 399372"),
                actualTaxNumber: TaxNumber("NIP 779-10-11-327"),
                actualReceiptAmount: Amount(106.60M, "Suma PLN 106,60")),

            new TestReceipt(
                id: 5,
                lines: new []{
                    "BIEDRONKA ''CODZIENNIE NISKIE CENY'' 1789",
                    "42-440 OGRODZIENIEC ul. Pl. Wolności 37",
                    "JERONIMO MARTINS POLSKA S.A.",
                    "62-025 KOSTRZYN UL.ZNIWNA 5",
                    "NIP 779-10-11-327",
                    "2018-05-11 Pt 196390",
                    "PARAGON FISKALNY",
                    "Torba T-SHIRT A 1,000 x 0,25 0,25A",
                    "P Okocim G 0,33l A 4,000 x 1,69 6,76A",
                    "Piwo NaMiodzi 0,5l A 1,000 x 3,99 3,99A",
                    "Piwo Kozel 0,5l A 1,000 x 2,99 2,99A",
                    "SmotMangVitFr0,75l C 1,000 x 5,99 5,99C",
                    "Śl po duńsk 200g C 1,000 x 4,99 4,99C",
                    "SurówkaSmakoł. 300g B 1,000 x 2,29 2,29B",
                    "ChipsyWiejTwaroż200g B 1,000 x 4,49 4,49B",
                    "rabat -0,50",
                    "Jabłko Szamp luz C 0,828 x 3,49 2,89C",
                    "Banan luz B 0,742 x 4,49 3,33B",
                    "Sprzedaż opodatk. A 13,99",
                    "Kwota PTU 23% 2,62",
                    "Sprzedaż opodatk. B 9,61",
                    "Kwota PTU B 8% 0,71",
                    "Sprzedaż opodatk. C 13,87",
                    "Kwota PTU C 5% 0,66",
                    "Suma PTU 3,99",
                    "Suma PLN 37,47",
                    "Stopka paragonu"
                },
                headerLines: new LinesRange(0,6),
                productLines: new LinesRange(7, 17),
                taxesLines: new LinesRange(18, 24),
                amountLine: 25,
                footerLines: new LinesRange(26, 26),
                actualProducts: new IReceiptProduct[]
                {
                    Product("Torba T-SHIRT A", "1,000", "0,25", "0,25", "A"),
                    Product("P Okocim G 0,33l A", "4,000", "1,69", "6,76", "A"),
                    Product("Piwo NaMiodzi 0,5l A", "1,000", "3,99", "3,99", "A"),
                    Product("Piwo Kozel 0,5l A", "1,000", "2,99", "2,99", "A"),
                    Product("SmotMangVitFr0,75l C", "1,000", "5,99", "5,99", "C"),
                    Product("Śl po duńsk 200g C", "1,000", "4,99", "4,99", "C"),
                    Product("SurówkaSmakoł. 300g B", "1,000", "2,29", "2,29", "B"),
                    Product("ChipsyWiejTwaroż200g B", "1,000", "4,49", "4,49", "B"),
                    Product("rabat", null, null, "-0,50"),
                    Product("Jabłko Szamp luz C", "0,828", "3,49", "2,89", "C"),
                    Product("Banan luz B", "0,742", "4,49", "3,33", "B"),
                },
                actualSeller: Seller("BIEDRONKA ''CODZIENNIE NISKIE CENY'' 1789"),
                actualDate: Date("2018-05-11","2018-05-11 Pt 196390"),
                actualTaxNumber: TaxNumber("NIP 779-10-11-327"),
                actualReceiptAmount: Amount(37.47M, "Suma PLN 37,47")),

            new TestReceipt(
                id: 6,
                lines: new []
                {
                    "HIPERMARKET AUCHAN",
                    "41-400 DĄBROWA G., J.III SOBIESKIEGO 6",
                    "TEL.(32) 639 86 00",
                    "AUCHAN POLSKA SP.Z O.O.",
                    "UL.PUŁAWSKA 46 05-500 PIASECZNO",
                    "www.auchan.pl NR GIOś E0002057WBW",
                    "NIP 5260309174",
                    "2018-04-09 645592",
                    "PARAGON FISKALNY",
                    "STOPKI DAM.443384 1 x4,00 4,00A",
                    "STOPKI DAM.443394 1 x4,00 4,00A",
                    "STOPKI DAMSK.41816 1 x4,50 4,50A",
                    "STOPKI DAMSK.41816 1 x4,50 4,50A",
                    "STOPKI DAMS.418209 1 x3,00 3,00A",
                    "STOPKI DAMS.418116 1 x3,50 3,50A",
                    "SPRZEDAż OPODATK. A 23,50",
                    "PTU A 23,00% 4,39",
                    "SUMA PTU 4,39",
                    "SUMA PLN 23,50",
                    "Stopka paragonu"
                },
                headerLines: new LinesRange(0, 8),
                productLines: new LinesRange(9, 14),
                taxesLines: new LinesRange(15, 17),
                amountLine: 18,
                footerLines: new LinesRange(19, 19),
                actualProducts: new []
                {
                    Product("STOPKI DAM.443384", "1", "4,00", "4,00", "A"),
                    Product("STOPKI DAM.443394", "1", "4,00", "4,00", "A"),
                    Product("STOPKI DAMSK.41816", "1", "4,50", "4,50", "A"),
                    Product("STOPKI DAMSK.41816", "1", "4,50", "4,50", "A"),
                    Product("STOPKI DAMS.418209", "1", "3,00", "3,00", "A"),
                    Product("STOPKI DAMS.418116", "1", "3,50", "3,50", "A")
                },
                actualSeller: Seller("HIPERMARKET AUCHAN"),
                actualDate: Date("2018-04-09","2018-04-09 645592"),
                actualTaxNumber: TaxNumber("NIP 5260309174"),
                actualReceiptAmount: Amount(23.50M, "SUMA PLN 23,50")),

            new TestReceipt(
                id: 7,
                lines: new []
                {
                    "Apteka Farmaceutów",
                    "MZPHARMA Małgorzata Gurgul-Pełka",
                    "40-613 Katowice, ul.Jankego 130",
                    "tel. 32 2500155",
                    "NIP 644-187-76-00",
                    "2019-10-07 nr wydr.069132",
                    "PARAGON FISKALNY",
                    "PARACETAMOL 80 MG 10 CZOP.DOODBYT.7190B",
                    "1 op * 4,50 = 4,50 B",
                    "Bez recepty 4,50",
                    "NASIVIN SOFT 0.025% AER.DONOSA 10M.6978B",
                    "1 op * 21,90 = 21,90B",
                    "Bez recepty 21,90",
                    "Sprzed. opod. PTU B 26,40",
                    "Kwota B 08,00% 1,96",
                    "Podatek PTU 1,96",
                    "RAZEM PLN 26,40",
                    "SUMA PLN 26,40",
                    "Stopka paragonu"
                },
                headerLines: new LinesRange(0, 6),
                productLines: new LinesRange(7, 12),
                taxesLines: new LinesRange(13, 15),
                amountLine: 17,
                footerLines: new LinesRange(18, 18),
                actualProducts: new IReceiptProduct[]
                {
                    Product("PARACETAMOL 80 MG 10 CZOP.DOODBYT.7190B", "1", "4,50", "4,50", "B", "op"),
                    Product("Bez recepty", null, null, "4,50"), 
                    Product("NASIVIN SOFT 0.025% AER.DONOSA 10M.6978B", "1", "21,90", "21,90", "B", "op"),
                    Product("Bez recepty", null, null, "21,90"),
                },
                actualSeller: Seller("Apteka Farmaceutów"),
                actualDate: Date("2019-10-07","2019-10-07 nr wydr.069132"),
                actualTaxNumber: TaxNumber("NIP 644-187-76-00"),
                actualReceiptAmount: Amount(26.40M, "SUMA PLN 26,40")),

            new TestReceipt(
                id: 8,
                lines: new []
                {
                    "KŁOS M.KURZYCA SP.JAWNA",
                    "40-664 KATOWICE, UL.NAPIERSKIEGO 43",
                    "SKLEP FIRMOWY - KAWIARENKA",
                    "NIP 954-264-02-26",
                    "2019-10-11 nr wydr.739546",
                    "PARAGON FISKALNY",
                    "BUŁKA MAŚLANA 0,05 KG",
                    "6 szt * 0,85 = 5,10 C",
                    "Sprzed. opod. PTU C 5,10",
                    "Kwota C 05,00% 0,24",
                    "Podatek PTU 0,24",
                    "SUMA PLN 5,10",
                    "Stopka paragonu"
                },
                headerLines: new LinesRange(0, 5),
                productLines: new LinesRange(6, 7),
                taxesLines: new LinesRange(8, 10),
                amountLine: 11,
                footerLines: new LinesRange(12, 12),
                actualProducts: new []
                {
                    Product("BUŁKA MAŚLANA 0,05 KG", "6", "0,85", "5,10", "C", "szt")
                },
                actualSeller: Seller("KŁOS M.KURZYCA SP.JAWNA"),
                actualDate: Date("2019-10-11","2019-10-11 nr wydr.739546"),
                actualTaxNumber: TaxNumber("NIP 954-264-02-26"),
                actualReceiptAmount: Amount(5.10M, "SUMA PLN 5,10")),

            new TestReceipt(
                id: 9,
                lines: new []
                {
                    "EMPiK S.A.",
                    "Marszałkowska 116/122 00-017 Warszawa",
                    "Salon CH POGORIA",
                    "Sobieskiego 6 Dąbrowa Górnicza",
                    "Centrum Wsparcia Klienta: +48 22 4627250",
                    "Nr.rej. GIOś E00110813WBW",
                    "NIP 526-020-74-27",
                    "2018-04-09 nr wydr.092744",
                    "PARAGON FISKALNY",
                    "KB6-1592 A 1*4,99= 4,99 A",
                    "5907736309610",
                    "WIERSZE I WIERSZ WAD 1*29,99= 29,99 D",
                    "9788328043114",
                    "WIERSZE I WIERSZYKBD 1*29,99= 29,99 D",
                    "9788328045170",
                    "Sprzed. opod. PTU A 4,99",
                    "Kwota A 23.00% 0,93",
                    "Sprzed. opod. PTU D 59,98",
                    "Kwota D 05,00% 2,86",
                    "Podatek PTU 3,79",
                    "SUMA PLN 64,97",
                    "Stopka paragonu"
                },
                headerLines: new LinesRange(0, 8),
                productLines: new LinesRange(9, 14),
                taxesLines: new LinesRange(15, 19),
                amountLine: 20,
                footerLines: new LinesRange(21, 21),
                actualProducts: new IReceiptProduct[]
                {
                    Product("KB6-1592 A", "1", "4,99", "4,99", "A"),
                    Unrecognized("5907736309610"),
                    Product("WIERSZE I WIERSZ WAD", "1", "29,99", "29,99", "D"),
                    Unrecognized("9788328043114"),
                    Product("WIERSZE I WIERSZYKBD", "1", "29,99", "29,99", "D"),
                    Unrecognized("9788328045170")
                },
                actualSeller: Seller("EMPiK S.A."),
                actualDate: Date("2018-04-09","2018-04-09 nr wydr.092744"),
                actualTaxNumber: TaxNumber("NIP 526-020-74-27"),
                actualReceiptAmount: Amount(64.97M, "SUMA PLN 64,97")),

            new TestReceipt(
                id: 10,
                lines: new []
                {
                    "AGATA Spółka Akcyjna",
                    "40-203 Katowice, Al. Roździeńskiego 93",
                    "Salon w Katowicach",
                    "NIP 634-019-74-76",
                    "2019-09-27 nr wydr.002153/0264",
                    "PARAGON FISKALNY",
                    "80964-10,5 POJEMNIK SZKLANY DO PRZECHOWY",
                    "3*11,99 35.97 A",
                    "80964-22 POJEMNIK SZKLANY DO PRZECHOWYWA",
                    "2*16,99 33,98 A",
                    "20065 ZAPARZACZ NIERDZEWNY Z UCHWYTEM 4,",
                    "1*6,99 6,99 A",
                    "27411004 KOMPLET POJEMNIKóW SZKLANYCH DO",
                    "1*6,99 6,99 A",
                    "PT3856 OBSIDIAN PATELNIA 24 CM EX.......",
                    "1*129,00 129,00 A",
                    "Sprzed. opod. PTU A 212,93",
                    "Kwota A 23,00% 39,82",
                    "Podatek PTU 39,82",
                    "SUMA PLN 212,93",
                    "Stopka paragonu"
                },
                headerLines: new LinesRange(0, 5),
                productLines: new LinesRange(6, 15),
                taxesLines: new LinesRange(16, 18),
                amountLine: 19,
                footerLines: new LinesRange(20, 20),
                actualProducts: new []
                {
                    Product("80964-10,5 POJEMNIK SZKLANY DO PRZECHOWY", "3", "11,99", "35.97", "A"),
                    Product("80964-22 POJEMNIK SZKLANY DO PRZECHOWYWA", "2", "16,99", "33,98", "A"),
                    Product("20065 ZAPARZACZ NIERDZEWNY Z UCHWYTEM 4,", "1", "6,99", "6,99", "A"),
                    Product("27411004 KOMPLET POJEMNIKóW SZKLANYCH DO", "1", "6,99", "6,99", "A"),
                    Product("PT3856 OBSIDIAN PATELNIA 24 CM EX.......", "1", "129,00", "129,00", "A")
                },
                actualSeller: Seller("AGATA Spółka Akcyjna"),
                actualDate: Date("2019-09-27","2019-09-27 nr wydr.002153/0264"),
                actualTaxNumber: TaxNumber("NIP 634-019-74-76"),
                actualReceiptAmount: Amount(212.93M, "SUMA PLN 212,93")),

            new TestReceipt(
                id: 11,
                lines: new []
                {
                    "DECATHLON Sp. z o.o.",
                    "ul.Geodezyjna 76 03-290 Warszawa",
                    "Decathlon Sosnowiec 445",
                    "GIOs: E0005018WBW",
                    "NIP 951-18-55-233",
                    "2019-10-11 82690",
                    "PARAGON FISKALNY",
                    "2181821 BALANCE SOFT DISC",
                    "1 x59,99 59,99A",
                    "SPRZEDAZ OPODATK. A 59,99",
                    "PTU A 23.00 % 11,22",
                    "SUMA PLN 59,99",
                    "Stopka paragonu"
                },
                headerLines: new LinesRange(0, 6),
                productLines: new LinesRange(7, 8),
                taxesLines: new LinesRange(9, 10),
                amountLine: 11,
                footerLines: new LinesRange(12, 12),
                actualProducts: new []
                {
                    Product("2181821 BALANCE SOFT DISC", "1", "59,99", "59,99", "A")
                },
                actualSeller: Seller("DECATHLON Sp. z o.o."),
                actualDate: Date("2019-10-11","2019-10-11 82690"),
                actualTaxNumber: TaxNumber("NIP 951-18-55-233"),
                actualReceiptAmount: Amount(59.99M,   "SUMA PLN 59,99")),

            new TestReceipt(
                id: 12, 
                lines: new []
                {
                    "DECATHLON Sp. z o.o.",
                    "ul.Geodezyjna 76 03-290 Warszawa",
                    "Decathlon Sosnowiec 445",
                    "GIOs: E0005018WBW",
                    "NIP 951-18-55-233",
                    "2019-03-11 20:11 82690",
                    "PARAGON FISKALNY",
                    "2143086 G 500 S1 WHITE 1 x99,99 99,99A",
                    "2690434 TB 160 X3 1 x12,99 12,99A",
                    "SPRZEDAZ OPODATKOWANA A 112,98",
                    "PTU A 23,00 % 21,13",
                    "SUMA PTU 21,13",
                    "SUMA PLN 112,98",
                    "Stopka paragonu"
                },
                headerLines: new LinesRange(0, 6),
                productLines: new LinesRange(7, 8),
                taxesLines: new LinesRange(9, 11),
                amountLine: 12,
                footerLines: new LinesRange(13, 13),
                actualProducts: new []
                {
                    Product("2143086 G 500 S1 WHITE", "1", "99,99", "99,99", "A"),
                    Product("2690434 TB 160 X3", "1", "12,99", "12,99", "A")
                },
                actualSeller: Seller("DECATHLON Sp. z o.o."),
                actualDate: Date("2019-03-11 20:11", "2019-03-11 20:11 82690"),
                actualTaxNumber: TaxNumber("NIP 951-18-55-233"),
                actualReceiptAmount: Amount(112.98M,  "SUMA PLN 112,98")),

            new TestReceipt(
                id: 13,
                lines: new []
                {
                    "TESCO /POLSKA/ Sp. z o.o.",
                    "ul. Kapelanka 56, 30-347 Kraków",
                    "TESCO HIPERMARKET - TYCHY",
                    "ul. Towarowa 2, 43-100 Tychy",
                    "tel. (32) 32 34 000",
                    "526-10-37-737",
                    "2019-10-03 18443",
                    "PARAGON FISKALNY",
                    "SALATKA SELER 320G.B 2 x3,59 7,18B",
                    "KUKURYDZA ZLOC 340 .B 2 x4,29 8,58B",
                    "BROKUL SZT 1 x3,99 3,99C",
                    "KALAFIOR SZT 1 x4,99 4,99C",
                    "FASOLKA.CZERW KON .B 2 x2,99 5,98B",
                    "ANAN.KAW.GIA 565 g 2 x5,69 11,38B",
                    "BRZOS.GIANA 820 g 1 x7,49 7,49B",
                    "WOD CISOW NIEG 1.5.A 6 x1,59 9,54A",
                    "BISZKOPTY LU 120G .A 4 x3,39 13,56A",
                    "DANIO 4X140G WANIL 1 x5,49 5,49C",
                    "MLEKO LAC 3.2% 1L 4 x2,59 10,36C",
                    "CHRUPKI TS KUKURYD 4 x1,19 4,76C",
                    "SER GOUDA 250 g 1 x5,99 5,99C",
                    "WAF JAB/RYZ BIO35G.D 4 x3,99 15,96C",
                    "TS SAL RZYM MINI 1 x3,99 3,99C",
                    "PLYN D/N LUD 1.5C..A 1 x7,99 7,99A",
                    "CUKIERKIKROWKA205g 1 x5,99 5,99A",
                    "PA TO TES3W BIA10R 1 x12,99 12,99A",
                    "WOR TS TO 20L30SZT 2 x2,59 5,18A",
                    "APRT ALOE GLIC ZAP 2 x3,49 6,98A",
                    "PALUSZKI JUNI 280g 2 x3,19 6,38A",
                    "GELLWE SERNIK 170g 1 x5,49 5,49B",
                    "TORTILLA FUN 4X25  2 x4,99 9,98A",
                    "CUKIER PUDER 500G 1 x2,39 2,39B",
                    "TS ZIEM GALA 2kg 1 x4,99 4,99C",
                    "TS KASZA MANN 500G 1 x1,99 1,99C",
                    "MUSLI 5 TROP 300G 1 x4,99 4,99C",
                    "MUSLI 5 BAK 300G 1 x4,99 4,99C",
                    "KAWA DALL CREM 1KG 1 x89,99 89,99A",
                    "WKLAD ANT 50SZT 1 x8,99 8,99B",
                    "POMIDORY KROJO 400 2 x1,99 3,98B",
                    "KOSZT DOSTAWY......A 1 x11,99 11,99A",
                    "KOSZT TOREB 1 x2,00 2,00A",
                    "Podsuma: 316,52",
                    "Rabat 6 ZA 8,34 zl -1,20A",
                    "Podsuma: 315,32",
                    "SPRZEDAŻ OPODATKOWANA A 181,37",
                    "PTU A 23,00% 33,91",
                    "SPRZEDAŻ OPODATKOWANA B 61,46",
                    "PTU B 8,00% 4,55",
                    "SPRZEDAŻ OPODATKOWANA C 72,49",
                    "PTU C 5,00% 3,45",
                    "SUMA PTU 41,91",
                    "SUMA PLN 315,32",
                    "Stopka paragonu",
                },
                headerLines: new LinesRange(0, 7),
                productLines: new LinesRange(8, 43),
                taxesLines: new LinesRange(44, 50),
                amountLine: 51,
                footerLines: new LinesRange(52, 52),
                actualProducts: new IReceiptProduct[]
                {
                    Product("SALATKA SELER 320G.B", "2", "3,59", "7,18", "B"),
                    Product("KUKURYDZA ZLOC 340 .B", "2", "4,29", "8,58", "B"),
                    Product("BROKUL SZT", "1", "3,99", "3,99", "C"),
                    Product("KALAFIOR SZT", "1", "4,99", "4,99", "C"),
                    Product("FASOLKA.CZERW KON .B", "2", "2,99", "5,98", "B"),
                    Product("ANAN.KAW.GIA 565 g", "2", "5,69", "11,38", "B"),
                    Product("BRZOS.GIANA 820 g", "1", "7,49", "7,49", "B"),
                    Product("WOD CISOW NIEG 1.5.A", "6", "1,59", "9,54", "A"),
                    Product("BISZKOPTY LU 120G .A", "4", "3,39", "13,56", "A"),
                    Product("DANIO 4X140G WANIL", "1", "5,49", "5,49", "C"),
                    Product("MLEKO LAC 3.2% 1L", "4", "2,59", "10,36", "C"),
                    Product("CHRUPKI TS KUKURYD", "4", "1,19", "4,76", "C"),
                    Product("SER GOUDA 250 g", "1", "5,99", "5,99", "C"),
                    Product("WAF JAB/RYZ BIO35G.D", "4", "3,99", "15,96", "C"),
                    Product("TS SAL RZYM MINI", "1", "3,99", "3,99", "C"),
                    Product("PLYN D/N LUD 1.5C..A", "1", "7,99", "7,99", "A"),
                    Product("CUKIERKIKROWKA205g", "1", "5,99", "5,99", "A"),
                    Product("PA TO TES3W BIA10R", "1", "12,99", "12,99", "A"),
                    Product("WOR TS TO 20L30SZT", "2", "2,59", "5,18", "A"),
                    Product("APRT ALOE GLIC ZAP", "2", "3,49", "6,98", "A"),
                    Product("PALUSZKI JUNI 280g", "2", "3,19", "6,38", "A"),
                    Product("GELLWE SERNIK 170g", "1", "5,49", "5,49", "B"),
                    Product("TORTILLA FUN 4X25", "2", "4,99", "9,98", "A"),
                    Product("CUKIER PUDER 500G", "1", "2,39", "2,39", "B"),
                    Product("TS ZIEM GALA 2kg", "1", "4,99", "4,99", "C"),
                    Product("TS KASZA MANN 500G", "1", "1,99", "1,99", "C"),
                    Product("MUSLI 5 TROP 300G", "1", "4,99", "4,99", "C"),
                    Product("MUSLI 5 BAK 300G", "1", "4,99", "4,99", "C"),
                    Product("KAWA DALL CREM 1KG", "1", "89,99", "89,99", "A"),
                    Product("WKLAD ANT 50SZT", "1", "8,99", "8,99", "B"),
                    Product("POMIDORY KROJO 400", "2", "1,99", "3,98", "B"),
                    Product("KOSZT DOSTAWY......A", "1", "11,99", "11,99", "A"),
                    Product("KOSZT TOREB", "1", "2,00", "2,00", "A"),
                    Product("Podsuma", null, null, "316,52"),
                    Product("Rabat", "6", "8,34", "-1,20", "A", "ZA"),
                    Product("Podsuma",null, null, "315,32")
                },
                actualSeller: Seller("TESCO /POLSKA/ Sp. z o.o."),
                actualDate: Date("2019-10-03", "2019-10-03 18443"),
                actualTaxNumber: TaxNumber("526-10-37-737"),
                actualReceiptAmount: Amount(315.32M, "SUMA PLN 315,32")),

            new TestReceipt(
                id: 14,
                lines: new []
                {
                    "CCC",
                    "CCC S.A. ul. Strefowa 6, 59-101 Polkowice",
                    "www.ccc.eu",
                    "Salon Firmowy CCC",
                    "41-400 Dąbrowa Górnicza, Salon 1050",
                    "ul. Jana III Sobieskiego 6",
                    "NIP 692-22-00-609",
                    "2018-03-26 nr wydr.280059",
                    "PARAGON FISKALNY",
                    "2220691050049 OBUWIE W107-SERENA-0",
                    "1 SZT * 139,99 = 139,99 A",
                    "Sprzed. opod. PTU A 139,99",
                    "Kwota A 23,00% 26,18",
                    "Podatek PTU 26,18",
                    "SUMA PLN 139,99",
                    "Stopka paragonu"
                },
                headerLines: new LinesRange(0, 8),
                productLines: new LinesRange(9, 10),
                taxesLines: new LinesRange(11, 13),
                amountLine: 14,
                footerLines: new LinesRange(15, 15),
                actualProducts: new []
                {
                    Product("2220691050049 OBUWIE W107-SERENA-0", "1", "139,99", "139,99", "A", "SZT")
                },
                actualSeller: Seller("CCC"),
                actualDate: Date("2018-03-26", "2018-03-26 nr wydr.280059"),
                actualTaxNumber: TaxNumber("NIP 692-22-00-609"),
                actualReceiptAmount: Amount(139.99M, "SUMA PLN 139,99")),

            new TestReceipt(
                id: 15,
                lines: new []
                {
                    "Apteka Farmaceutów",
                    "MZPHARMA Małgorzata Gurgul-Pełka",
                    "40-613 Katowice, ul.Jankego 130",
                    "tel. 32 2500155",
                    "NIP 644-187-76-00",
                    "2019-10-08 nr wydr.036720",
                    "PARAGON FISKALNY",
                    "HYDROMARIN DLA CAŁEJ RODZINY RODZI.3519B",
                    "1 op * 12,70 = 12,70 B",
                    "Bez recepty 12,70",
                    "PLAST.NA ODCISKI Z KWASEM SALICYL.11880B",
                    "1 op * 12,50 = 12,50 B",
                    "Bez recepty 12,50",
                    "Sprzed. opod. PTU B 25,20",
                    "Kwota B 08,00% 1,87",
                    "Podatek PTU 1,87",
                    "RAZEM PLN 25,20",
                    "SUMA PLN 25,20",
                    "Stopka paragonu"
                }, 
                headerLines: new LinesRange(0, 6),
                productLines: new LinesRange(7, 12),
                taxesLines: new LinesRange(13, 15),
                amountLine: 17,
                footerLines: new LinesRange(18, 18),
                actualProducts: new IReceiptProduct[]
                {
                    Product("HYDROMARIN DLA CAŁEJ RODZINY RODZI.3519B", "1", "12,70", "12,70", "B", "op"),
                    Product("Bez recepty", null, null, "12,70"),
                    Product("PLAST.NA ODCISKI Z KWASEM SALICYL.11880B", "1", "12,50", "12,50", "B", "op"),
                    Product("Bez recepty", null, null, "12,50"),
                },
                actualSeller: Seller("Apteka Farmaceutów"),
                actualDate: Date("2019-10-08", "2019-10-08 nr wydr.036720"),
                actualTaxNumber: TaxNumber("NIP 644-187-76-00"),
                actualReceiptAmount: Amount(25.20M, "SUMA PLN 25,20")),

            new TestReceipt(
                id: 16,
                lines: new []
                {
                    "\"Brawo\" Sp z o.o.",
                    "31-130 Kalwaria Zebrzydowska",
                    "Sklep RYŁKO DESIGNER OUTLET SOSNOWIEC",
                    "41-208 Sosnowiec, ul. Orląt Lwowskich 138",
                    "Nr rej. BDO:000118950",
                    "NIP: 551-21-85-838",
                    "13-05-2019 W042400",
                    "PARAGON FISKALNY",
                    "5902224690539 OBUWIE 8PH61 T1 VM6 39 1*169,99",
                    "169,99A",
                    "5907728785170 AKCESORIA RENOWATOR CZER 1*22,99",
                    "22,99A",
                    "SP.OP.A: 192.98 PTU 23% 36,09",
                    "Suma PTU 36,09",
                    "Suma: PLN 192,98",
                    "Stopka paragonu"
                },
                headerLines: new LinesRange(0, 7),
                productLines: new LinesRange(8, 11),
                taxesLines: new LinesRange(12, 13),
                amountLine: 14,
                footerLines: new LinesRange(15, 15),
                actualProducts: new IReceiptProduct[]
                {
                    Product("5902224690539 OBUWIE 8PH61 T1 VM6 39", "1", "169,99", "169,99", "A"),
                    Product("5907728785170 AKCESORIA RENOWATOR CZER", "1", "22,99", "22,99", "A"),
                },
                actualSeller: Seller("\"Brawo\" Sp z o.o."),
                actualDate: Date("13-05-2019", "13-05-2019 W042400"),
                actualTaxNumber: TaxNumber("NIP: 551-21-85-838"),
                actualReceiptAmount: Amount(192.98M, "Suma: PLN 192,98")),
        };

        private static ParsingResult<string> Seller(string rawLine)
        {
            return ParsingResult<string>.WithoutProblems(rawLine);
        }

        private static ParsingResult<TaxNumber> TaxNumber(string rawLine)
        {
            return rawLine == null
               ? ParsingResult<TaxNumber>.NotFound()
               : ParsingResult<TaxNumber>.WithoutProblems(
               value: TaxNumberRegex.TryMatch(rawLine).Match(
                   value => new TaxNumber(value),
                   () => throw new InvalidOperationException(
                       $"Cannot parse '{rawLine}' to taxnumber")),
               rawValue: rawLine);
        }

        private static ParsingResult<TaxNumber> TaxNumberWithPrefix(string rawLine)
        {
            return rawLine == null
                ? ParsingResult<TaxNumber>.NotFound()
                : ParsingResult<TaxNumber>.WithoutProblems(
                    value: TaxNumberRegex.TryMatchWithPrefix(rawLine).Match(
                        value => new TaxNumber(value),
                        () => throw new InvalidOperationException(
                            $"Cannot parse '{rawLine}' to taxnumber")),
                    rawValue: rawLine);
        }

        private static ParsingResult<DateTime> Date(string date, string rawLine)
        {
            return ParsingResult<DateTime>.WithoutProblems(
                value: DateRegex.TryMatch(date).Match(
                    value => value,
                    () => throw new InvalidOperationException(
                        $"Cannot parse '{date}' to date")),
                rawValue: rawLine);
        }

        private static ParsingResult<decimal> Amount(decimal amount, string rawLine)
        {
            return ParsingResult<decimal>.WithoutProblems(
                value: amount,
                rawValue: rawLine);
        }

        //TODO: that logic should probably not happen here, but product should be prepare more explicitly in expected data structures above
        private static RecognizedReceiptProduct Product(
            string name, 
            string quantity,
            string unitPrice, 
            string amount,
            string taxTag = null,
            string unit = null)
        {
            return new RecognizedReceiptProduct(
                name: ParsingResult<string>.WithoutProblems(name),

                quantity: quantity == null
                    ? ParsingResult<decimal>.NotFound()
                    : ParsingResult<decimal>.WithoutProblems(
                        StringAlgorithm
                            .ToDecimal(quantity)
                            .GetLeftOr(notANumber => throw new Exception($"'{notANumber.Text}' is not a decimal")),
                        quantity),

                unit: unit == null
                    ? ParsingResult<string>.NotFound()
                    : ParsingResult<string>.WithoutProblems(unit),

                unitPrice: unitPrice == null
                    ? ParsingResult<decimal>.NotFound()
                    : ParsingResult<decimal>.WithoutProblems(
                        StringAlgorithm
                            .ToDecimal(unitPrice)
                            .GetLeftOr(notANumber => throw new Exception($"'{notANumber.Text}' is not a decimal")),
                        unitPrice),

                amount: ParsingResult<decimal>.WithoutProblems(
                    StringAlgorithm
                        .ToDecimal(amount)
                        .GetLeftOr(notANumber => throw new Exception($"'{notANumber.Text}' is not a decimal")),
                    amount),

                taxTag: taxTag == null
                    ? ParsingResult<string>.NotFound()
                    : taxTag.Length == 1
                        ? ParsingResult<string>.WithoutProblems(taxTag)
                        : ParsingResult<string>.WithProblems(taxTag, taxTag, new[] {ParsingProblem.TooManyCharactersFound}),
                
                boundingBox: new BoundingBox(
                    new Point(0,0),
                    new Point(0,0),
                    new Point(0,0),
                    new Point(0,0))
            );
        }

        private static UnrecognizedReceiptProduct Unrecognized(string text)
        {
            return new UnrecognizedReceiptProduct(
                text:text, 
                boundingBox: new BoundingBox(
                    new Point(0, 0),
                    new Point(0, 0),
                    new Point(0, 0),
                    new Point(0, 0)));
        }

        static TestReceipts()
        {
            ThrowIfReceiptsIdsAreNotUnique();
        }

        private static void ThrowIfReceiptsIdsAreNotUnique()
        {
            var repeatedIds = AllReceipts
                .GroupBy(receipt => receipt.Id)
                .Where(group => @group.Count() > 1)
                .Select(group => @group.Key)
                .ToArray();

            if (repeatedIds.Any())
                throw new Exception(
                    $"Test receipts ids should be unique, following are not: {string.Join(", ", repeatedIds)}");
        }

        public static IEnumerable<object[]> Receipts()
        {
            return AllReceipts
                .Select(receipt => new[] {receipt})
                .ToArray();
        }

        public static ExpectedReceipt ExpectedReceipt(string fileName) =>
            ExpectedReceipts[fileName];
    }
}
