using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

/// <summary>
/// Summary description for Global
/// </summary>
public static class Global
{
    public const string  CodeDateFormat = "MMMM/dd/yyyy";
    public const string GOOGLE_API_KEY = "AIzaSyAkGLRNMDfouo8TS3SaDsnPOX7w2doPKdk";
    public static string ConnectionString()
    {
        var cnB = new MySqlConnectionStringBuilder();
        cnB["Data Source"] = "198.71.225.52";
        cnB["Initial Catalog"] = "ninjatekweb_";
        cnB["UID"] = "ninjatekadmin";
        cnB["Password"] = "Snail@Task";
        cnB["Convert Zero Datetime"] = true;
        return cnB.ToString();
    }

    public static string GetDescription(Enum en)
    {
        Type type = en.GetType();
        MemberInfo[] memInfo = type.GetMember(en.ToString());
        if (memInfo != null && memInfo.Length > 0)
        {
            object[] attrs = memInfo[0].GetCustomAttributes(typeof(Description),
                                                            false);
            if (attrs != null && attrs.Length > 0)
                return ((Description)attrs[0]).Text;
        }

        return en.ToString();
    }
}