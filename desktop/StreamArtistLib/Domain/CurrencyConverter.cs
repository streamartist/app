using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Text.Json.Serialization;

namespace StreamArtist.Domain
{
    public class CurrencyConverter
    {

        static Dictionary<string, double> Currencies = new Dictionary<string, double>
        {
          { "USD",1D},
            { "EUR",1.09727193041945D},
            { "GBP",1.31183565951939D},
            { "INR",0.0119010182582076D},
            { "AUD",0.67956165512744D},
            { "CAD",0.736629958912061D},
            { "SGD",0.766415195368855D},
            { "CHF",1.16552119523326D},
            { "MYR",0.236986563186832D},
            { "JPY",0.00672378330063147D},
            { "CNY",0.142496601860363D},
            { "NZD",0.615844672500321D},
            { "THB",0.0300973466217575D},
            { "HUF",0.00273494623204106D},
            { "AED",0.272294077603812D},
            { "HKD",0.128758888707266D},
            { "MXN",0.0518882387840135D},
            { "ZAR",0.0572680897373046D},
            { "PHP",0.0177605267432466D},
            { "SEK",0.0964996871010024D},
            { "IDR",0.0000646598815958011D},
            { "BRL",0.18335541072127D},
            { "SAR",0.266666666666666D},
            { "TRY",0.0291910230308379D},
            { "KES",0.00775196737022082D},
            { "KRW",0.000742250049481155D},
            { "EGP",0.0206882200422503D},
            { "IQD",0.000763185577110177D},
            { "NOK",0.0938528978525691D},
            { "KWD",3.25520412841038D},
            { "RUB",0.0105069038368126D},
            { "DKK",0.147290544718658D},
            { "PKR",0.00360275095413146D},
            { "ILS",0.263363203590281D},
            { "PLN",0.254404735848502D},
            { "QAR",0.274725274725274D},
            { "XAU",2653.09393863069D},
            { "OMR",2.59808934897609D},
            { "COP",0.000239765312694695D},
            { "CLP",0.00108191501171666D},
            { "TWD",0.0309260510838023D},
            { "ARS",0.00102986199671621D},
            { "CZK",0.0433199495602057D},
            { "VND",0.0000403820836656399D},
            { "MAD",0.10181059064068D},
            { "JOD",1.41043723554301D},
            { "BHD",2.6595744680851D},
            { "XOF",0.00167278027434641D},
            { "LKR",0.00340603529640247D},
            { "UAH",0.0242022627745363D},
            { "NGN",0.000608202135386226D},
            { "TND",0.326398427652293D},
            { "UGX",0.00027278204268899D},
            { "RON",0.220528303731764D},
            { "BDT",0.0083393298667584D},
            { "PEN",0.267788964008884D},
            { "GEL",0.365577251621679D},
            { "XAF",0.00167278027434641D},
            { "FJD",0.452062308499276D},
            { "VEF",0.000000270347838096656D},
            { "VES",0.0270347838096656D},
            { "BYN",0.305343472347548D},
            { "UZS",0.0000784414176867732D},
            { "BGN",0.561026229487968D},
            { "DZD",0.00748413780034445D},
            { "IRR",0.0000238080548586201D},
            { "DOP",0.0166250652582394D},
            { "ISK",0.00737197554157902D},
            { "CRC",0.00192145692020843D},
            { "XAG",32.2258036188995D},
            { "SYP",0.0000769119980044708D},
            { "JMD",0.00633255444919702D},
            { "LYD",0.209753561890106D},
            { "GHS",0.0531557851627097D},
            { "MUR",0.0215424232410761D},
            { "AOA",0.00109895414098296D},
            { "UYU",0.0240183424658313D},
            { "AFN",0.0146272697525174D},
            { "LBP",0.0000110786514466724D},
            { "XPF",0.00919513877694866D},
            { "TTD",0.148721003262185D},
            { "TZS",0.000367813973667596D},
            { "ALL",0.0110801320795861D},
            { "XCD",0.370370363829386D},
            { "GTQ",0.129336419570532D},
            { "NPR",0.00743465141852732D},
            { "BOB",0.14440251774288D},
            { "ZWD",0.00276319425255595D},
            { "BBD",0.5D},
            { "CUC",1D},
            { "LAK",0.0000451370498181468D},
            { "BND",0.766415195368855D},
            { "BWP",0.0765690253349124D},
            { "HNL",0.0402161038056959D},
            { "PYG",0.000127959352213243D},
            { "ETB",0.00827057188862036D},
            { "NAD",0.0572680897373046D},
            { "PGK",0.254200452305566D},
            { "SDG",0.00166969264784488D},
            { "MOP",0.125008629812879D},
            { "BMD",1D},
            { "NIO",0.0273042549003123D},
            { "BAM",0.561026229487968D},
            { "KZT",0.00207718924330041D},
            { "PAB",1D},
            { "GYD",0.00478997925043904D},
            { "YER",0.00399728176221262D},
            { "MGA",0.000218879230577327D},
            { "KYD",1.2195063794557D},
            { "MZN",0.0157057104986875D},
            { "RSD",0.00942463632791699D},
            { "SCR",0.0669620057065499D},
            { "AMD",0.00259138651841088D},
            { "AZN",0.588226710647337D},
            { "SBD",0.120800492003837D},
            { "SLL",0.0000441890606764741D},
            { "TOP",0.433597331362615D},
            { "BZD",0.496229192437623D},
            { "GMD",0.0144929038077006D},
            { "MWK",0.000576909832230631D},
            { "BIF",0.000345768436016272D},
            { "HTG",0.00746389551925436D},
            { "SOS",0.00175112988196004D},
            { "GNF",0.000115927266365713D},
            { "MNT",0.000295241557805019D},
            { "MVR",0.0646830773112814D},
            { "CDF",0.000351190188347697D},
            { "STN",0.0447439107042485D},
            { "TJS",0.0939510726726403D},
            { "KPW",0.00111111111111111D},
            { "KGS",0.0118063774106286D},
            { "LRD",0.00517188085147701D},
            { "LSL",0.0572680897373046D},
            { "MMK",0.000477136448761631D},
            { "GIP",1.31183565951939D},
            { "XPT",991.469157027836D},
            { "MDL",0.0573641095569319D},
            { "CUP",0.0418097577348055D},
            { "KHR",0.000246923723974024D},
            { "MKD",0.0178030999657577D},
            { "VUV",0.00860207148159156D},
            { "ANG",0.559501795392929D},
            { "MRU",0.0251319726255272D},
            { "SZL",0.0572680897373046D},
            { "CVE",0.00995077473854586D},
            { "SRD",0.0322580753457711D},
            { "SVC",0.114285714285714D},
            { "XPD",960.00001110791D},
            { "BSD",1D},
            { "XDR",1.34421991878416D},
            { "RWF",0.000743983156005446D},
            { "AWG",0.558659217877094D},
            { "BTN",0.0119010182582076D},
            { "DJF",0.00559055305773251D},
            { "KMF",0.00223037369912855D},
            { "ERN",0.0666666666666666D},
            { "FKP",1.31183565951939D},
            { "SHP",1.31183565951939D},
            { "SPL",6.000000024D},
            { "WST",0.372800245446585D},
            { "JEP",1.31183565951939D},
            { "TMT",0.284906085333926D},
            { "GGP",1.31183565951939D},
            { "IMP",1.31183565951939D},
            { "TVD",0.67956165512744D},
            { "ZMW",0.0377382715365794D},
            { "ADA",0.353430615949577D},
            { "BCH",325.147113504806D},
            { "BTC",62747.4657771313D},
            { "CLF",41.0349875294187D},
            { "CNH",0.140882766438495D},
            { "DOGE",0.112811211384992D},
            { "DOT",4.18042761301972D},
            { "ETH",2449.69572862285D},
            { "LINK",11.302297254269D},
            { "LTC",67.4091713565661D},
            { "LUNA",0.38057899981147D},
            { "MXV",0.430532321710127D},
            { "SLE",0.0441890606764741D},
            { "UNI",6.95843333519877D},
            { "VED",0.0270347838096656D},
            { "XBT",62747.4657771313D},
            { "XLM",0.0931045477263217D},
            { "XRP",0.537514754561604D},
            { "ZWG",0.0709717108791415D}
        };

        public double GetUSD(string currency, double amount)
        {
            return amount / Currencies[currency];
        }
    }
}