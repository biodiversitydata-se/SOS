using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Models
{ 
        /// <summary>
        /// Municipality
        /// </summary>
        public enum Municipality
        {
            /// <summary>
            /// Ale
            /// </summary>
            [EnumMember(Value = "Ale")]
            Ale = 0,
            /// <summary>
            /// Alingsås
            /// </summary>
            [EnumMember(Value = "Alingsås")]
            Alingsås = 1,
            /// <summary>
            ///  Alvesta
            /// </summary>
            [EnumMember(Value = "Alvesta")]
            Alvesta = 2,
            /// <summary>
            ///  Aneby
            /// </summary>
            [EnumMember(Value = "Aneby")]
            Aneby = 3,
            /// <summary>
            ///  Arboga
            /// </summary>
            [EnumMember(Value = "Arboga")]
            Arboga = 4,
            /// <summary>
            ///  Arjeplog
            /// </summary>
            [EnumMember(Value = "Arjeplog")]
            Arjeplog = 5,
            /// <summary>
            ///  Arvidsjaur
            /// </summary>
            [EnumMember(Value = "Arvidsjaur")]
            Arvidsjaur = 6,
            /// <summary>
            ///  Arvika
            /// </summary>
            [EnumMember(Value = "Arvika")]
            Arvika = 7,
            /// <summary>
            ///  Askersund
            /// </summary>
            [EnumMember(Value = "Askersund")]
            Askersund = 8,
            /// <summary>
            ///  Avesta
            /// </summary>
            [EnumMember(Value = "Avesta")]
            Avesta = 9,
            /// <summary>
            ///  Bengtsfors
            /// </summary>
            [EnumMember(Value = "Bengtsfors")]
            Bengtsfors = 10,
            /// <summary>
            ///  Berg
            /// </summary>
            [EnumMember(Value = "Berg")]
            Berg = 11,
            /// <summary>
            ///  Bjurholm
            /// </summary>
            [EnumMember(Value = "Bjurholm")]
            Bjurholm = 12,
            /// <summary>
            ///  Bjuv
            /// </summary>
            [EnumMember(Value = "Bjuv")]
            Bjuv = 13,
            /// <summary>
            ///  Boden
            /// </summary>
            [EnumMember(Value = "Boden")]
            Boden = 14,
            /// <summary>
            ///  Bollebygd
            /// </summary>
            [EnumMember(Value = "Bollebygd")]
            Bollebygd = 15,
            /// <summary>
            ///  Bollnäs
            /// </summary>
            [EnumMember(Value = "Bollnäs")]
            Bollnäs = 16,
            /// <summary>
            ///  Borgholm
            /// </summary>
            [EnumMember(Value = "Borgholm")]
            Borgholm = 17,
            /// <summary>
            ///  Borlänge
            /// </summary>
            [EnumMember(Value = "Borlänge")]
            Borlänge = 18,
            /// <summary>
            ///  Borås
            /// </summary>
            [EnumMember(Value = "Borås")]
            Borås = 19,
            /// <summary>
            /// Botkyrka
            /// </summary>
            [EnumMember(Value = "Botkyrka")]
            Botkyrka = 20,
            /// <summary>
            /// Boxholm
            /// </summary>
            [EnumMember(Value = "Boxholm")]
            Boxholm = 21,
            /// <summary>
            /// Bromölla
            /// </summary>
            [EnumMember(Value = "Bromölla")]
            Bromölla = 22,
            /// <summary>
            ///  Bräcke
            /// </summary>
            [EnumMember(Value = "Bräcke")]
            Bräcke = 23,
            /// <summary>
            /// Burlöv
            /// </summary>
            [EnumMember(Value = "Burlöv")]
            Burlöv = 24,
            /// <summary>
            /// Båstad
            /// </summary>
            [EnumMember(Value = "Båstad")]
            Båstad = 25,
            /// <summary>
            ///  Dals-Ed
            /// </summary>
            [EnumMember(Value = "Dals-Ed")]
            DalsEd = 26,
            /// <summary>
            ///  Danderyd
            /// </summary>
            [EnumMember(Value = "Danderyd")]
            Danderyd = 27,
            /// <summary>
            ///  Degerfors
            /// </summary>
            [EnumMember(Value = "Degerfors")]
            Degerfors = 28,
            /// <summary>
            ///  Dorotea
            /// </summary>
            [EnumMember(Value = "Dorotea")]
            Dorotea = 29,
            /// <summary>
            ///  Eda
            /// </summary>
            [EnumMember(Value = "Eda")]
            Eda = 30,
            /// <summary>
            ///  Ekerö
            /// </summary>
            [EnumMember(Value = "Ekerö")]
            Ekerö = 31,
            /// <summary>
            ///  Eksjö
            /// </summary>
            [EnumMember(Value = "Eksjö")]
            Eksjö = 32,
            /// <summary>
            ///  Emmaboda
            /// </summary>
            [EnumMember(Value = "Emmaboda")]
            Emmaboda = 33,
            /// <summary>
            ///  Enköping
            /// </summary>
            [EnumMember(Value = "Enköping")]
            Enköping = 34,
            /// <summary>
            /// Eskilstuna
            /// </summary>
            [EnumMember(Value = "Eskilstuna")]
            Eskilstuna = 35,
            /// <summary>
            /// Eslöv
            /// </summary>
            [EnumMember(Value = "Eslöv")]
            Eslöv = 36,
            /// <summary>
            /// Essunga
            /// </summary>
            [EnumMember(Value = "Essunga")]
            Essunga = 37,
            /// <summary>
            /// Fagersta
            /// </summary>
            [EnumMember(Value = "Fagersta")]
            Fagersta = 38,
            /// <summary>
            /// Falkenberg
            /// </summary>
            [EnumMember(Value = "Falkenberg")]
            Falkenberg = 39,
            /// <summary>
            /// Falköping
            /// </summary>
            [EnumMember(Value = "Falköping")]
            Falköping = 40,
            /// <summary>
            ///  Falun
            /// </summary>
            [EnumMember(Value = "Falun")]
            Falun = 41,
            /// <summary>
            ///  Filipstad
            /// </summary>
            [EnumMember(Value = "Filipstad")]
            Filipstad = 42,
            /// <summary>
            ///  Finspång
            /// </summary>
            [EnumMember(Value = "Finspång")]
            Finspång = 43,
            /// <summary>
            ///  Flen
            /// </summary>
            [EnumMember(Value = "Flen")]
            Flen = 44,
            /// <summary>
            ///  Forshaga
            /// </summary>
            [EnumMember(Value = "Forshaga")]
            Forshaga = 45,
            /// <summary>
            ///  Färgelanda
            /// </summary>
            [EnumMember(Value = "Färgelanda")]
            Färgelanda = 46,
            /// <summary>
            /// Gagnef
            /// </summary>
            [EnumMember(Value = "Gagnef")]
            Gagnef = 47,
            /// <summary>
            /// Gislaved
            /// </summary>
            [EnumMember(Value = "Gislaved")]
            Gislaved = 48,
            /// <summary>
            /// Gnesta
            /// </summary>
            [EnumMember(Value = "Gnesta")]
            Gnesta = 49,
            /// <summary>
            /// Gnosjö
            /// </summary>
            [EnumMember(Value = "Gnosjö")]
            Gnosjö = 50,
            /// <summary>
            /// Gotland
            /// </summary>
            [EnumMember(Value = "Gotland")]
            Gotland = 51,
            /// <summary>
            /// Grums
            /// </summary>
            [EnumMember(Value = "Grums")]
            Grums = 52,
            /// <summary>
            /// Grästorp
            /// </summary>
            [EnumMember(Value = "Grästorp")]
            Grästorp = 53,
            /// <summary>
            /// Gullspång
            /// </summary>
            [EnumMember(Value = "Gullspång")]
            Gullspång = 54,
            /// <summary>
            /// Gällivare
            /// </summary>
            [EnumMember(Value = "Gällivare")]
            Gällivare = 55,
            /// <summary>
            /// Gävle
            /// </summary>
            [EnumMember(Value = "Gävle")]
            Gävle = 56,
            /// <summary>
            /// Göteborg
            /// </summary>
            [EnumMember(Value = "Göteborg")]
            Göteborg = 57,
            /// <summary>
            /// Götene
            /// </summary>
            [EnumMember(Value = "Götene")]
            Götene = 58,
            /// <summary>
            /// Habo
            /// </summary>
            [EnumMember(Value = "Habo")]
            Habo = 59,
            /// <summary>
            /// Hagfors
            /// </summary>
            [EnumMember(Value = "Hagfors")]
            Hagfors = 60,
            /// <summary>
            /// Hallsberg
            /// </summary>
            [EnumMember(Value = "Hallsberg")]
            Hallsberg = 61,
            /// <summary>
            /// Hallstahammar
            /// </summary>
            [EnumMember(Value = "Hallstahammar")]
            Hallstahammar = 62,
            /// <summary>
            /// Halmstad
            /// </summary>
            [EnumMember(Value = "Halmstad")]
            Halmstad = 63,
            /// <summary>
            /// Hammarö
            /// </summary>
            [EnumMember(Value = "Hammarö")]
            Hammarö = 64,
            /// <summary>
            /// Haninge
            /// </summary>
            [EnumMember(Value = "Haninge")]
            Haninge = 65,
            /// <summary>
            /// Haparanda
            /// </summary>
            [EnumMember(Value = "Haparanda")]
            Haparanda = 66,
            /// <summary>
            /// Heby
            /// </summary>
            [EnumMember(Value = "Heby")]
            Heby = 67,
            /// <summary>
            /// Hedemora
            /// </summary>
            [EnumMember(Value = "Hedemora")]
            Hedemora = 68,
            /// <summary>
            /// Helsingborg
            /// </summary>
            [EnumMember(Value = "Helsingborg")]
            Helsingborg = 69,
            /// <summary>
            /// Herrljunga
            /// </summary>
            [EnumMember(Value = "Herrljunga")]
            Herrljunga = 70,
            /// <summary>
            /// Hjo
            /// </summary>
            [EnumMember(Value = "Hjo")]
            Hjo = 71,
            /// <summary>
            /// Hofors
            /// </summary>
            [EnumMember(Value = "Hofors")]
            Hofors = 72,
            /// <summary>
            /// Huddinge
            /// </summary>
            [EnumMember(Value = "Huddinge")]
            Huddinge = 73,
            /// <summary>
            /// Hudiksvall
            /// </summary>
            [EnumMember(Value = "Hudiksvall")]
            Hudiksvall = 74,
            /// <summary>
            /// Hultsfred
            /// </summary>
            [EnumMember(Value = "Hultsfred")]
            Hultsfred = 75,
            /// <summary>
            /// Hylte
            /// </summary>
            [EnumMember(Value = "Hylte")]
            Hylte = 76,
            /// <summary>
            /// Håbo
            /// </summary>
            [EnumMember(Value = "Håbo")]
            Håbo = 77,
            /// <summary>
            /// Hällefors
            /// </summary>
            [EnumMember(Value = "Hällefors")]
            Hällefors = 78,
            /// <summary>
            /// Härjedalen
            /// </summary>
            [EnumMember(Value = "Härjedalen")]
            Härjedalen = 79,
            /// <summary>
            /// Härnösand
            /// </summary>
            [EnumMember(Value = "Härnösand")]
            Härnösand = 80,
            /// <summary>
            /// Härryda
            /// </summary>
            [EnumMember(Value = "Härryda")]
            Härryda = 81,
            /// <summary>
            ///Hässleholm
            /// </summary>
            [EnumMember(Value = "Hässleholm")]
            Hässleholm = 82,
            /// <summary>
            /// Höganäs
            /// </summary>
            [EnumMember(Value = "Höganäs")]
            Höganäs = 83,
            /// <summary>
            /// Högsby
            /// </summary>
            [EnumMember(Value = "Högsby")]
            Högsby = 84,
            /// <summary>
            /// Hörby
            /// </summary>
            [EnumMember(Value = "Hörby")]
            Hörby = 85,
            /// <summary>
            /// Höör
            /// </summary>
            [EnumMember(Value = "Höör")]
            Höör = 86,
            /// <summary>
            /// Jokkmokk
            /// </summary>
            [EnumMember(Value = "Jokkmokk")]
            Jokkmokk = 87,
            /// <summary>
            /// Järfälla
            /// </summary>
            [EnumMember(Value = "Järfälla")]
            Järfälla = 88,
            /// <summary>
            /// Jönköping
            /// </summary>
            [EnumMember(Value = "Jönköping")]
            Jönköping = 89,
            /// <summary>
            /// Kalix
            /// </summary>
            [EnumMember(Value = "Kalix")]
            Kalix = 90,
            /// <summary>
            /// Kalmar
            /// </summary>
            [EnumMember(Value = "Kalmar")]
            Kalmar = 91,
            /// <summary>
            /// Karlsborg
            /// </summary>
            [EnumMember(Value = "Karlsborg")]
            Karlsborg = 92,
            /// <summary>
            /// Karlshamn
            /// </summary>
            [EnumMember(Value = "Karlshamn")]
            Karlshamn = 93,
            /// <summary>
            /// Karlskoga
            /// </summary>
            [EnumMember(Value = "Karlskoga")]
            Karlskoga = 94,
            /// <summary>
            /// Karlskrona
            /// </summary>
            [EnumMember(Value = "Karlskrona")]
            Karlskrona = 95,
            /// <summary>
            /// Karlstad
            /// </summary>
            [EnumMember(Value = "Karlstad")]
            Karlstad = 96,
            /// <summary>
            /// Katrineholm
            /// </summary>
            [EnumMember(Value = "Katrineholm")]
            Katrineholm = 97,
            /// <summary>
            /// Kil
            /// </summary>
            [EnumMember(Value = "Kil")]
            Kil = 98,
            /// <summary>
            /// Kinda
            /// </summary>
            [EnumMember(Value = "Kinda")]
            Kinda = 99,
            /// <summary>
            /// Kiruna
            /// </summary>
            [EnumMember(Value = "Kiruna")]
            Kiruna = 100,
            /// <summary>
            /// Klippan
            /// </summary>
            [EnumMember(Value = "Klippan")]
            Klippan = 101,
            /// <summary>
            /// Knivsta
            /// </summary>
            [EnumMember(Value = "Knivsta")]
            Knivsta = 102,
            /// <summary>
            /// Kramfors
            /// </summary>
            [EnumMember(Value = "Kramfors")]
            Kramfors = 103,
            /// <summary>
            /// Kristianstad
            /// </summary>
            [EnumMember(Value = "Kristianstad")]
            Kristianstad = 104,
            /// <summary>
            /// Kristinehamn
            /// </summary>
            [EnumMember(Value = "Kristinehamn")]
            Kristinehamn = 105,
            /// <summary>
            /// Krokom
            /// </summary>
            [EnumMember(Value = "Krokom")]
            Krokom = 106,
            /// <summary>
            /// Kumla
            /// </summary>
            [EnumMember(Value = "Kumla")]
            Kumla = 107,
            /// <summary>
            /// Kungsbacka
            /// </summary>
            [EnumMember(Value = "Kungsbacka")]
            Kungsbacka = 108,
            /// <summary>
            /// Kungsor for Kungsör
            /// </summary>
            [EnumMember(Value = "Kungsör")]
            Kungsör = 109,
            /// <summary>
            /// Kungälv
            /// </summary>
            [EnumMember(Value = "Kungälv")]
            Kungälv = 110,
            /// <summary>
            /// Kävlinge
            /// </summary>
            [EnumMember(Value = "Kävlinge")]
            Kävlinge = 111,
            /// <summary>
            /// Köping
            /// </summary>
            [EnumMember(Value = "Köping")]
            Köping = 112,
            /// <summary>
            /// Laholm
            /// </summary>
            [EnumMember(Value = "Laholm")]
            Laholm = 113,
            /// <summary>
            /// Landskrona
            /// </summary>
            [EnumMember(Value = "Landskrona")]
            Landskrona = 114,
            /// <summary>
            /// Laxå
            /// </summary>
            [EnumMember(Value = "Laxå")]
            Laxå = 115,
            /// <summary>
            /// Lekeberg
            /// </summary>
            [EnumMember(Value = "Lekeberg")]
            Lekeberg = 116,
            /// <summary>
            /// Leksand
            /// </summary>
            [EnumMember(Value = "Leksand")]
            Leksand = 117,
            /// <summary>
            /// Lerum
            /// </summary>
            [EnumMember(Value = "Lerum")]
            Lerum = 118,
            /// <summary>
            /// Lessebo
            /// </summary>
            [EnumMember(Value = "Lessebo")]
            Lessebo = 119,
            /// <summary>
            /// Lidingö
            /// </summary>
            [EnumMember(Value = "Lidingö")]
            Lidingö = 120,
            /// <summary>
            /// Lidköping
            /// </summary>
            [EnumMember(Value = "Lidköping")]
            Lidköping = 121,
            /// <summary>
            /// Lilla Edet
            /// </summary>
            [EnumMember(Value = "Lilla Edet")]
            LillaEdet = 122,
            /// <summary>
            /// Lindesberg
            /// </summary>
            [EnumMember(Value = "Lindesberg")]
            Lindesberg = 123,
            /// <summary>
            /// Linköping
            /// </summary>
            [EnumMember(Value = "Linköping")]
            Linköping = 124,
            /// <summary>
            /// Ljungby
            /// </summary>
            [EnumMember(Value = "Ljungby")]
            Ljungby = 125,
            /// <summary>
            /// Ljusdal
            /// </summary>
            [EnumMember(Value = "Ljusdal")]
            Ljusdal = 126,
            /// <summary>
            /// Ljusnarsberg
            /// </summary>
            [EnumMember(Value = "Ljusnarsberg")]
            Ljusnarsberg = 127,
            /// <summary>
            /// Lomma
            /// </summary>
            [EnumMember(Value = "Lomma")]
            Lomma = 128,
            /// <summary>
            /// Ludvika
            /// </summary>
            [EnumMember(Value = "Ludvika")]
            Ludvika = 129,
            /// <summary>
            /// Luleå
            /// </summary>
            [EnumMember(Value = "Luleå")]
            Luleå = 130,
            /// <summary>
            /// Lund
            /// </summary>
            [EnumMember(Value = "Lund")]
            Lund = 131,
            /// <summary>
            /// Lycksele
            /// </summary>
            [EnumMember(Value = "Lycksele")]
            Lycksele = 132,
            /// <summary>
            /// Lysekil
            /// </summary>
            [EnumMember(Value = "Lysekil")]
            Lysekil = 133,
            /// <summary>
            /// Malmö
            /// </summary>
            [EnumMember(Value = "Malmö")]
            Malmö = 134,
            /// <summary>
            /// Malung-Sälen
            /// </summary>
            [EnumMember(Value = "Malung-Sälen")]
            MalungSälen = 135,
            /// <summary>
            /// Malå
            /// </summary>
            [EnumMember(Value = "Malå")]
            Malå = 136,
            /// <summary>
            /// Mariestad
            /// </summary>
            [EnumMember(Value = "Mariestad")]
            Mariestad = 137,
            /// <summary>
            /// Mark
            /// </summary>
            [EnumMember(Value = "Mark")]
            Mark = 138,
            /// <summary>
            /// Markaryd
            /// </summary>
            [EnumMember(Value = "Markaryd")]
            Markaryd = 139,
            /// <summary>
            /// Mellerud
            /// </summary>
            [EnumMember(Value = "Mellerud")]
            Mellerud = 140,
            /// <summary>
            /// Mjölby
            /// </summary>
            [EnumMember(Value = "Mjölby")]
            Mjölby = 141,
            /// <summary>
            /// Mora
            /// </summary>
            [EnumMember(Value = "Mora")]
            Mora = 142,
            /// <summary>
            /// Motala
            /// </summary>
            [EnumMember(Value = "Motala")]
            Motala = 143,
            /// <summary>
            /// Mullsjö
            /// </summary>
            [EnumMember(Value = "Mullsjö")]
            Mullsjö = 144,
            /// <summary>
            /// Munkedal
            /// </summary>
            [EnumMember(Value = "Munkedal")]
            Munkedal = 145,
            /// <summary>
            /// Munkfors
            /// </summary>
            [EnumMember(Value = "Munkfors")]
            Munkfors = 146,
            /// <summary>
            /// Mölndal
            /// </summary>
            [EnumMember(Value = "Mölndal")]
            Mölndal = 147,
            /// <summary>
            /// Mönsterås
            /// </summary>
            [EnumMember(Value = "Mönsterås")]
            Mönsterås = 148,
            /// <summary>
            /// Mörbylånga
            /// </summary>
            [EnumMember(Value = "Mörbylånga")]
            Mörbylånga = 149,
            /// <summary>
            /// Nacka
            /// </summary>
            [EnumMember(Value = "Nacka")]
            Nacka = 150,
            /// <summary>
            /// Nora
            /// </summary>
            [EnumMember(Value = "Nora")]
            Nora = 151,
            /// <summary>
            /// Norberg
            /// </summary>
            [EnumMember(Value = "Norberg")]
            Norberg = 152,
            /// <summary>
            /// Nordanstig
            /// </summary>
            [EnumMember(Value = "Nordanstig")]
            Nordanstig = 153,
            /// <summary>
            /// Nordmaling
            /// </summary>
            [EnumMember(Value = "Nordmaling")]
            Nordmaling = 154,
            /// <summary>
            /// Norrköping
            /// </summary>
            [EnumMember(Value = "Norrköping")]
            Norrköping = 155,
            /// <summary>
            /// Norrtälje
            /// </summary>
            [EnumMember(Value = "Norrtälje")]
            Norrtälje = 156,
            /// <summary>
            /// Norsjö
            /// </summary>
            [EnumMember(Value = "Norsjö")]
            Norsjö = 157,
            /// <summary>
            /// Nybro
            /// </summary>
            [EnumMember(Value = "Nybro")]
            Nybro = 158,
            /// <summary>
            /// Nykvarn
            /// </summary>
            [EnumMember(Value = "Nykvarn")]
            Nykvarn = 159,
            /// <summary>
            /// Nyköping
            /// </summary>
            [EnumMember(Value = "Nyköping")]
            Nyköping = 160,
            /// <summary>
            /// Nynäshamn
            /// </summary>
            [EnumMember(Value = "Nynäshamn")]
            Nynäshamn = 161,
            /// <summary>
            /// Nässjö
            /// </summary>
            [EnumMember(Value = "Nässjö")]
            Nässjö = 162,
            /// <summary>
            /// Ockelbo
            /// </summary>
            [EnumMember(Value = "Ockelbo")]
            Ockelbo = 163,
            /// <summary>
            /// Olofström
            /// </summary>
            [EnumMember(Value = "Olofström")]
            Olofström = 164,
            /// <summary>
            /// Orsa
            /// </summary>
            [EnumMember(Value = "Orsa")]
            Orsa = 165,
            /// <summary>
            /// Orust
            /// </summary>
            [EnumMember(Value = "Orust")]
            Orust = 166,
            /// <summary>
            /// Osby
            /// </summary>
            [EnumMember(Value = "Osby")]
            Osby = 167,
            /// <summary>
            /// Oskarshamn
            /// </summary>
            [EnumMember(Value = "Oskarshamn")]
            Oskarshamn = 168,
            /// <summary>
            /// Ovanåker
            /// </summary>
            [EnumMember(Value = "Ovanåker")]
            Ovanåker = 169,
            /// <summary>
            /// Oxelösund
            /// </summary>
            [EnumMember(Value = "Oxelösund")]
            Oxelösund = 170,
            /// <summary>
            /// Pajala
            /// </summary>
            [EnumMember(Value = "Pajala")]
            Pajala = 171,
            /// <summary>
            /// Partille
            /// </summary>
            [EnumMember(Value = "Partille")]
            Partille = 172,
            /// <summary>
            /// Perstorp
            /// </summary>
            [EnumMember(Value = "Perstorp")]
            Perstorp = 173,
            /// <summary>
            /// Piteå
            /// </summary>
            [EnumMember(Value = "Piteå")]
            Piteå = 174,
            /// <summary>
            /// Ragunda
            /// </summary>
            [EnumMember(Value = "Ragunda")]
            Ragunda = 175,
            /// <summary>
            /// Robertsfors
            /// </summary>
            [EnumMember(Value = "Robertsfors")]
            Robertsfors = 176,
            /// <summary>
            /// Ronneby
            /// </summary>
            [EnumMember(Value = "Ronneby")]
            Ronneby = 177,
            /// <summary>
            /// Rättvik
            /// </summary>
            [EnumMember(Value = "Rättvik")]
            Rättvik = 178,
            /// <summary>
            /// Sala
            /// </summary>
            [EnumMember(Value = "Sala")]
            Sala = 179,
            /// <summary>
            /// Salem
            /// </summary>
            [EnumMember(Value = "Salem")]
            Salem = 180,
            /// <summary>
            /// Sandviken
            /// </summary>
            [EnumMember(Value = "Sandviken")]
            Sandviken = 181,
            /// <summary>
            /// Sigtuna
            /// </summary>
            [EnumMember(Value = "Sigtuna")]
            Sigtuna = 182,
            /// <summary>
            /// Simrishamn
            /// </summary>
            [EnumMember(Value = "Simrishamn")]
            Simrishamn = 183,
            /// <summary>
            /// Sjöbo
            /// </summary>
            [EnumMember(Value = "Sjöbo")]
            Sjöbo = 184,
            /// <summary>
            /// Skara
            /// </summary>
            [EnumMember(Value = "Skara")]
            Skara = 185,
            /// <summary>
            /// Skellefteå
            /// </summary>
            [EnumMember(Value = "Skellefteå")]
            Skellefteå = 186,
            /// <summary>
            /// Skinnskatteberg
            /// </summary>
            [EnumMember(Value = "Skinnskatteberg")]
            Skinnskatteberg = 187,
            /// <summary>
            /// Skurup
            /// </summary>
            [EnumMember(Value = "Skurup")]
            Skurup = 188,
            /// <summary>
            /// Skövde
            /// </summary>
            [EnumMember(Value = "Skövde")]
            Skövde = 189,
            /// <summary>
            /// Smedjebacken
            /// </summary>
            [EnumMember(Value = "Smedjebacken")]
            Smedjebacken = 190,
            /// <summary>
            /// Sollefteå
            /// </summary>
            [EnumMember(Value = "Sollefteå")]
            Sollefteå = 191,
            /// <summary>
            /// Sollentuna
            /// </summary>
            [EnumMember(Value = "Sollentuna")]
            Sollentuna = 192,
            /// <summary>
            /// Solna
            /// </summary>
            [EnumMember(Value = "Solna")]
            Solna = 193,
            /// <summary>
            /// Sorsele
            /// </summary>
            [EnumMember(Value = "Sorsele")]
            Sorsele = 194,
            /// <summary>
            /// Sotenäs
            /// </summary>
            [EnumMember(Value = "Sotenäs")]
            Sotenäs = 195,
            /// <summary>
            /// Staffanstorp
            /// </summary>
            [EnumMember(Value = "Staffanstorp")]
            Staffanstorp = 196,
            /// <summary>
            /// Stenungsund
            /// </summary>
            [EnumMember(Value = "Stenungsund")]
            Stenungsund = 197,
            /// <summary>
            /// Stockholm
            /// </summary>
            [EnumMember(Value = "Stockholm")]
            Stockholm = 198,
            /// <summary>
            /// Storfors
            /// </summary>
            [EnumMember(Value = "Storfors")]
            Storfors = 199,
            /// <summary>
            /// Storuman
            /// </summary>
            [EnumMember(Value = "Storuman")]
            Storuman = 200,
            /// <summary>
            /// Strängnäs
            /// </summary>
            [EnumMember(Value = "Strängnäs")]
            Strängnäs = 201,
            /// <summary>
            /// Strömstad
            /// </summary>
            [EnumMember(Value = "Strömstad")]
            Strömstad = 202,
            /// <summary>
            /// Strömsund
            /// </summary>
            [EnumMember(Value = "Strömsund")]
            Strömsund = 203,
            /// <summary>
            /// Sundbyberg
            /// </summary>
            [EnumMember(Value = "Sundbyberg")]
            Sundbyberg = 204,
            /// <summary>
            /// Sundsvall
            /// </summary>
            [EnumMember(Value = "Sundsvall")]
            Sundsvall = 205,
            /// <summary>
            /// Sunne
            /// </summary>
            [EnumMember(Value = "Sunne")]
            Sunne = 206,
            /// <summary>
            /// Surahammar
            /// </summary>
            [EnumMember(Value = "Surahammar")]
            Surahammar = 207,
            /// <summary>
            /// Svalöv
            /// </summary>
            [EnumMember(Value = "Svalöv")]
            Svalöv = 208,
            /// <summary>
            /// Svedala
            /// </summary>
            [EnumMember(Value = "Svedala")]
            Svedala = 209,
            /// <summary>
            /// Svenljunga
            /// </summary>
            [EnumMember(Value = "Svenljunga")]
            Svenljunga = 210,
            /// <summary>
            /// Säffle
            /// </summary>
            [EnumMember(Value = "Säffle")]
            Säffle = 211,
            /// <summary>
            /// Säter
            /// </summary>
            [EnumMember(Value = "Säter")]
            Säter = 212,
            /// <summary>
            /// Sävsjö
            /// </summary>
            [EnumMember(Value = "Sävsjö")]
            Sävsjö = 213,
            /// <summary>
            /// Söderhamn
            /// </summary>
            [EnumMember(Value = "Söderhamn")]
            Söderhamn = 214,
            /// <summary>
            /// Söderköping
            /// </summary>
            [EnumMember(Value = "Söderköping")]
            Söderköping = 215,
            /// <summary>
            /// Södertälje
            /// </summary>
            [EnumMember(Value = "Södertälje")]
            Södertälje = 216,
            /// <summary>
            /// Sölvesborg
            /// </summary>
            [EnumMember(Value = "Sölvesborg")]
            Sölvesborg = 217,
            /// <summary>
            /// Tanum
            /// </summary>
            [EnumMember(Value = "Tanum")]
            Tanum = 218,
            /// <summary>
            /// Tibro
            /// </summary>
            [EnumMember(Value = "Tibro")]
            Tibro = 219,
            /// <summary>
            /// Tidaholm
            /// </summary>
            [EnumMember(Value = "Tidaholm")]
            Tidaholm = 220,
            /// <summary>
            /// Tierp
            /// </summary>
            [EnumMember(Value = "Tierp")]
            Tierp = 221,
            /// <summary>
            /// Timrå
            /// </summary>
            [EnumMember(Value = "Timrå")]
            Timrå = 222,
            /// <summary>
            /// Tingsryd
            /// </summary>
            [EnumMember(Value = "Tingsryd")]
            Tingsryd = 223,
            /// <summary>
            /// Tjörn
            /// </summary>
            [EnumMember(Value = "Tjörn")]
            Tjörn = 224,
            /// <summary>
            /// Tomelilla
            /// </summary>
            [EnumMember(Value = "Tomelilla")]
            Tomelilla = 225,
            /// <summary>
            /// Torsby
            /// </summary>
            [EnumMember(Value = "Torsby")]
            Torsby = 226,
            /// <summary>
            /// Torsås
            /// </summary>
            [EnumMember(Value = "Torsås")]
            Torsås = 227,
            /// <summary>
            /// Tranemo
            /// </summary>
            [EnumMember(Value = "Tranemo")]
            Tranemo = 228,
            /// <summary>
            /// Tranås
            /// </summary>
            [EnumMember(Value = "Tranås")]
            Tranås = 229,
            /// <summary>
            /// Trelleborg
            /// </summary>
            [EnumMember(Value = "Trelleborg")]
            Trelleborg = 230,
            /// <summary>
            /// Trollhättan
            /// </summary>
            [EnumMember(Value = "Trollhättan")]
            Trollhättan = 231,
            /// <summary>
            /// Trosa
            /// </summary>
            [EnumMember(Value = "Trosa")]
            Trosa = 232,
            /// <summary>
            /// Tyresö
            /// </summary>
            [EnumMember(Value = "Tyresö")]
            Tyresö = 233,
            /// <summary>
            /// Täby
            /// </summary>
            [EnumMember(Value = "Täby")]
            Täby = 234,
            /// <summary>
            /// Töreboda
            /// </summary>
            [EnumMember(Value = "Töreboda")]
            Töreboda = 235,
            /// <summary>
            /// Uddevalla
            /// </summary>
            [EnumMember(Value = "Uddevalla")]
            Uddevalla = 236,
            /// <summary>
            /// Ulricehamn
            /// </summary>
            [EnumMember(Value = "Ulricehamn")]
            Ulricehamn = 237,
            /// <summary>
            /// Umeå
            /// </summary>
            [EnumMember(Value = "Umeå")]
            Umeå = 238,
            /// <summary>
            /// Upplands Väsby
            /// </summary>
            [EnumMember(Value = "Upplands Väsby")]
            UpplandsVäsby = 239,
            /// <summary>
            /// Upplands-Bro
            /// </summary>
            [EnumMember(Value = "Upplands-Bro")]
            UpplandsBro = 240,
            /// <summary>
            /// Uppsala
            /// </summary>
            [EnumMember(Value = "Uppsala")]
            Uppsala = 241,
            /// <summary>
            /// Uppvidinge
            /// </summary>
            [EnumMember(Value = "Uppvidinge")]
            Uppvidinge = 242,
            /// <summary>
            /// Vadstena
            /// </summary>
            [EnumMember(Value = "Vadstena")]
            Vadstena = 243,
            /// <summary>
            /// Vaggeryd
            /// </summary>
            [EnumMember(Value = "Vaggeryd")]
            Vaggeryd = 244,
            /// <summary>
            /// Valdemarsvik
            /// </summary>
            [EnumMember(Value = "Valdemarsvik")]
            Valdemarsvik = 245,
            /// <summary>
            /// Vallentuna
            /// </summary>
            [EnumMember(Value = "Vallentuna")]
            Vallentuna = 246,
            /// <summary>
            /// Vansbro
            /// </summary>
            [EnumMember(Value = "Vansbro")]
            Vansbro = 247,
            /// <summary>
            /// Vara
            /// </summary>
            [EnumMember(Value = "Vara")]
            Vara = 248,
            /// <summary>
            /// Varberg
            /// </summary>
            [EnumMember(Value = "Varberg")]
            Varberg = 249,
            /// <summary>
            /// Vaxholm
            /// </summary>
            [EnumMember(Value = "Vaxholm")]
            Vaxholm = 250,
            /// <summary>
            /// Vellinge
            /// </summary>
            [EnumMember(Value = "Vellinge")]
            Vellinge = 251,
            /// <summary>
            /// Vetlanda
            /// </summary>
            [EnumMember(Value = "Vetlanda")]
            Vetlanda = 252,
            /// <summary>
            /// Vilhelmina
            /// </summary>
            [EnumMember(Value = "Vilhelmina")]
            Vilhelmina = 253,
            /// <summary>
            /// Vimmerby
            /// </summary>
            [EnumMember(Value = "Vimmerby")]
            Vimmerby = 254,
            /// <summary>
            /// Vindeln
            /// </summary>
            [EnumMember(Value = "Vindeln")]
            Vindeln = 255,
            /// <summary>
            /// Vingåker
            /// </summary>
            [EnumMember(Value = "Vingåker")]
            Vingåker = 256,
            /// <summary>
            /// Vårgårda
            /// </summary>
            [EnumMember(Value = "Vårgårda")]
            Vårgårda = 257,
            /// <summary>
            /// Vänersborg
            /// </summary>
            [EnumMember(Value = "Vänersborg")]
            Vänersborg = 258,
            /// <summary>
            /// Vännäs
            /// </summary>
            [EnumMember(Value = "Vännäs")]
            Vännäs = 259,
            /// <summary>
            /// Värmdö
            /// </summary>
            [EnumMember(Value = "Värmdö")]
            Värmdö = 260,
            /// <summary>
            /// Värnamo
            /// </summary>
            [EnumMember(Value = "Värnamo")]
            Värnamo = 261,
            /// <summary>
            /// Västervik
            /// </summary>
            [EnumMember(Value = "Västervik")]
            Västervik = 262,
            /// <summary>
            /// Västerås
            /// </summary>
            [EnumMember(Value = "Västerås")]
            Västerås = 263,
            /// <summary>
            /// Växjö
            /// </summary>
            [EnumMember(Value = "Växjö")]
            Växjö = 264,
            /// <summary>
            /// Ydre
            /// </summary>
            [EnumMember(Value = "Ydre")]
            Ydre = 265,
            /// <summary>
            /// Ystad
            /// </summary>
            [EnumMember(Value = "Ystad")]
            Ystad = 266,
            /// <summary>
            /// Åmål
            /// </summary>
            [EnumMember(Value = "Åmål")]
            Åmål = 267,
            /// <summary>
            /// Ånge
            /// </summary>
            [EnumMember(Value = "Ånge")]
            Ånge = 268,
            /// <summary>
            /// Åre
            /// </summary>
            [EnumMember(Value = "Åre")]
            Åre = 269,
            /// <summary>
            /// Årjäng
            /// </summary>
            [EnumMember(Value = "Årjäng")]
            Årjäng = 270,
            /// <summary>
            /// Åsele
            /// </summary>
            [EnumMember(Value = "Åsele")]
            Åsele = 271,
            /// <summary>
            /// Åstorp
            /// </summary>
            [EnumMember(Value = "Åstorp")]
            Åstorp = 272,
            /// <summary>
            /// Åtvidaberg
            /// </summary>
            [EnumMember(Value = "Åtvidaberg")]
            Åtvidaberg = 273,
            /// <summary>
            /// Älmhult
            /// </summary>
            [EnumMember(Value = "Älmhult")]
            Älmhult = 274,
            /// <summary>
            /// Älvdalen
            /// </summary>
            [EnumMember(Value = "Älvdalen")]
            Älvdalen = 275,
            /// <summary>
            /// Älvkarleby
            /// </summary>
            [EnumMember(Value = "Älvkarleby")]
            Älvkarleby = 276,
            /// <summary>
            /// Älvsbyn
            /// </summary>
            [EnumMember(Value = "Älvsbyn")]
            Älvsbyn = 277,
            /// <summary>
            /// Ängelholm
            /// </summary>
            [EnumMember(Value = "Ängelholm")]
            Ängelholm = 278,
            /// <summary>
            /// Öckerö
            /// </summary>
            [EnumMember(Value = "Öckerö")]
            Öckerö = 279,
            /// <summary>
            /// Ödeshög
            /// </summary>
            [EnumMember(Value = "Ödeshög")]
            Ödeshög = 280,
            /// <summary>
            /// Örebro
            /// </summary>
            [EnumMember(Value = "Örebro")]
            Örebro = 281,
            /// <summary>
            /// Örkelljunga
            /// </summary>
            [EnumMember(Value = "Örkelljunga")]
            Örkelljunga = 282,
            /// <summary>
            /// Örnsköldsvik
            /// </summary>
            [EnumMember(Value = "Örnsköldsvik")]
            Örnsköldsvik = 283,
            /// <summary>
            /// Östersund
            /// </summary>
            [EnumMember(Value = "Östersund")]
            Östersund = 284,
            /// <summary>
            /// Österåker
            /// </summary>
            [EnumMember(Value = "Österåker")]
            Österåker = 285,
            /// <summary>
            /// Östhammar
            /// </summary>
            [EnumMember(Value = "Östhammar")]
            Östhammar = 286,
            /// <summary>
            /// Östra Göinge
            /// </summary>
            [EnumMember(Value = "Östra Göinge")]
            ÖstraGöinge = 287,
            /// <summary>
            /// Överkalix
            /// </summary>
            [EnumMember(Value = "Överkalix")]
            Överkalix = 288,
            /// <summary>
            /// Övertorneå
            /// </summary>
            [EnumMember(Value = "Övertorneå")]
            Övertorneå = 289
        }
}
