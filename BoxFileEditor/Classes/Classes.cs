using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SprávceŠablonOCR.Classes
{
    public class Settings
    {
        public List<string> languages { get; set; }
        public DateTime lastLogin { get; set; }
        //public string pcname { get; set; }
        //public string username { get; set; }
        //public string user_id { get; set; }
        //public int status { get; set; }
    }

    public class apiSettings
    {
        public string serverIP { get; set; }
        public string loginUrl { get; set; }
        public string refreshUrl { get; set; }
        public string logoutUrl { get; set; }
        public string userInfoUrl { get; set; }
        public string definitionUrl { get; set; }
        public string templateUrl { get; set; }
        public bool writeToLog { get; set; }
    }

    public class apiTokenRequest
    {
        public string username { get; set; }
        public string password { get; set; }
        public long salt { get; set; }

    }

    public class apiBearerToken
    {
        public string token_type { get; set; }
        public string bearerToken { get; set; }
    }

    public class apiTokenRefreshRequest
    {
        public bool refresh { get; set; }
    }

    public class apiTokenRefreshResponse
    {
        public bool success { get; set; }
        public DateTime expires_at { get; set; }
    }

    public class apiTokenResponse
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
        public long expires_in { get; set; }
    }

    public class userInfoResponse
    {
        public int id { get; set; }
        public string userName { get; set; }
        public int id_skoly { get; set; }
        public int role { get; set; }
    }

    public class templateListResponse
    {
        public int id_ocr_sablony { get; set; }
        public string nazev { get; set; }
        public string typ_dokladu { get; set; }
        public int id_typu_dokladu { get; set; }
        public string poznamky { get; set; }

    }


    public class Moznosti
    {
        public string id { get; set; }
        public string hodnota { get; set; }
    }

    public class Polozky2
    {
        public string nazev { get; set; }
        public string hodnota { get; set; }
        public string typ { get; set; }
        public string id_doklady_definice { get; set; }
        public IList<Moznosti> moznosti { get; set; }
    }

    public class Polozky
    {
        public string nazev { get; set; }
        public string hodnota { get; set; }
        public string typ { get; set; }
        public string id_doklady_definice { get; set; }
        public IList<Polozky2> polozky { get; set; }
    }

    public class DefinitionHeaderResponse
    {
        public string nazev { get; set; }
        public string hodnota { get; set; }
        public string typ { get; set; }
        public string id_doklady_definice { get; set; }
        public IList<Polozky> polozky { get; set; }
    }


    public class DefinitionTemplateSaveRequest
    {
        public string id_typu_dokladu { get; set; }
        public string nazev { get; set; }
        public string poznamky { get; set; }
        public string data { get; set; }
        public string data_header { get; set; }
    }

    public class TemplateHeaderData
    {
        public string identDoc { get; set; }
        public string sizeX { get; set; }
        public string sizeY { get; set; }

    }

    public class DefinitionTemplateEditRequest
    {
        public string nazev { get; set; }
        public string poznamky { get; set; }
        public string data { get; set; }
        public string data_header { get; set; }
    }

    public class DefinitionTemplateSaveResponse
    {
        public int id_doklady_definice { get; set; }
        public string id_typu_dokladu { get; set; }
        public string nazev { get; set; }
        public string poznamky { get; set; }
        public string data { get; set; }
    }

    public class BoolResponse
    {
        public bool result { get; set; }
    }

    public class DefinitionTemplateList
    {
        public int id_ocr_sablony { get; set; }
        public int id_typu_dokladu { get; set; }
        public string typ_dokladu { get; set; }
        public string nazev { get; set; }
        public string poznamky { get; set; }
        public string data_header { get; set; }

    }

    public class DataItem
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public string GroupValueIndex { get; set; }
        public string GroupValue { get; set; }
        public string GroupSubValueIndex { get; set; }
        public string GroupSubValue { get; set; }
    }
       
  
}